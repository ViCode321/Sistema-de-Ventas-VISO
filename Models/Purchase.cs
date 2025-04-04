using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models
{
    public class Purchase
    {
        public Purchase() 
        {
            Purchase_Details = new List<Purchase_Detail>();        
        }
        
        [Key]
        [Display(Name = @"Nº Compra")]
        public int Compra_Id { get; set; }

        [Display(Name = @"Fecha")]
        public DateTime Fecha {  get; set; }

        [Display(Name = @"Total")]
        public float Total_Compra { get; set; }

        public List<Purchase_Detail>? Purchase_Details { get; set; }
    }

}
