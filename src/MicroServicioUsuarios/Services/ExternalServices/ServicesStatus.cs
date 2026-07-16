namespace MicroServicioUsuarios.Services.ExternalServices
{
    public class ServicesStatus
    {
        public enum ServiceStatus
        {
            Success,             // Existe y estado true
            Inactive,            // Existe pero estado false
            NotFound,            // No existe
            ServiceUnavailable,  // Error de conexión
            Unauthorized         // No Autorizado
        }

        public ServiceStatus Success { get;} = ServiceStatus.Success;

        public ServiceStatus Inactive { get; } = ServiceStatus.Inactive;
        public ServiceStatus NotFound { get;} = ServiceStatus.NotFound;

        public ServiceStatus ServiceUnavailable { get; } = ServiceStatus.ServiceUnavailable;

        public ServiceStatus Unauthorized { get; } = ServiceStatus.Unauthorized;
    }
}
