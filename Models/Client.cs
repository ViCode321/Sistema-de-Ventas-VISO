using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Client
    {
        [Display(Name = @"Código")]
        public int Cliente_Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [Display(Name = @"Nombre")]

        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido es requerido")]
        [Display(Name = @"Apellido")]
        public string? Apellido { get; set; }
        
        [Display(Name = @"Estatus")]
        public bool Status {  get; set; }
    }
}
