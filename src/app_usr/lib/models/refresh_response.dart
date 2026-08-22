/// Respuesta del POST /gateway/auth/refresh
class RefreshResponse {
  final String accessToken;
  final String refreshToken;

  RefreshResponse({
    required this.accessToken,
    required this.refreshToken,
  });

  factory RefreshResponse.fromJson(Map<String, dynamic> json) {
    return RefreshResponse(
      accessToken:  json['access_token']  as String,
      refreshToken: json['refresh_token'] as String,
    );
  }
}