using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace project_DBA_VISO.Models

{
    public class Product
    {
        
        [Required]
        [Display(Name = @"Código")]
        public int Producto_Id { get; set; }

        [Display(Name = @"Proveedor")]
        public int Proveedor_Id { get; set; }

        [ForeignKey("Proveedor_Id")]
        [Display(Name = @"Proveedor")]
        public Supplier? Suppliers { get; set; }

        [Required(ErrorMessage = "El campo Descripción es requerido")]
        [StringLength(200)]
        [Display(Name = @"Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = @"Categoría")]
        public int Categoria_Id { get; set; }

        [ForeignKey("Categoria_Id")]
        [Display(Name = @"Categoría")]
        public Category? Categories { get; set; }

        [Required(ErrorMessage = "El campo Cantidad es requerido")]
        [Display(Name = @"Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El campo Costo es requerido")]
        [Display(Name = @"Costo")]
        public float Costo { get; set; }

        [Required(ErrorMessage = "El campo Precio es requerido")]
        [Display(Name = @"Precio")]
        public float Precio { get; set; }

        [Display(Name = @"Marca")]
        public int Marca_Id { get; set; }

        [ForeignKey("Marca_Id")]
        [Display(Name = @"Marca")]
        public Brand? Brands { get; set; }
        
        [Display(Name = @"Imagen")]
        public string? Image { get; set; }

        [Display(Name = @"Estado")]
        public bool Status { get; set; }

       

    }

   

}
