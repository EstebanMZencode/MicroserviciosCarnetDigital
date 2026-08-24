import 'dart:async';
import 'dart:convert';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:app_usr/models/login_response.dart';
import 'package:app_usr/models/refresh_response.dart';
import 'package:app_usr/services/i_auth_service.dart';
import 'package:app_usr/services/secure_storage_service.dart';

class AuthService implements IAuthService {
  final SecureStorageService _storage;
  Timer? _refreshTimer;

  // Los tipos se leen del .env en el orden en que están declarados.
  // El login los intenta secuencialmente hasta encontrar uno exitoso.
  static const List<String> _tiposEnvKeys = [
    'TipoEstudiante',
    'TipoFuncionario',
    'TipoGuarda',
    'TipoAdministrador',
  ];

  AuthService(this._storage);

  String get _baseUrl => dotenv.env['BASE_URL']!;

  // ── Login ─────────────────────────────────────────────────────────────────

  @override
  Future<LoginResponse> login(String email, String password) async {
    for (final key in _tiposEnvKeys) {
      final tipoUsuario = dotenv.env[key];
      if (tipoUsuario == null || tipoUsuario.isEmpty) continue;

      final respuesta = await _intentarLogin(email, password, tipoUsuario);
      if (respuesta != null) {
        // Login exitoso: persistir la sesión y arrancar el refresh automático
        await _storage.saveSession(
          accessToken:  respuesta.accessToken,
          refreshToken: respuesta.refreshToken,
          usuarioId:    respuesta.usuarioId,
        );
        iniciarRefreshPeriodico();
        return respuesta;
      }
    }
    // Ningún tipo de usuario logró autenticar
    throw Exception('Usuario y/o contraseña incorrectos.');
  }

  /// Hace un único intento de login con el [tipoUsuario] dado.
  /// Devuelve null si la respuesta no es 201.
  Future<LoginResponse?> _intentarLogin(
    String email,
    String password,
    String tipoUsuario,
  ) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/gateway/auth/login'),
        headers: {
          'usuario':      email,
          'contrasena':   password,
          'tipo_usuario': tipoUsuario,
        },
      );
      if (response.statusCode == 201) {
        return LoginResponse.fromJson(
            jsonDecode(response.body) as Map<String, dynamic>);
      }
      return null;
    } catch (_) {
      return null;
    }
  }

  // ── Refresh ───────────────────────────────────────────────────────────────

  @override
  Future<void> refresh() async {
    final refreshToken = await _storage.getRefreshToken();
    final email        = await _storage.getUsuarioId();

    if (refreshToken == null || email == null) return;

    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/gateway/auth/refresh'),
        headers: {
          'email':         email,
          'refresh_token': refreshToken,
        },
      );

      if (response.statusCode == 201 || response.statusCode == 200) {
        final datos = RefreshResponse.fromJson(
            jsonDecode(response.body) as Map<String, dynamic>);
        // Actualizar ambos tokens — crítico para que la sesión no expire
        await _storage.updateTokens(
          accessToken:  datos.accessToken,
          refreshToken: datos.refreshToken,
        );
      }
    } catch (_) {
      // Error de red en el refresh: se reintenta en el próximo ciclo
    }
  }

  // ── Timer periódico ───────────────────────────────────────────────────────

  @override
  void iniciarRefreshPeriodico() {
    _refreshTimer?.cancel();
    // Cada 4 minutos exactos, sin depender de actividad del usuario
    _refreshTimer = Timer.periodic(
      const Duration(minutes: 4),
      (_) => refresh(),
    );
  }

  @override
  Future<void> cerrarSesion() async {
    _refreshTimer?.cancel();
    _refreshTimer = null;
    await _storage.clearSession();
  }
}
