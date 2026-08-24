import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:app_grd/services/secure_storage_service.dart';

class AuthService {
  final SecureStorageService storage;

  AuthService(this.storage);

  static const String _baseUrl =
      'https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioGatewayPry';

  // TODO: reemplazar por el GUID real del TipoUsuario "Guarda"
  // (tabla TiposUsuarios, MicroServicioTiposUsuario). Con este placeholder
  // el login siempre devolverá 401 hasta que se configure el GUID correcto.
  static const String _tipoUsuarioGuarda = 'D332DF13-979D-F111-947E-E5709BBC83CF';

  // POST /gateway/auth/login → MicroServicioAuth: /api/login.
  // El endpoint real recibe las credenciales por headers HTTP (usuario,
  // contrasena, tipo_usuario) con el body vacío, no como JSON body.
  Future<void> login(String email, String password) async {
    final url = Uri.parse('$_baseUrl/gateway/auth/login');

    http.Response response;
    try {
      response = await http.post(
        url,
        headers: {
          'usuario': email,
          'contrasena': password,
          'tipo_usuario': _tipoUsuarioGuarda,
        },
      );
    } catch (_) {
      throw Exception('No se pudo conectar con el servidor. Inténtelo más tarde.');
    }

    if (response.statusCode == 200 || response.statusCode == 201) {
      final json = jsonDecode(response.body) as Map<String, dynamic>;

      await storage.saveSession(
        accessToken: json['access_token'] as String,
        refreshToken: json['refresh_token'] as String,
        usuarioId: json['usuarioID'] as String,
      );
    } else if (response.statusCode == 401) {
      throw Exception('Usuario y/o contraseña incorrectos.');
    } else {
      throw Exception('No se pudo iniciar sesión. Inténtelo más tarde.');
    }
  }
}
