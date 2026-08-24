import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Centraliza el acceso al almacenamiento seguro del dispositivo.
/// Todas las operaciones de sesión (guardar, leer, borrar) pasan por aquí.
class SecureStorageService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  /// Guarda los tres datos de sesión después de un login o refresh exitoso.
  Future<void> saveSession({
    required String accessToken,
    required String refreshToken,
    required String usuarioId,
  }) async {
    await _storage.write(key: 'access_token',  value: accessToken);
    await _storage.write(key: 'refresh_token', value: refreshToken);
    await _storage.write(key: 'usuario_id',    value: usuarioId);
  }

  /// Actualiza solo los tokens tras un refresh exitoso.
  Future<void> updateTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    await _storage.write(key: 'access_token',  value: accessToken);
    await _storage.write(key: 'refresh_token', value: refreshToken);
  }

  Future<String?> getAccessToken()  async => _storage.read(key: 'access_token');
  Future<String?> getRefreshToken() async => _storage.read(key: 'refresh_token');
  Future<String?> getUsuarioId()    async => _storage.read(key: 'usuario_id');

  /// Borra toda la sesión. Llamar al cerrar sesión.
  Future<void> clearSession() async => _storage.deleteAll();
}