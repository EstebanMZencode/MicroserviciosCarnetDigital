import 'dart:convert';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:app_usr/models/fotografia_model.dart';
import 'package:app_usr/models/usuario_model.dart';
import 'package:app_usr/services/i_home_service.dart';
import 'package:app_usr/services/secure_storage_service.dart';

class HomeService implements IHomeService {
  final SecureStorageService _storage;

  HomeService(this._storage);

  String get _baseUrl => dotenv.env['BASE_URL']!;

  // ── Datos del usuario ─────────────────────────────────────────────────────

  @override
  Future<UsuarioModel> getUsuario() async {
    final token = await _storage.getAccessToken();
    final email = await _storage.getUsuarioId();

    if (token == null || email == null) {
      throw Exception('Sesión no disponible.');
    }

    final response = await http.get(
      Uri.parse('$_baseUrl/gateway/usuarios/$email'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      return UsuarioModel.fromJson(
          jsonDecode(response.body) as Map<String, dynamic>);
    }
    throw Exception('No se pudieron obtener los datos del usuario (${response.statusCode}).');
  }

  // ── Fotografía ────────────────────────────────────────────────────────────

  @override
  Future<FotografiaModel> getFotografia() async {
    final token = await _storage.getAccessToken();
    final email = await _storage.getUsuarioId();

    if (token == null || email == null) {
      throw Exception('Sesión no disponible.');
    }

    final response = await http.get(
      Uri.parse('$_baseUrl/gateway/fotografias/$email'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      return FotografiaModel.fromJson(
          jsonDecode(response.body) as Map<String, dynamic>);
    }
    // Si el endpoint devuelve 404 u otro código se considera sin foto
    return FotografiaModel(email: email ?? '', fotoBase64: null);
  }
}
