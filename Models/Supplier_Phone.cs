using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Supplier_Phone
    {
        [Display(Name = @"Código")]
        public int Id { get; set; }

        [Display(Name = @"Proveedor")]
        public int Proveedor_Id { get; set; }
        
        [ForeignKey("Proveedor_Id")]
        [Display(Name = @"Proveedor")]
        public Supplier? Suppliers { get; set; }

        [Required(ErrorMessage = "El campo número es requerido")]
        [Display(Name = @"Número")]
        public string? Numero { get; set; }
    }
}
