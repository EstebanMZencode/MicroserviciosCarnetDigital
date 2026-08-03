(function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    let idAEliminar = null;
    let nombreAEliminar = null;

    // TODO (parametrizable): idealmente este valor debería leerse del
    // microservicio de Parametros (HU Web17) en vez de ser una constante fija.
    const TAMANIO_PAGINA = 15;
    let paginaActual = 1;

    const modalForm = document.getElementById('modalForm');
    const modalEliminar = document.getElementById('modalEliminar');
    const modalTitulo = document.getElementById('modalTitulo');
    const inputId = document.getElementById('inputId');
    const inputNombre = document.getElementById('inputNombre');
    const filas = Array.from(document.querySelectorAll('#tablaBody tr'));
    const paginacionInfo = document.getElementById('paginacionInfo');
    const btnPaginaAnterior = document.getElementById('btnPaginaAnterior');
    const btnPaginaSiguiente = document.getElementById('btnPaginaSiguiente');

    function totalPaginas() {
        return Math.max(1, Math.ceil(filas.length / TAMANIO_PAGINA));
    }

    function renderPagina() {
        const inicio = (paginaActual - 1) * TAMANIO_PAGINA;
        const fin = inicio + TAMANIO_PAGINA;

        filas.forEach((fila, i) => {
            fila.style.display = (i >= inicio && i < fin) ? '' : 'none';
        });

        const total = totalPaginas();
        paginacionInfo.textContent = filas.length === 0
            ? 'Sin registros'
            : `Página ${paginaActual} de ${total} (${filas.length} registros)`;

        btnPaginaAnterior.disabled = paginaActual <= 1;
        btnPaginaSiguiente.disabled = paginaActual >= total;
    }

    btnPaginaAnterior.addEventListener('click', () => {
        if (paginaActual > 1) {
            paginaActual--;
            renderPagina();
        }
    });

    btnPaginaSiguiente.addEventListener('click', () => {
        if (paginaActual < totalPaginas()) {
            paginaActual++;
            renderPagina();
        }
    });

    renderPagina();

    // Modal de mensajes (éxito/error) — mismo patrón que ModuloParametros del equipo.
    function mostrarAlerta(mensaje, tipo, recargar = false) {
        document.getElementById('modalMensajeTitulo').textContent = tipo === 'exito' ? 'Éxito' : 'Error';
        document.getElementById('modalMensajeBody').textContent = mensaje;
        document.getElementById('modalMensaje').style.display = 'flex';
        window._recargarAlCerrar = recargar;
    }

    window.cerrarModalMensaje = function () {
        document.getElementById('modalMensaje').style.display = 'none';
        if (window._recargarAlCerrar) location.reload();
    };

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
            nombreAEliminar = e.target.closest('tr').dataset.nombre;
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
            body: JSON.stringify({ id: idAEliminar, nombre: nombreAEliminar })
        });
        const data = await response.json();
        modalEliminar.classList.remove('tu-modal-visible');
        mostrarAlerta(data.mensaje, data.exito ? 'exito' : 'error', data.exito);
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
        mostrarAlerta(data.mensaje, data.exito ? 'exito' : 'error', data.exito);
    });
})();