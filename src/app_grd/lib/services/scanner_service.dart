import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:app_grd/models/usuario_qr_dto.dart';
import 'package:app_grd/models/validacion_response.dart';
import 'package:app_grd/services/secure_storage_service.dart';

class ScannerService {
  final SecureStorageService storage;

  ScannerService(this.storage);

  String get _baseUrl => dotenv.env['BASE_URL']!;

  Future<ValidacionResponse> validarQr(UsuarioQrDto datos) async {
    final token = await storage.getAccessToken();

    if (token == null || token.isEmpty) {
      throw Exception('No hay sesión activa.');
    }

    final url = Uri.parse('$_baseUrl/gateway/qrs/validar');

    final response = await http.post(
      url,
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode(datos.toJson()),
    );

    if (response.statusCode == 200) {
      return ValidacionResponse.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception('No autorizado. La sesión pudo haber expirado.');
    } else {
      throw Exception('Error al validar el QR (${response.statusCode}).');
    }
  }
}