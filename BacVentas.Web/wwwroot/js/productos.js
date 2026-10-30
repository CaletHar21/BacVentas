// Eliminar producto: confirmación con SweetAlert2 + petición fetch() al controlador.
document.querySelectorAll('.btn-eliminar').forEach(boton => {
    boton.addEventListener('click', async () => {
        const id = boton.dataset.id;
        const nombre = boton.dataset.nombre;

        const confirmacion = await Swal.fire({
            icon: 'warning',
            title: '¿Eliminar producto?',
            text: `${String(id).padStart(3, '0')} - ${nombre}`,
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc3545'
        });
        if (!confirmacion.isConfirmed) return;

        try {
            const resp = await fetch(boton.dataset.url, {
                method: 'POST',
                headers: { 'RequestVerificationToken': obtenerToken() }
            });
            const data = await resp.json();

            if (data.ok) {
                document.getElementById(`fila-${id}`)?.remove();
                Swal.fire({ icon: 'success', title: 'Eliminado', text: data.mensaje, timer: 2000 });
            } else {
                // Ej.: el producto tiene ventas registradas (regla validada en el SP).
                Swal.fire({ icon: 'error', title: 'No se pudo eliminar', text: data.mensaje });
            }
        } catch {
            Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo comunicar con el servidor.' });
        }
    });
});
