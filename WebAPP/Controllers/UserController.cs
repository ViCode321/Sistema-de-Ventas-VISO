using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Services.Contract;
using project_DBA_VISO.Utilities;
using Rotativa.AspNetCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace WebAPP.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userservice;
        private readonly DBContext _context;

        public UserController(IUserService userservice, DBContext context)
        {
            _userservice = userservice;
            _context = context;
        }

        public IActionResult ExportUsersToExcel()
        {
            var users = _context.Users.Include(u => u.User_Rol).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Usuarios");

                // Headers
                worksheet.Cells[1, 1].Value = "ID Usuario";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Rol";
                worksheet.Cells[1, 4].Value = "Estado";
                // Add more headers as needed

                // Data
                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Usuario_Id;
                    worksheet.Cells[row, 2].Value = user.Nombre;
                    worksheet.Cells[row, 3].Value = user.User_Rol != null ? user.User_Rol.Rol : "";
                    worksheet.Cells[row, 4].Value = user.Status ? "Activo" : "Inactivo";
                    // Add more data as needed
                    row++;
                }

                // Autofit columns
                worksheet.Cells.AutoFitColumns();

                // Estilo para el encabezado
                using (var range = worksheet.Cells["A1:D1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Guardar el archivo
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Usuarios_{ DateTime.Now.ToString("yyyy-MM-dd")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public IActionResult ExportToPdf()
        {
            var users = _context.Users.Include(u => u.User_Rol).ToList();
            return new ViewAsPdf("UsersPdf", users)
            {
                FileName = $"ListaUsuarios_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }


        public string route = "/img/images/images_user/";

        public IActionResult Signin()
        {
            //CreateAdminUser();
            return View(nameof(Signin));
        }

        /*public async Task CreateAdminUser()
        {
            User adminUser = new User
            {
                Nombre = "Sofia",
                Contraseña = Encrypt.Encryptkey("123"),
                Rol_Id = 1,
                Image = "/img/customer/customer1.jpg",
                Status = true
            };
            User userUser = new User
            {
                Nombre = "Victor",
                Contraseña = Encrypt.Encryptkey("123"),
                Rol_Id = 2,
                Image = "/img/customer/customer5.jpg",
                Status = true
            };
            await _userservice.SaveUser(adminUser);
            await _userservice.SaveUser(userUser);
        }
        */
        [HttpPost]
        public async Task<IActionResult> Signin(string username, string key)
        {
            /*Valores quemados agregados*/
            username = "Sofia";
            key = "123";
            //CreateAdminUser();
            string encryptedKey = Encrypt.Encryptkey(key);
            User user = await _userservice.GetUser(username, encryptedKey);
            if (user == null)
            {
                //ViewData["Mensaje"] = "No se encontró el usuario";
                return RedirectToAction("Index", "Home");

//                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Nombre!),
                new Claim(ClaimTypes.Role, user.User_Rol!.Rol!),
                new Claim(ClaimTypes.NameIdentifier, user.Usuario_Id.ToString()),
                new Claim("UserImage", user.Image!) // Asegúrate de tener una propiedad ImageUrl en tu objeto de usuario
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), properties
                );
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Userlist()
        {
            var users = _context.Users
                .Include(u => u.User_Rol)
                .ToList();
            return View(users);
        }

        [Authorize]
        public JsonResult UserlistJson()
        {
            var users = _context.Users
                .Select(u => new
                {
                    u.Usuario_Id,
                    u.Nombre,
                    u.Contraseña,
                    u.Status,
                    Image = u.Image,
                    Roles = new { u.User_Rol!.Rol}                    
                })
                .ToList();
            return Json(users);
        }
        
        [Authorize]
        public IActionResult Newuser()
        {
            var roles = _context.User_Rols
                .Select(c => new { c.Rol_id, c.Rol })
                .ToList();

            ViewData["Rol_id"] = new SelectList(roles, "Rol_id", "Rol");
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Newuser(User user, IFormFile image)
        {
            ModelState.Remove("Image");
            if (image == null)
            {
                user.Image = "/img/images/default/imagen.png";
            }
            else if (image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                user.Image = route + fileName;
            }            
            user.Contraseña = Encrypt.Encryptkey(user.Contraseña!);
            user.Status = true;
            User user_create = await _userservice.SaveUser(user);

            if (user_create.Usuario_Id > 0)
            {
                //return RedirectToAction("Index", "Home");
                return Json(new { success = true, message = "¡Usuario creado con éxito!" });
            }
            //ViewData["No se creo el usuario"] = "No se pudo crear el usuario";
            return Json(new { success = false, message = "¡Error!" });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Edituser(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var user = _context.Users.FirstOrDefault(p => p.Usuario_Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var roles = _context.User_Rols
                .Select(s => new { s.Rol_id, s.Rol })
                .ToList();

            ViewBag.Roles = new SelectList(roles, "Rol_id", "Rol");
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edituser(int id, User user, IFormFile image)
        {
            ModelState.Remove("Image");
            if (id != user.Usuario_Id)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(p => p.Usuario_Id == id);

                if (existingUser == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                existingUser.Nombre = user.Nombre;
                existingUser.Contraseña = Encrypt.Encryptkey(user.Contraseña!);
                existingUser.Rol_Id = user.Rol_Id;
                existingUser.User_Rol = _context.User_Rols.Find(user.Rol_Id);
                existingUser.Status = true;
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existingUser.Image = route + fileName;
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Usuario actualizado con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var user = _context.Users.Include(u => u.User_Rol).FirstOrDefault(p => p.Usuario_Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            ViewBag.Role = user.User_Rol?.Rol;
            ViewBag.Roleid = user.Rol_Id;
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(int id, User user, IFormFile image)
        {
            ModelState.Remove("Image");
            if (id != user.Usuario_Id)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(p => p.Usuario_Id == id);

                if (existingUser == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                existingUser.Nombre = user.Nombre;
                existingUser.Contraseña = Encrypt.Encryptkey(user.Contraseña!);
                existingUser.Rol_Id = user.Rol_Id;
                existingUser.User_Rol = _context.User_Rols.Find(user.Rol_Id);
                existingUser.Status = true;
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existingUser.Image = route + fileName;
                }
                _context.SaveChanges();
                // Cerrar la sesión del usuario
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Crear las nuevas reclamaciones
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Nombre!),
                    new Claim(ClaimTypes.Role, user.User_Rol!.Rol!),
                    new Claim(ClaimTypes.NameIdentifier, user.Usuario_Id.ToString()),
                    new Claim("UserImage", existingUser!.Image!)
                };

                // Iniciar la sesión del usuario de nuevo con las reclamaciones actualizadas
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), properties
                );
                return Json(new { success = true, message = "¡Usuario actualizado con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int id, bool status)
        {
            var user = _context.Users.FirstOrDefault(p => p.Usuario_Id == id);
            if (user != null)
            {
                user.Status = status;
                _context.SaveChanges();
            }
            return Ok(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.FirstOrDefault(p => p.Usuario_Id == id);

            if (user != null)
            {
                _context.Remove(user);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Usuario eliminado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo eliminar la venta" });
        }
    }
}
