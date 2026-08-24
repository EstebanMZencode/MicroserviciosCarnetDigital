class GuardaPerfil {
  final String identificacion;
  final String nombreCompleto;
  final String tipoUsuario;
  final String? fotoBase64;

  GuardaPerfil({
    required this.identificacion,
    required this.nombreCompleto,
    required this.tipoUsuario,
    this.fotoBase64,
  });

  factory GuardaPerfil.fromDetalleJson(
    Map<String, dynamic> json, {
    String? fotoBase64,
  }) {
    return GuardaPerfil(
      identificacion: json['identificacion'] as String,
      nombreCompleto: json['nombreCompleto'] as String,
      tipoUsuario: json['tipoUsuario'] as String,
      fotoBase64: fotoBase64,
    );
  }
}
