using project_DBA_VISO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models.Views
{
    public class NewsalesViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Invoice_Detail> Invoice_details { get; set; }
        public List<Product> Products { get; set; }
        public List<Invoice> Invoices { get; set; }
        public List<User> Users { get; set; }
    }


}
