import 'package:your_app/services/secure_storage_service.dart';

class AuthService {
  final SecureStorageService storage;

  AuthService(this.storage);

  Future<void> obtenerDatos() async {
    final token = await storage.getAccessToken();
    final usuarioId = await storage.getUsuarioId();

    // Consumir API...
  }
}