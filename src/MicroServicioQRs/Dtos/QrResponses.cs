namespace MicroServicioQRs.Dtos
{
    // Lo que recibe USR3 (Flutter) para pintar el QR en pantalla.
    public class QrResponse
    {
        // Imagen PNG del QR en base64 (sin el prefijo "data:image/png;base64,").
        // En Flutter se muestra asi:  Image.memory(base64Decode(qrBase64))
        public string QrBase64 { get; set; }

        // El JSON crudo que quedo codificado dentro del QR (util para depurar).
        public string Contenido { get; set; }

        public Guid UsuarioID { get; set; }
    }

    // Resultado de la validacion dato por dato (para GRD3, la app del guarda).
    public class ValidacionResponse
    {
        public bool Valido { get; set; }
        public string Mensaje { get; set; }
        public List<string> Diferencias { get; set; } = new();
    }
}