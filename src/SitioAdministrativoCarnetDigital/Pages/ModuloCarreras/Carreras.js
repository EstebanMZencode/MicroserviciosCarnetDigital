function abrirModalCrear() {
    document.getElementById('modalTitulo').innerText = 'Nueva Carrera';
    document.getElementById('CarreraID').value = '';
    document.getElementById('NombreCarrera').value = '';
    document.getElementById('DirectorCarrera').value = '';
    document.getElementById('Email').value = '';
    document.getElementById('Telefono').value = '';
    document.getElementById('InstitucionID').value = '';
    document.getElementById('modalCarrera').classList.add('active');
}

function abrirModalEditar(id, nombre, director, email, telefono, institucionId) {
    document.getElementById('modalTitulo').innerText = 'Editar Carrera';
    document.getElementById('CarreraID').value = id;
    document.getElementById('NombreCarrera').value = nombre;
    document.getElementById('DirectorCarrera').value = director;
    document.getElementById('Email').value = email;
    document.getElementById('Telefono').value = telefono;
    document.getElementById('InstitucionID').value = institucionId;
    document.getElementById('modalCarrera').classList.add('active');
}

function cerrarModal() {
    document.getElementById('modalCarrera').classList.remove('active');
}

function confirmarEliminar(id, nombre) {
    document.getElementById('EliminarID').value = id;
    document.getElementById('mensajeEliminar').innerText = `¿Realmente desea eliminar la carrera "${nombre}"?`;
    document.getElementById('modalEliminar').classList.add('active');
}

function cerrarModalEliminar() {
    document.getElementById('modalEliminar').classList.remove('active');
} 