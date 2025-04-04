using project_DBA_VISO.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models
{
    public class Purchase_Detail
    {
        [Key]
        [Display(Name = @"Nº Compra")]
        public int DetalleCompra_Id { get; set; }

        [Display(Name = @"Nº Compra")]
        public int Compra_Id { get; set; }

        [ForeignKey("Compra_Id")]
        [Display(Name = @"Nº Compra")]
        public Purchase? Purchases { get; set; }

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

        [Display(Name = @"Proveedor")]
        public int Proveedor_Id { get; set; }

        [ForeignKey("Proveedor_Id")]
        [Display(Name = @"Proveedor")]
        public Supplier? Suppliers { get; set; }

        [Display(Name = @"Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = @"Precio unitario")]
        public float Precio_Unitario { get; set; }

        [Display(Name = @"Total")]
        public float Total { get; set; }
    }
}
