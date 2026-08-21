class ValidacionResponse {
  final bool valido;
  final String mensaje;
  final List<String> diferencias;

  ValidacionResponse({
    required this.valido,
    required this.mensaje,
    required this.diferencias,
  });

  factory ValidacionResponse.fromJson(Map<String, dynamic> json) {
    return ValidacionResponse(
      valido: json['valido'] as bool,
      mensaje: json['mensaje'] as String,
      diferencias: List<String>.from(json['diferencias'] ?? []),
    );
  }
}