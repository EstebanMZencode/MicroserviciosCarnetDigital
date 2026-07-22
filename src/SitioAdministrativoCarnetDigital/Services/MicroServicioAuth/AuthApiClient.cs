namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAuth
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
