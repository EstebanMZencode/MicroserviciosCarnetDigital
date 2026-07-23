(function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const alertBox = document.getElementById('alertBox');

    function mostrarAlerta(mensaje, tipo) {
        alertBox.classList.remove('ce-alert-exito', 'ce-alert-error');
        alertBox.classList.add(tipo === 'exito' ? 'ce-alert-exito' : 'ce-alert-error');
        alertBox.style.display = 'block';
        alertBox.textContent = mensaje;
        setTimeout(() => alertBox.style.display = 'none', 4000);
    }

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

        if (data.exito) {
            mostrarAlerta(data.mensaje, 'exito');
        } else {
            mostrarAlerta(data.mensaje, 'error');
        }
    });
})();