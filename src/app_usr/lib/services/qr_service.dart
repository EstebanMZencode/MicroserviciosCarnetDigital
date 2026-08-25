import 'dart:convert';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:app_usr/services/secure_storage_service.dart';

class QRService {
  final SecureStorageService storage;

  QRService(this.storage);

  // Misma base del Gateway que usan los demás services (viene del .env)
  String get _baseUrl => dotenv.env['BASE_URL']!;

  /// USR3: obtiene el QR del usuario autenticado a través del Gateway.
  /// Devuelve un mapa con: qrBase64 (imagen), contenido (json) y usuarioID.
  Future<Map<String, dynamic>> obtenerQr() async {
    final token = await storage.getAccessToken();
    final usuarioId = await storage.getUsuarioId();

    if (token == null || usuarioId == null) {
      throw Exception('Sesión no disponible.');
    }

    final response = await http.get(
      Uri.parse('$_baseUrl/gateway/qrs/$usuarioId'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    } else if (response.statusCode == 401) {
      throw Exception('No autorizado. La sesión pudo haber expirado.');
    } else if (response.statusCode == 404) {
      throw Exception('Usuario no encontrado o inactivo.');
    } else {
      throw Exception('No se pudo obtener el QR (${response.statusCode}).');
    }
  }
}