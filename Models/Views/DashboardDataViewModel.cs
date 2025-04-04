using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models.Views
{
    public class DashboardDataViewModel
    {
        public double? TotalVentas { get; set; }
        public int? ProductosVendidos { get; set; }
        public int? ProductosStockBajo { get; set; }
        public int? ClientesAtendidos { get; set; }
    }
}
