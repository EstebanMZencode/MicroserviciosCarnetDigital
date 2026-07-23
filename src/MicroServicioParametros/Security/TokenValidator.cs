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

        public async Task<bool> EsValidoAsync(string? token)
        {
            var validar = _configuration.GetValue<bool>("Auth:ValidarToken");
            if (!validar)
                return true;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var url = _configuration.GetValue<string>("Auth:ValidateUrl");
                var method = _configuration.GetValue<string>("Auth:HttpMethod") ?? "POST";

                var httpMethod = method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                    ? HttpMethod.Get
                    : HttpMethod.Post;

                var request = new HttpRequestMessage(httpMethod, url);
                request.Headers.Add("token", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return false;

                var body = await response.Content.ReadAsStringAsync();
                return body.Trim().ToLower().Contains("true");
            }
            catch
            {
                return false;
            }
        }
    }
}