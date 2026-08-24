import 'package:app_usr/models/login_response.dart';

/// Contrato para el servicio de autenticación.
abstract class IAuthService {
  /// Intenta login con cada tipo de usuario del .env en orden.
  /// Devuelve el [LoginResponse] del primer intento exitoso.
  /// Lanza [Exception] si ningún tipo logra autenticar.
  Future<LoginResponse> login(String email, String password);

  /// Llama a /refresh y actualiza ambos tokens en SecureStorage.
  Future<void> refresh();

  /// Inicia el timer periódico de refresh (cada 4 minutos).
  void iniciarRefreshPeriodico();

  /// Detiene el timer y limpia la sesión. Llamar al cerrar sesión.
  Future<void> cerrarSesion();
}