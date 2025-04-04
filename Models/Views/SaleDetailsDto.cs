using project_DBA_VISO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Views
{
    public class SaleDetailsDto
    {
        public int DetalleFactura_Id { get; set; }
        public int Factura_Id { get; set; }        
        public string? Image { get; set; }
        public string? Producto_Descripcion { get; set; }
        public int Cliente_Id {  get; set; }
        public string? Cliente_nombre { get; set; }
        public string? Cliente_apellido { get; set; }
        public string? Usuario_nombre { get; set; }
        public DateTime Fecha { get; set; }
        public string? Moneda { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }
        public float Total { get; set; }        
    }
}
