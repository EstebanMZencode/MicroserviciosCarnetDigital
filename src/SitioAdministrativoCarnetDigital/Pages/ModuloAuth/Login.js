// ============================================================
// ModuloAuth — Login.js
// ============================================================

document.addEventListener('DOMContentLoaded', () => {

    const form = document.getElementById('loginForm');
    const btn = document.getElementById('btnSubmit');
    const user = document.getElementById('inputUsuario');
    const pass = document.getElementById('inputContrasena');

    if (!form) return;

    form.addEventListener('submit', (e) => {
        // Validación mínima en cliente: ambos campos requeridos.
        // El servidor hace la misma validación; esto evita un round-trip innecesario.
        if (!user.value.trim() || !pass.value.trim()) {
            e.preventDefault();
            showAlert('Usuario y/o contraseña incorrectos.');
            return;
        }

        // Deshabilitar botón mientras el servidor procesa la solicitud.
        btn.disabled = true;
        btn.textContent = 'Verificando...';
    });

    // Re-habilitar el botón si el usuario modifica los campos tras un error.
    [user, pass].forEach(el =>
        el.addEventListener('input', () => {
            if (btn.disabled) {
                btn.disabled = false;
                btn.textContent = 'Aceptar';
            }
        })
    );
});

// Muestra el mensaje de error en el bloque .form-alert.
function showAlert(msg) {
    const el = document.getElementById('formAlert');
    if (!el) return;
    el.textContent = msg;
    el.classList.add('show');
}

// Persiste en localStorage todos los datos que devuelve el endpoint /login.
// Se llama desde el markup Razor justo después de un login exitoso,
// antes de redirigir, para que cualquier código JS del sitio pueda acceder
// al email, tipo de usuario, fechas, etc. del usuario autenticado.
function storeLoginData(data) {
    try {
        localStorage.setItem('loginData', JSON.stringify(data));
    } catch (err) {
        // localStorage puede no estar disponible (modo incógnito sin cuota, etc.)
        console.warn('No se pudo guardar en localStorage:', err);
    }
}
