using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Views
{
    public class PurchaseDetailsDto
    {
        public int DetalleCompra_Id { get; set; }
        public int Compra_Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? Proveedor_Nombre { get; set; }
        public int Cantidad { get; set; }
        public float Precio_Unitario { get; set; }
        public float Total { get; set; }
        //public float Total_Compra { get; set; }       
        public string? Producto_Descripcion { get; set; }
    }

}
