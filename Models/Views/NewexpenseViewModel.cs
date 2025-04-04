using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models.Views
{
    public class NewexpenseViewModel
    {
        public List<Product> Products { get; set; }

        public List<Purchase_Detail> Purchase_Details { get; set; }

        public List<Purchase> Purchases { get; set; }       

        public List<User> Users { get; set; }

        public List<Supplier> Suppliers { get; set; }

    }
}
