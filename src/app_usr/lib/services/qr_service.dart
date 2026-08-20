import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:app_usr/services/secure_storage_service.dart';

class QRService {
  final SecureStorageService storage;

  QRService(this.storage);

  // URL base de tu MicroServicioQRs publicado en Plesk.
  static const String _baseUrl =
      'https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioQRs';

  /// USR3: obtiene el QR del usuario autenticado.
  /// Devuelve un mapa con: qrBase64 (imagen), contenido (json) y usuarioID.
  Future<Map<String, dynamic>> obtenerQr() async {
    // 1. Sacar el token y el id del usuario guardados en la sesion.
    final token = await storage.getAccessToken();
    final usuarioId = await storage.getUsuarioId();

    if (token == null || token.isEmpty) {
      throw Exception('No hay sesion activa (token no encontrado).');
    }
    if (usuarioId == null || usuarioId.isEmpty) {
      throw Exception('No se encontro el usuario en la sesion.');
    }

    // 2. Armar la peticion al microservicio.
    final url = Uri.parse('$_baseUrl/api/qr/$usuarioId');

    final response = await http.get(
      url,
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
    );

    // 3. Procesar la respuesta.
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      return data; // contiene qrBase64, contenido, usuarioID
    } else if (response.statusCode == 401) {
      throw Exception('No autorizado. La sesion pudo haber expirado.');
    } else if (response.statusCode == 404) {
      throw Exception('Usuario no encontrado o inactivo.');
    } else {
      throw Exception('Error al obtener el QR (${response.statusCode}).');
    }
  }
}