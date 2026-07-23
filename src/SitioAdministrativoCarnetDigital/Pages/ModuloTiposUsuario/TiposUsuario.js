(function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    let idAEliminar = null;

    const alertBox = document.getElementById('alertBox');
    const modalForm = document.getElementById('modalForm');
    const modalEliminar = document.getElementById('modalEliminar');
    const modalTitulo = document.getElementById('modalTitulo');
    const inputId = document.getElementById('inputId');
    const inputNombre = document.getElementById('inputNombre');

    function mostrarAlerta(mensaje, tipo) {
        alertBox.classList.remove('tu-alert-exito', 'tu-alert-error');
        alertBox.classList.add(tipo === 'exito' ? 'tu-alert-exito' : 'tu-alert-error');
        alertBox.style.display = 'block';
        alertBox.textContent = mensaje;
        setTimeout(() => alertBox.style.display = 'none', 4000);
    }

    document.getElementById('btnNuevo').addEventListener('click', () => {
        modalTitulo.textContent = 'Nuevo Tipo de Usuario';
        inputId.value = '';
        inputNombre.value = '';
        modalForm.classList.add('tu-modal-visible');
    });

    document.getElementById('btnCancelar').addEventListener('click', () => {
        modalForm.classList.remove('tu-modal-visible');
    });

    document.querySelectorAll('.btn-editar').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const fila = e.target.closest('tr');
            modalTitulo.textContent = 'Editar Tipo de Usuario';
            inputId.value = fila.dataset.id;
            inputNombre.value = fila.dataset.nombre;
            modalForm.classList.add('tu-modal-visible');
        });
    });

    document.querySelectorAll('.btn-eliminar').forEach(btn => {
        btn.addEventListener('click', (e) => {
            idAEliminar = e.target.closest('tr').dataset.id;
            modalEliminar.classList.add('tu-modal-visible');
        });
    });

    document.getElementById('btnNo').addEventListener('click', () => {
        modalEliminar.classList.remove('tu-modal-visible');
        idAEliminar = null;
    });

    document.getElementById('btnSi').addEventListener('click', async () => {
        const response = await fetch('?handler=Eliminar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify(idAEliminar)
        });
        const data = await response.json();
        modalEliminar.classList.remove('tu-modal-visible');
        if (data.exito) {
            mostrarAlerta(data.mensaje, 'exito');
            setTimeout(() => location.reload(), 700);
        } else {
            mostrarAlerta(data.mensaje, 'error');
        }
    });

    document.getElementById('btnGuardar').addEventListener('click', async () => {
        const id = inputId.value;
        const nombre = inputNombre.value.trim();
        if (!nombre) {
            mostrarAlerta('El nombre no puede estar vacío.', 'error');
            return;
        }

        const response = await fetch('?handler=Guardar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ tipoUsuarioID: id || null, nombre: nombre })
        });
        const data = await response.json();
        modalForm.classList.remove('tu-modal-visible');
        if (data.exito) {
            mostrarAlerta(data.mensaje, 'exito');
            setTimeout(() => location.reload(), 700);
        } else {
            mostrarAlerta(data.mensaje, 'error');
        }
    });
})();