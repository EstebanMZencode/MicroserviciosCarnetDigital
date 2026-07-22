function abrirModalCrear() {
    document.getElementById('modalTitulo').innerText = 'Nueva Institución';
    document.getElementById('InstitucionID').value = '';
    document.getElementById('Nombre').value = '';
    document.getElementById('Email').value = '';
    document.getElementById('Telefono').value = '';
    document.getElementById('Dominios').value = '';
    document.getElementById('modalInstitucion').classList.add('active');
}

function abrirModalEditar(id, nombre, email, telefono, dominios) {
    document.getElementById('modalTitulo').innerText = 'Editar Institución';
    document.getElementById('InstitucionID').value = id;
    document.getElementById('Nombre').value = nombre;
    document.getElementById('Email').value = email;
    document.getElementById('Telefono').value = telefono;
    document.getElementById('Dominios').value = dominios;
    document.getElementById('modalInstitucion').classList.add('active');
}

function cerrarModal() {
    document.getElementById('modalInstitucion').classList.remove('active');
}

function confirmarEliminar(id, nombre) {
    document.getElementById('EliminarID').value = id;
    document.getElementById('mensajeEliminar').innerText = `¿Realmente desea eliminar la institución "${nombre}"?`;
    document.getElementById('modalEliminar').classList.add('active');
}

function cerrarModalEliminar() {
    document.getElementById('modalEliminar').classList.remove('active');
}