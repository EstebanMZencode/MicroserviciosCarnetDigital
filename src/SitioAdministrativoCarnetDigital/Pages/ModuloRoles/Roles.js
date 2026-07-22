//
// ModuloRoles — lógica JavaScript
// ============================================================

// ── Modal de confirmación de eliminación ─────────────────────
let _rolIdPendiente = null;

// Abre el modal de advertencia y guarda el ID a eliminar.
function showDeleteModal(rolId, rolNombre) {
    _rolIdPendiente = rolId;
    const span = document.getElementById('delRolNombre');
    if (span) span.textContent = rolNombre;
    document.getElementById('modalDelete').classList.add('show');
}

// Cierra el modal de advertencia sin hacer nada.
function hideDeleteModal() {
    document.getElementById('modalDelete').classList.remove('show');
    _rolIdPendiente = null;
}

// Transfiere el ID al input oculto y envía el formulario de borrado.
function confirmDelete() {
    if (!_rolIdPendiente) return;
    document.getElementById('inputDeleteId').value = _rolIdPendiente;
    document.getElementById('formDelete').submit();
}

// ── Modal genérico éxito / error ────────────────────────────
// Se llama desde código C# si se requiere mostrar un modal
// en lugar de la etiqueta inline.
function showResultModal(type, title, message) {
    const overlay = document.getElementById('modalResult');
    if (!overlay) return;

    // Aplica la clase de color al ícono
    const icon = document.getElementById('resultIcon');
    icon.className = 'modal-icon modal-icon-' + type; // 'success' | 'error'

    document.getElementById('resultTitle').textContent = title;
    document.getElementById('resultMsg').textContent = message;
    overlay.classList.add('show');
}

function hideResultModal() {
    const overlay = document.getElementById('modalResult');
    if (overlay) overlay.classList.remove('show');
}

// ── Validación en tiempo real de NombreRol ───────────────────
// Elimina cualquier carácter que no sea letra (incluyendo
// español: ñ, tildes) o espacio mientras el usuario escribe.
function validarNombreRol(input) {
    const permitidos = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$/;
    if (!permitidos.test(input.value)) {
        input.value = input.value.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]/g, '');
    }
}

// ── Checklist de pantallas ───────────────────────────────────
// Añade o quita la clase visual "is-checked" al hacer clic.
function togglePantalla(checkbox) {
    const item = checkbox.closest('.pantalla-item');
    if (item) item.classList.toggle('is-checked', checkbox.checked);
}

// ── Inicialización ───────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {

    // En la página Edit, marcar visualmente los checks que ya vienen activos.
    document.querySelectorAll('.pantalla-item input[type="checkbox"]').forEach(cb => {
        if (cb.checked) {
            cb.closest('.pantalla-item')?.classList.add('is-checked');
        }
    });

    // Cerrar cualquier modal al hacer clic en el fondo oscuro (fuera de la caja).
    document.querySelectorAll('.modal-bg').forEach(overlay => {
        overlay.addEventListener('click', e => {
            if (e.target === overlay) overlay.classList.remove('show');
        });
    });
});
