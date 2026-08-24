/// Respuesta del GET /gateway/usuarios/{email}
class UsuarioModel {
  final String identificacion;
  final String nombreCompleto;
  final String tipoUsuario;
  // Vacías cuando no aplican — nunca null
  final List<String> carreras;
  final List<String> areas;

  UsuarioModel({
    required this.identificacion,
    required this.nombreCompleto,
    required this.tipoUsuario,
    required this.carreras,
    required this.areas,
  });

  factory UsuarioModel.fromJson(Map<String, dynamic> json) {
    return UsuarioModel(
      identificacion: json['identificacion'] as String,
      nombreCompleto: json['nombreCompleto'] as String,
      tipoUsuario:    json['tipoUsuario']    as String,
      carreras: (json['carreras'] as List<dynamic>?)
              ?.map((e) => e as String)
              .toList() ??
          [],
      areas: (json['areas'] as List<dynamic>?)
              ?.map((e) => e as String)
              .toList() ??
          [],
    );
  }
}
