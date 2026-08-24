/// Respuesta del GET /gateway/fotografias/{email}
class FotografiaModel {
  final String email;
  // null o vacío cuando el usuario no tiene fotografía registrada
  final String? fotoBase64;

  FotografiaModel({
    required this.email,
    this.fotoBase64,
  });

  bool get tieneFoto =>
      fotoBase64 != null && fotoBase64!.isNotEmpty;

  factory FotografiaModel.fromJson(Map<String, dynamic> json) {
    return FotografiaModel(
      email:       json['email']      as String,
      fotoBase64: (json['fotoBase64'] as String?)?.isEmpty == true
          ? null
          : json['fotoBase64'] as String?,
    );
  }
}