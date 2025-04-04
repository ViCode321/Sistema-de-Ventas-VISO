using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using project_DBA_VISO.Models.Views;

namespace project_DBA_VISO.Models.Views
{
    /*    public class ProductViewModel
        {
            public int Producto_Id { get; set; }
            public string Image { get; set; }
            public string Descripcion { get; set; }
            public float Precio { get; set; }
            public int Categoria_Id { get; set; }
            public Category? Categories { get; set; }
            public int Marca_Id { get; set; }
            public Brand? Brands { get; set; }
            public bool Status {  get; set; }
        }*/
    public class ProductViewModel
    {
        public int Producto_Id { get; set; }
        public string Image { get; set; }
        public string Descripcion { get; set; }
        public float Precio { get; set; }
        public string CategoriaNombre { get; set; }
        public string MarcaNombre { get; set; }
        public bool Status { get; set; }
    }

}
