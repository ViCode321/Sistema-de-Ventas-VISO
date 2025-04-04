using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Currency
    {
        public int Moneda_Id { get; set; }

        [Required(ErrorMessage = "El campo Moneda es requerido")]
        [Display(Name = @"Moneda")]
        public string? Tipo { get; set; }
    }
}
