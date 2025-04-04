using ClosedXML.Excel;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Models;
using Rotativa.AspNetCore;

namespace WebAPP.Controllers
{
    [Authorize]
    public class BrandController : Controller
    {
        private readonly ILogger<BrandController> _logger;
        private readonly DBContext _context;

        public string route = "/img/images/images_brand/";

        public BrandController(ILogger<BrandController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Brandlist()
        {
            var brands = _context.Brands.ToList();
            if (brands.Any(b => b == null))
            {
                brands = brands.Where(b => b != null).ToList();
            }
            return View(brands);
        }

        public JsonResult BrandlistJson()
        {
            var brands = _context.Brands.ToList();
            if (brands.Any(b => b == null))
            {
                brands = brands.Where(b => b != null).ToList();
            }
            return Json(brands);
        }

        public IActionResult Addbrand()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Addbrand(Brand obj, IFormFile image)
        {
            ModelState.Remove("Image");
            if (image == null)
            {
                obj.Image = "/img/images/default/imagen.png";
            }
            else if (image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                obj.Image = route + fileName;
            }
            if (ModelState.IsValid)
            {
                obj.Status = true;
                _context.Brands.Add(obj);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Marca creada con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [HttpGet]
        public IActionResult Editbrand(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var brand = _context.Brands.FirstOrDefault(b => b.Marca_Id == id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editbrand(int id, Brand obj, IFormFile image)
        {
            ModelState.Remove("Image");
            if (id != obj.Marca_Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingBrand = _context.Brands.FirstOrDefault(b => b.Marca_Id == id);
                if (existingBrand == null)
                {
                    return NotFound();
                }
                existingBrand.Nombre = obj.Nombre;
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existingBrand.Image = route + fileName;
                }
                else
                {
                    existingBrand.Image = "/img/images/default/imagen.png";
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Marca actualizada con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.Marca_Id == id);
            if (brand != null)
            {
                brand.Status = false;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Marca desactivada correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo desactivar la marca" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateConfirmed(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.Marca_Id == id);
            if (brand != null)
            {
                brand.Status = true;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Marca activada correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo activar la marca" });
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var brands = _context.Brands.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Marcas");
                var currentRow = 1;

                // Encabezados de la tabla
                worksheet.Cell(currentRow, 1).Value = "Marca_Id";
                worksheet.Cell(currentRow, 2).Value = "Nombre";
                worksheet.Cell(currentRow, 3).Value = "Estado";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;


                // Datos de la tabla
                foreach (var brand in brands)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = brand.Marca_Id;
                    worksheet.Cell(currentRow, 2).Value = brand.Nombre;
                    worksheet.Cell(currentRow, 3).Value = brand.Status ? "Activo" : "Inactivo";
                    
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaMarcas_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult ExportToPdf()
        {
            var brands = _context.Brands.ToList();
            return new ViewAsPdf("BrandPdf", brands)
            {
                FileName = $"ListaMarcas_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }
    }
}
