namespace project_DBA_VISO.Models.Views
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int StatusCode { get; set; } // Propiedad para almacenar el código de estado HTTP
    }
}
