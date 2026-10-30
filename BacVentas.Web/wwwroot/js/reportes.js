// Muestra el detalle de una venta en un modal de SweetAlert2.
document.querySelectorAll('.btn-detalle').forEach(boton => {
    boton.addEventListener('click', async () => {
        try {
            const resp = await fetch(boton.dataset.url);
            const data = await resp.json();
            if (!resp.ok) {
                Swal.fire({ icon: 'error', title: 'Error', text: data.mensaje });
                return;
            }

            const filas = data.map(d => `
                <tr>
                    <td>${String(d.idPro).padStart(3, '0')}</td>
                    <td class="text-start">${escaparHtml(d.producto)}</td>
                    <td class="text-end">${d.cantidad}</td>
                    <td class="text-end">${formatoMoneda(d.precio)}</td>
                    <td class="text-end">${formatoMoneda(d.iva)}</td>
                    <td class="text-end">${formatoMoneda(d.total)}</td>
                </tr>`).join('');

            Swal.fire({
                title: `Venta ${String(boton.dataset.id).padStart(5, '0')}`,
                width: 800,
                html: `
                    <table class="table table-sm">
                        <thead><tr><th>Código</th><th class="text-start">Producto</th><th class="text-end">Cant.</th>
                        <th class="text-end">Precio</th><th class="text-end">IVA</th><th class="text-end">Total</th></tr></thead>
                        <tbody>${filas}</tbody>
                    </table>`
            });
        } catch {
            Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo comunicar con el servidor.' });
        }
    });
});
