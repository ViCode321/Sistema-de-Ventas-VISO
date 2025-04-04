using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace project_DBA_VISO.Models
{
    public class Invoice
    {
        public Invoice()
        {
            Invoice_detail = new List<Invoice_Detail>();
        }

        [Key]
        [Display(Name = @"Factura")]
        public int Factura_Id { get; set; }

        [Display(Name = @"Fecha")]
        public DateTime Fecha { get; set; }

        [Display(Name = @"Cliente")]
        public int Cliente_Id { get; set; }

        [ForeignKey("Cliente_Id")] // Especifica la clave foránea
        [Display(Name = @"Cliente")]
        public Client? Clients { get; set; }

        [Display(Name = @"Monto final")]
        public float Monto_final { get; set; }

        [Display(Name = @"Moneda")]
        public int Moneda_Id { get; set; }

        [ForeignKey("Moneda_Id")] // Especifica la clave foránea
        [Display(Name = @"Moneda")]
        public Currency? Currencies { get; set; }
        
        //public bool? Status {  get; set; }
        public List<Invoice_Detail>? Invoice_detail { get; set; }

    }
}
