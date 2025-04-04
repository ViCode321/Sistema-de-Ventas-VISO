using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using Rotativa.AspNetCore;

namespace WebAPP.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly DBContext _context;
        public string route = "/img/images/images_category/";

        public CategoryController(ILogger<CategoryController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Categorylist()
        {
            var category = _context.Categories.ToList();
            if (category.Any(c => c == null))
            {
                category = category.Where(c => c != null).ToList();
            }
            return View(category);
        }

        //para exel
        public IActionResult ExportCategoriesToExcel()
        {
            var categories = _context.Categories.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Categorías");

                // Headers
                worksheet.Cell(1, 1).Value = "ID Categoría";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "Estado";

                // Color de fondo y estilo para el encabezado
                var headerRange = worksheet.Range("A1:C1");
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.Bold = true;

                // Data
                int row = 2;
                foreach (var category in categories)
                {
                    worksheet.Cell(row, 1).Value = category.Categoria_Id;
                    worksheet.Cell(row, 2).Value = category.Nombre;
                    worksheet.Cell(row, 3).Value = category.Status ? "Activa" : "Inactiva";
                    row++;
                }

                // Autoajuste de columnas
                worksheet.Columns().AdjustToContents();

                // Guardar el archivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaCategoria_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx");
                }
            }
        }

        // Método para exportar a PDF
        public IActionResult ExportCategoriesToPdf()
        {
            var categories = _context.Categories.ToList();
            return new ViewAsPdf("CategoryPdf", categories)
            {
                FileName = $"ListaCategoria_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }

        public JsonResult CategorylistJson()
        {
            var category = _context.Categories.ToList();
            if (category.Any(c => c == null))
            {
                category = category.Where(c => c != null).ToList();
            }
            return Json(category);
        }

        public IActionResult Addcategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Addcategory(Category obj, IFormFile image)
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
                _context.Categories.Add(obj);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Categoría creado con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [HttpGet]
        public IActionResult Editcategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = _context.Categories.FirstOrDefault(b => b.Categoria_Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editcategory(int id, Category obj, IFormFile image)
        {
            ModelState.Remove("Image");
            if (id != obj.Categoria_Id)
            {
                return Json(new { success = false, message = "Categoría no encontrada" });
            }
            if (ModelState.IsValid)
            {
                var existingcategory = _context.Categories.FirstOrDefault(b => b.Categoria_Id == id);
                if (existingcategory == null)
                {
                    return Json(new { success = false, message = "Categoría no encontrada" });
                }
                existingcategory.Nombre = obj.Nombre;
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existingcategory.Image = route + fileName;
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Categoría actualizada con éxito!" });

            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Categoria_Id == id);
            if (category != null)
            {
                category.Status = false;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Categoría desactivada correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo desactivadar la categoría" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateConfirmed(int id) 
        {
            var category = _context.Categories.FirstOrDefault(c => c.Categoria_Id == id);
            if (category != null)
            {
                category.Status = true;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Categoría activada correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo activar la categoría" });
        }
    }
}
