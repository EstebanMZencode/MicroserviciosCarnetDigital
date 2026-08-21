class UsuarioQrDto {
  final String usuarioID;
  final String identificacion;
  final String nombreCompleto;

  UsuarioQrDto({
    required this.usuarioID,
    required this.identificacion,
    required this.nombreCompleto,
  });

  factory UsuarioQrDto.fromJson(Map<String, dynamic> json) {
    return UsuarioQrDto(
      usuarioID: json['usuarioID'].toString(),
      identificacion: json['identificacion'] as String,
      nombreCompleto: json['nombreCompleto'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'usuarioID': usuarioID,
      'identificacion': identificacion,
      'nombreCompleto': nombreCompleto,
    };
  }
}