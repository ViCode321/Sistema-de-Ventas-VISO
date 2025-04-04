using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models.Views
{
    public class Invoice_detail_clientViewModel
    {
        public int DetalleFactura_Id { get; set; }
        public int Factura_Id { get; set; }
        public int Producto_Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }
        public float Total_ventas { get; set; }
        public bool Status { get; set; }
    }
}
