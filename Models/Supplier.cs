using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Supplier
    {
        public Supplier()
        {
            Product = new List<Product>();
            Phones = new List<Supplier_Phone>();
        }
        
        [Display(Name = @"Código")]
        public int Proveedor_Id { get; set; }

        [StringLength(100)]
        [Display(Name = @"Nombre")]
        public string? Nombre { get; set; }

        [StringLength(100)]
        [Display(Name = @"Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = @"Logo")]
        public string? Image { get; set; }
        
        [Display(Name = @"Estado")]
        public bool Status {  get; set; }

        public List<Product> Product { get; set; }
        public List<Supplier_Phone> Phones { get; set; }

    }
}