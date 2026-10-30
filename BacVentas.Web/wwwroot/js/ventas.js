// Pantalla de registro de ventas.
// Los cálculos (subtotal, IVA, total) se hacen aquí para mostrarlos al instante,
// pero el servidor (SP_VENTAS_REGISTRAR) los VUELVE a calcular con los precios de la BD:
// lo que se ve en pantalla es informativo, lo que se guarda es lo que calcula SQL Server.

const TASA_IVA = 0.13;   // IVA El Salvador

const config = document.getElementById('config').dataset;
const inputCodigo = document.getElementById('codigo');
const inputNombre = document.getElementById('nombreProducto');
const inputPrecio = document.getElementById('precio');
const inputCantidad = document.getElementById('cantidad');
const btnBuscar = document.getElementById('btnBuscar');
const btnAgregar = document.getElementById('btnAgregar');
const btnRegistrar = document.getElementById('btnRegistrar');
const btnLimpiar = document.getElementById('btnLimpiar');
const tbody = document.querySelector('#tablaDetalle tbody');

let productoActual = null;   // producto encontrado en la búsqueda
let detalle = [];            // líneas de la venta: { idPro, nombre, precio, cantidad }

// ---------- Búsqueda de producto ----------
async function buscarProducto() {
    const codigo = parseInt(inputCodigo.value, 10);
    limpiarProducto();
    if (!codigo || codigo < 1) {
        Swal.fire({ icon: 'warning', title: 'Código inválido', text: 'Ingrese un código de producto.' });
        return;
    }

    try {
        const resp = await fetch(`${config.buscarUrl}/${codigo}`);
        const data = await resp.json();
        if (!resp.ok) {
            Swal.fire({ icon: 'error', title: 'No encontrado', text: data.mensaje });
            inputCodigo.select();
            return;
        }
        productoActual = data;
        inputNombre.value = data.nombre;
        inputPrecio.value = formatoMoneda(data.precio);
        btnAgregar.disabled = false;
        inputCantidad.focus();
        inputCantidad.select();
    } catch {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo comunicar con el servidor.' });
    }
}

function limpiarProducto() {
    productoActual = null;
    inputNombre.value = '';
    inputPrecio.value = '';
    inputCantidad.value = 1;
    btnAgregar.disabled = true;
}

// ---------- Agregar / quitar líneas ----------
function agregarLinea() {
    if (!productoActual) return;
    const cantidad = parseInt(inputCantidad.value, 10);
    if (!cantidad || cantidad < 1) {
        Swal.fire({ icon: 'warning', title: 'Cantidad inválida', text: 'La cantidad debe ser mayor a cero.' });
        return;
    }

    // Si el producto ya está en la venta, se suma la cantidad en lugar de duplicar la fila.
    const existente = detalle.find(d => d.idPro === productoActual.idPro);
    if (existente) {
        existente.cantidad += cantidad;
    } else {
        detalle.push({
            idPro: productoActual.idPro,
            nombre: productoActual.nombre,
            precio: productoActual.precio,
            cantidad: cantidad
        });
    }

    dibujarTabla();
    limpiarProducto();
    inputCodigo.value = '';
    inputCodigo.focus();
}

function quitarLinea(idPro) {
    detalle = detalle.filter(d => d.idPro !== idPro);
    dibujarTabla();
}

// ---------- Cálculos y tabla ----------
// Mismo cálculo que el SP: IVA redondeado por línea.
function calcularLinea(d) {
    const subtotal = redondear(d.cantidad * d.precio);
    const iva = redondear(d.cantidad * d.precio * TASA_IVA);
    return { subtotal, iva, total: redondear(subtotal + iva) };
}

function dibujarTabla() {
    tbody.innerHTML = '';
    let totSubtotal = 0, totIva = 0, totTotal = 0;

    if (detalle.length === 0) {
        tbody.innerHTML = '<tr id="filaVacia"><td colspan="8" class="text-center text-muted py-4">Agregue productos a la venta.</td></tr>';
    }

    detalle.forEach(d => {
        const c = calcularLinea(d);
        totSubtotal += c.subtotal;
        totIva += c.iva;
        totTotal += c.total;

        const fila = document.createElement('tr');
        fila.innerHTML = `
            <td>${String(d.idPro).padStart(3, '0')}</td>
            <td>${escaparHtml(d.nombre)}</td>
            <td class="text-end">${d.cantidad}</td>
            <td class="text-end">${formatoMoneda(d.precio)}</td>
            <td class="text-end">${formatoMoneda(c.subtotal)}</td>
            <td class="text-end">${formatoMoneda(c.iva)}</td>
            <td class="text-end">${formatoMoneda(c.total)}</td>
            <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" title="Quitar">✕</button></td>`;
        fila.querySelector('button').addEventListener('click', () => quitarLinea(d.idPro));
        tbody.appendChild(fila);
    });

    document.getElementById('totSubtotal').textContent = formatoMoneda(totSubtotal);
    document.getElementById('totIva').textContent = formatoMoneda(totIva);
    document.getElementById('totTotal').textContent = formatoMoneda(totTotal);
    btnRegistrar.disabled = detalle.length === 0;
}

// ---------- Registrar venta ----------
async function registrarVenta() {
    if (detalle.length === 0) return;

    const total = detalle.reduce((s, d) => s + calcularLinea(d).total, 0);
    const confirmacion = await Swal.fire({
        icon: 'question',
        title: '¿Registrar la venta?',
        text: `Total a pagar: ${formatoMoneda(total)}`,
        showCancelButton: true,
        confirmButtonText: 'Sí, registrar',
        cancelButtonText: 'Cancelar'
    });
    if (!confirmacion.isConfirmed) return;

    btnRegistrar.disabled = true;   // evita doble clic = venta duplicada
    try {
        const resp = await fetch(config.registrarUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': obtenerToken()
            },
            // Solo se envía código y cantidad: el precio lo pone el servidor.
            body: JSON.stringify({ detalle: detalle.map(d => ({ idPro: d.idPro, cantidad: d.cantidad })) })
        });
        const data = await resp.json();

        if (!resp.ok) {
            Swal.fire({ icon: 'error', title: 'No se pudo registrar', text: data.mensaje });
            btnRegistrar.disabled = false;
            return;
        }

        await Swal.fire({
            icon: 'success',
            title: `Venta ${String(data.idVenta).padStart(5, '0')} registrada`,
            text: `Total: ${formatoMoneda(data.total)}`
        });
        limpiarVenta();
    } catch {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo comunicar con el servidor.' });
        btnRegistrar.disabled = false;
    }
}

function limpiarVenta() {
    detalle = [];
    limpiarProducto();
    inputCodigo.value = '';
    dibujarTabla();
    inputCodigo.focus();
}

// ---------- Eventos ----------
btnBuscar.addEventListener('click', buscarProducto);
btnAgregar.addEventListener('click', agregarLinea);
btnRegistrar.addEventListener('click', registrarVenta);
btnLimpiar.addEventListener('click', async () => {
    if (detalle.length === 0) return limpiarVenta();
    const r = await Swal.fire({
        icon: 'warning', title: '¿Cancelar la venta?', text: 'Se quitarán todos los productos.',
        showCancelButton: true, confirmButtonText: 'Sí, cancelar', cancelButtonText: 'No'
    });
    if (r.isConfirmed) limpiarVenta();
});

// Enter en "Código" busca; Enter en "Cantidad" agrega (flujo rápido con teclado).
inputCodigo.addEventListener('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); buscarProducto(); } });
inputCantidad.addEventListener('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); agregarLinea(); } });
