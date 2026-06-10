namespace MicroServicioParametros.Security
{
    public class TokenValidator
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TokenValidator(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Devuelve true si el token es válido (o si la validación está apagada)
        public async Task<bool> EsValidoAsync(string? token)
        {
            // El interruptor: si está apagado, deja pasar sin validar
            var validar = _configuration.GetValue<bool>("Auth:ValidarToken");
            if (!validar)
            {
                return true;
            }

            // Si está encendido pero no llegó token, no es válido
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var url = _configuration.GetValue<string>("Auth:ValidateUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("token", token);

                var response = await _httpClient.SendAsync(request);

                // El Auth devuelve 200 con true si es válido, 401 si no
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var body = await response.Content.ReadAsStringAsync();
                return body.Trim().ToLower().Contains("true");
            }
            catch
            {
                // Si el Auth no responde, por seguridad no autoriza
                return false;
            }
        }
    }
}