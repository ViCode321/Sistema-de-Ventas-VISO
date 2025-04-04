using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models
{
    public class User_Rol
    {
        [Key]
        public int Rol_id { get; set; }

        [Required(ErrorMessage = "El campo Rol es requerido")]
        [StringLength(100)]
        [Display(Name = @"Rol")]
        public string? Rol { get; set; }
    }
}
