using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Invoice_Detail
    {
        [Required]
        
        [Display(Name = @"Factura")]
        public int DetalleFactura_Id {  get; set; }

        [Display(Name = @"Factura")]
        public int Factura_Id { get; set; }

        [ForeignKey("Factura_Id")]
        [Display(Name = @"Factura")]
        public Invoice? Invoices { get; set; }

        [Display(Name = @"Usuario")]
        public int Usuario_Id { get; set; }

        [ForeignKey("Usuario_Id")]
        [Display(Name = @"Usuario")]
        public User? Users { get; set; }

        [Display(Name = @"Producto")]
        public int Producto_Id { get; set; }

        [ForeignKey("Producto_Id")]
        [Display(Name = @"Producto")]
        public Product? Products { get; set; }
        
        [Display(Name = @"Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = @"Precio")]
        public float Precio { get; set; }

        [Display(Name = @"Total de Venta")]
        public float Total_ventas {  get; set; }
        
        [Display(Name = @"Estatus")]
        public bool? Status {  get; set; }
    }
}
