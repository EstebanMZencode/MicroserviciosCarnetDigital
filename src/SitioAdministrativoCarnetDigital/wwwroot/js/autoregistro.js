// GUIDs de tipos de usuario - hardcodeados porque este archivo no pasa por Razor
var GUID_ESTUDIANTE = '0B4D7DA0-1438-436C-B2D4-2D3A8F3005E7';
var GUID_FUNCIONARIO = 'A4CB240E-5115-45BE-B9CE-43DE7F8E6862';
var AT = String.fromCharCode(64); // '@' — sin escribirlo literalmente por precaución

function onTipoUsuarioCambio(valor) {
    var secCarreras = document.getElementById('seccionCarreras');
    var secAreas = document.getElementById('seccionAreas');
    var emailInput = document.getElementById('Email');

    if (valor === GUID_ESTUDIANTE) {
        emailInput.placeholder = 'usuario' + AT + 'cuc.cr';
        secCarreras.classList.add('visible');
        secAreas.classList.remove('visible');
        desmarcarTodos('#seccionAreas input[type="checkbox"]');
    } else if (valor === GUID_FUNCIONARIO) {
        emailInput.placeholder = 'usuario' + AT + 'cuc.ac.cr';
        secAreas.classList.add('visible');
        secCarreras.classList.remove('visible');
        desmarcarTodos('#seccionCarreras input[type="checkbox"]');
    } else {
        secCarreras.classList.remove('visible');
        secAreas.classList.remove('visible');
    }
}

function desmarcarTodos(selector) {
    document.querySelectorAll(selector).forEach(function (cb) {
        cb.checked = false;
        var item = cb.closest('.check-item');
        if (item) item.classList.remove('checked');
    });
}

function toggleCheck(checkbox) {
    var item = checkbox.closest('.check-item');
    if (item) item.classList.toggle('checked', checkbox.checked);
}

document.addEventListener('DOMContentLoaded', function () {
    // Adjuntar validación al formulario
    var form = document.getElementById('formRegistro');
    if (form) {
        form.addEventListener('submit', function (e) {
            var tipoUsuario = document.getElementById('TipoUsuarioID').value;
            var email = document.getElementById('Email').value.trim().toLowerCase();
            var pass = document.getElementById('Contrasena').value;
            var confPass = document.getElementById('ConfContrasena').value;
            var btn = document.getElementById('btnRegistrar');

            if (!tipoUsuario) {
                e.preventDefault();
                alert('Debe seleccionar un tipo de usuario.');
                return;
            }

            var atPos = email.indexOf(AT);
            var dominio = atPos >= 0 ? email.substring(atPos + 1) : '';
            var dominioValido = tipoUsuario === GUID_ESTUDIANTE ? 'cuc.cr' : 'cuc.ac.cr';

            if (dominio !== dominioValido) {
                e.preventDefault();
                alert('El correo debe terminar en ' + AT + dominioValido + ' para este tipo de usuario.');
                return;
            }

            if (pass !== confPass) {
                e.preventDefault();
                alert('Las contraseñas no coinciden.');
                return;
            }

            if (tipoUsuario === GUID_ESTUDIANTE) {
                var marcadasCarreras = document.querySelectorAll('#seccionCarreras input[type="checkbox"]:checked');
                if (marcadasCarreras.length === 0) {
                    e.preventDefault();
                    alert('Debe seleccionar al menos una carrera.');
                    return;
                }
            } else {
                var marcadasAreas = document.querySelectorAll('#seccionAreas input[type="checkbox"]:checked');
                if (marcadasAreas.length === 0) {
                    e.preventDefault();
                    alert('Debe seleccionar al menos un area de trabajo.');
                    return;
                }
            }

            btn.disabled = true;
            btn.textContent = 'Enviando...';
        });
    }

    // Restaurar estado visual si el form viene pre-rellenado tras un POST con error
    var sel = document.getElementById('TipoUsuarioID');
    if (sel && sel.value) {
        onTipoUsuarioCambio(sel.value);
    }

    document.querySelectorAll('.check-item input[type="checkbox"]').forEach(function (cb) {
        if (cb.checked) {
            var item = cb.closest('.check-item');
            if (item) item.classList.add('checked');
        }
    });
});
