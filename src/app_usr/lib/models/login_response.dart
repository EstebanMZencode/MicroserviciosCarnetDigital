/// Respuesta del POST /gateway/auth/login
class LoginResponse {
  final String accessToken;
  final String refreshToken;
  final String expiresIn;
  // El campo usuarioID en la respuesta del microservicio corresponde al email
  final String usuarioId;

  LoginResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresIn,
    required this.usuarioId,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      accessToken:  json['access_token']  as String,
      refreshToken: json['refresh_token'] as String,
      expiresIn:    json['expires_in'].toString(),
      usuarioId:    json['usuarioID']     as String,
    );
  }
}