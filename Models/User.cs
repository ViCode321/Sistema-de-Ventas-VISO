using project_DBA_VISO.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Models
{
    public class User
    {
        [Display(Name = @"Código")]
        public int Usuario_Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [StringLength(100)]
        [Display(Name = @"Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        [StringLength(500)]
        [Display(Name = @"Contraseña")]
        public string? Contraseña { get; set; }

        [Display(Name = @"Rol")]
        public int Rol_Id { get; set; }

        [ForeignKey("Rol_Id")]
        [Display(Name = @"Rol")]
        public User_Rol? User_Rol { get; set; }

        [Display(Name = @"Imagen")]
        public string? Image { get; set; }

        [Display(Name = @"Estatus")]
        public bool Status { get; set; }
    }
}
