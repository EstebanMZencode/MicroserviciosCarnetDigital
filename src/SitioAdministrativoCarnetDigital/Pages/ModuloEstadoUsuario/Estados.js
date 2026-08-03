(function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Modal de mensajes (éxito/error) — mismo patrón que ModuloParametros del equipo.
    function mostrarAlerta(mensaje, tipo) {
        document.getElementById('modalMensajeTitulo').textContent = tipo === 'exito' ? 'Éxito' : 'Error';
        document.getElementById('modalMensajeBody').textContent = mensaje;
        document.getElementById('modalMensaje').style.display = 'flex';
    }

    window.cerrarModalMensaje = function () {
        document.getElementById('modalMensaje').style.display = 'none';
    };

    document.getElementById('btnCambiar').addEventListener('click', async () => {
        const email = document.getElementById('inputEmail').value.trim();
        const estadoId = document.getElementById('inputEstado').value;

        if (!email) {
            mostrarAlerta('El email del usuario es obligatorio.', 'error');
            return;
        }
        if (!estadoId) {
            mostrarAlerta('Debe seleccionar un estado.', 'error');
            return;
        }

        const response = await fetch('?handler=Cambiar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ emailUsuario: email, estadoID: estadoId })
        });
        const data = await response.json();

        mostrarAlerta(data.mensaje, data.exito ? 'exito' : 'error');
    });
})();