// Utilidades compartidas por todas las pantallas.

// Lee el token antiforgery que genera @Html.AntiForgeryToken() en la vista.
// Se envía en la cabecera "RequestVerificationToken" en cada POST con fetch().
function obtenerToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

// Formato de dinero: 1234.5 -> "$1,234.50"
function formatoMoneda(valor) {
    return '$' + Number(valor).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// Redondeo a 2 decimales (igual que ROUND(x, 2) en SQL Server).
// Number.EPSILON corrige errores de punto flotante, ej. 1.005 -> 1.01
function redondear(valor) {
    return Math.round((valor + Number.EPSILON) * 100) / 100;
}

// Escapa texto antes de insertarlo como HTML (previene XSS si un nombre trae "<script>").
function escaparHtml(texto) {
    const div = document.createElement('div');
    div.textContent = texto ?? '';
    return div.innerHTML;
}
