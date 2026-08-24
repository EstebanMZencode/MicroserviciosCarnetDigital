import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:app_grd/models/guarda_perfil.dart';
import 'package:app_grd/services/secure_storage_service.dart';

// Señala que el token guardado no existe o ya no es válido, para que la
// pantalla redirija al login sin mostrar un mensaje de error genérico.
class SesionExpiradaException implements Exception {}

class HomeService {
  final SecureStorageService storage;

  HomeService(this.storage);

  static const String _baseUrl =
      'https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioGatewayPry';

  Future<GuardaPerfil> obtenerPerfil(String email) async {
    final token = await storage.getAccessToken();
    if (token == null || token.isEmpty) {
      throw SesionExpiradaException();
    }

    final headers = {'Authorization': 'Bearer $token'};

    final detalleResponse = await http.get(
      Uri.parse('$_baseUrl/gateway/usuarios/$email'),
      headers: headers,
    );

    if (detalleResponse.statusCode == 401) {
      throw SesionExpiradaException();
    } else if (detalleResponse.statusCode == 404) {
      throw Exception('No se encontró la información del guarda.');
    } else if (detalleResponse.statusCode != 200) {
      throw Exception('No se pudo obtener la información del guarda.');
    }

    final detalleJson = jsonDecode(detalleResponse.body) as Map<String, dynamic>;

    // La fotografía vive en un microservicio aparte (MicroServicioFotografias).
    // Si la consulta falla o el usuario no tiene foto, se omite y GRD2 muestra
    // un avatar por defecto en su lugar.
    String? fotoBase64;
    try {
      final fotoResponse = await http.get(
        Uri.parse('$_baseUrl/gateway/fotografias/usuario/fotografia/$email'),
        headers: headers,
      );

      if (fotoResponse.statusCode == 200) {
        final fotoJson = jsonDecode(fotoResponse.body) as Map<String, dynamic>;
        fotoBase64 = fotoJson['fotoBase64'] as String?;
      }
    } catch (_) {
      fotoBase64 = null;
    }

    return GuardaPerfil.fromDetalleJson(detalleJson, fotoBase64: fotoBase64);
  }
}
