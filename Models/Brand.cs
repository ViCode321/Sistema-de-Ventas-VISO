using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Brand
    {
        public Brand()
        { 
            Product = new List<Product>();
        }

        public int Marca_Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [StringLength(100)]
        [Display(Name = @"Nombre")]
        public string? Nombre { get; set; }
        
        [Display(Name = @"Imagen")]
        public string? Image { get; set; }
        public bool Status { get; set; }
        public List<Product>? Product { get; set; }
    }
}