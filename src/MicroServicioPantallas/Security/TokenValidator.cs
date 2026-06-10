namespace MicroServicioPantallas.Security
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
                var request = new HttpRequestMessage(HttpMethod.Post, url);
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