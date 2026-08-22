import 'package:app_usr/models/fotografia_model.dart';
import 'package:app_usr/models/usuario_model.dart';

/// Contrato para el servicio de datos de la pantalla Home.
abstract class IHomeService {
  /// Recupera los datos del usuario autenticado desde /gateway/usuarios/{email}.
  Future<UsuarioModel> getUsuario();

  /// Recupera la fotografía del usuario desde /gateway/fotografias/{email}.
  Future<FotografiaModel> getFotografia();
}
