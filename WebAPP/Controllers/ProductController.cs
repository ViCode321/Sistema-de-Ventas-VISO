using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Services.Contract;
using Rotativa.AspNetCore;

namespace WebAPP.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly DBContext _context;
        private readonly IProductService _productService;
        public ProductController(ILogger<ProductController> logger, DBContext context, IProductService productService)
        {
            _logger = logger;
            _context = context;
            _productService = productService;
        }

        public string route = "/img/images/images_product/";

        public IActionResult Productlist()
        {
            return View(_context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)
                .ToList());
        }

        // Nuevo método para exportar a Excel
        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var products = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Productos");
                var currentRow = 1;

                // Encabezados de la tabla
                worksheet.Cell(currentRow, 1).Value = "Producto_Id";
                worksheet.Cell(currentRow, 2).Value = "Descripcion";
                worksheet.Cell(currentRow, 3).Value = "Proveedor";
                worksheet.Cell(currentRow, 4).Value = "Categoria";
                worksheet.Cell(currentRow, 5).Value = "Marca";
                worksheet.Cell(currentRow, 6).Value = "Cantidad";
                worksheet.Cell(currentRow, 7).Value = "Costo";
                worksheet.Cell(currentRow, 8).Value = "Precio";
                worksheet.Cell(currentRow, 9).Value = "Status";

                // Estilo para los encabezados
                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Datos de la tabla
                foreach (var product in products)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = product.Producto_Id;
                    worksheet.Cell(currentRow, 2).Value = product.Descripcion;
                    worksheet.Cell(currentRow, 3).Value = product.Suppliers?.Nombre;
                    worksheet.Cell(currentRow, 4).Value = product.Categories?.Nombre ?? "N/A";
                    worksheet.Cell(currentRow, 5).Value = product.Brands?.Nombre ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = product.Cantidad;
                    worksheet.Cell(currentRow, 7).Value = product.Costo;
                    worksheet.Cell(currentRow, 8).Value = product.Precio;
                    worksheet.Cell(currentRow, 9).Value = product.Status ? "Activo" : "Inactivo";
                }

                // Ajustar el ancho de las columnas
                worksheet.Columns().AdjustToContents();

                // Guardar el archivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaProductos_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx");
                }
            }
        }
        //PDF
        public IActionResult ExportToPdf()
        {
            var products = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)
                .ToList();
            
            return new ViewAsPdf("ProductPdf", products)
            {
                FileName = $"ListaProductos_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                CustomSwitches = "--page-offset 0 --footer-center [page] --footer-font-size 12"
            };
        }


        public JsonResult ProductlistJson()
        {
            var products = _context.Products
                //.Where(p => p.Status != false)
                .Select(p => new
                {
                    p.Producto_Id,
                    p.Descripcion,
                    p.Cantidad,
                    p.Costo,
                    p.Precio,
                    Image = p.Image, // Si quieres incluir la propiedad Image
                    p.Status,
                    Suppliers = new { p.Suppliers!.Status, p.Suppliers!.Nombre },
                    Categories = new { p.Categories!.Status, p.Categories.Nombre },
                    Brands = new { p.Brands!.Status, p.Brands.Nombre }
                })
                .ToList();

            return Json(products);
        }


        [HttpGet]
        public IActionResult Productdetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)        
                .FirstOrDefault(p => p.Producto_Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View("Productdetails", product);
        }

        public IActionResult Addproduct()
        {

            var categories = _context.Categories
                .Where(c => c.Status != false)
                .Select(c => new { c.Categoria_Id, c.Nombre })
                .ToList();
            var brands = _context.Brands
                .Where(b => b.Status != false)
                .Select(b => new { b.Marca_Id, b.Nombre })
                .ToList();
            var suppliers = _context.Suppliers
                .Where(s => s.Status != false)
                .Select(s => new { s.Proveedor_Id, s.Nombre })
                .ToList();

            ViewData["Categoria_Id"] = new SelectList(categories, "Categoria_Id", "Nombre");

            ViewData["Marca_Id"] = new SelectList(brands, "Marca_Id", "Nombre");

            ViewData["Proveedor_Id"] = new SelectList(suppliers, "Proveedor_Id", "Nombre");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Addproduct(Product obj, IFormFile image)
        {
            var existingProductDescription = _context.Products.FirstOrDefault(p => p.Descripcion == obj.Descripcion);

            if (existingProductDescription != null)
            {
                return Json(new { success = false, message = "Este producto ya existe en la base de datos." });
            }

            ModelState.Remove("Image");
            ModelState.Remove("Fecha");
            if (image == null)
            {
                obj.Image = "/img/images/default/imagen.png";
            }
            else if (image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route ,fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                obj.Image = route + fileName;
            }
            if (ModelState.IsValid)
            {
                obj.Categories = _context.Categories.Find(obj.Categoria_Id);
                obj.Brands = _context.Brands.Find(obj.Marca_Id);
                obj.Suppliers = _context.Suppliers.Find(obj.Proveedor_Id);
                obj.Status = true;

                _context.Add(obj);
                _context.SaveChanges();
                //return RedirectToAction(nameof(Productlist));
                return Json(new { success = true, message = "¡Producto creado con éxito!" });

            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
            //return RedirectToAction(nameof(Addproduct));
        }

        [HttpGet]
        public IActionResult Editproduct(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Producto no encontrada" });
            }

            var product = _context.Products.FirstOrDefault(p => p.Producto_Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = _context.Categories
                .Where(c => c.Status != false)
                .Select(c => new { c.Categoria_Id, c.Nombre })
                .ToList();

            var brands = _context.Brands
                .Where(b => b.Status != false)
                .Select(b => new { b.Marca_Id, b.Nombre })
                .ToList();

            var suppliers = _context.Suppliers
                .Where(s => s.Status != false)
                .Select(s => new { s.Proveedor_Id, s.Nombre })
                .ToList();
            
            ViewBag.Categorias = new SelectList(categories, "Categoria_Id", "Nombre");
            ViewBag.Marcas = new SelectList(brands, "Marca_Id", "Nombre");
            ViewBag.Proveedores = new SelectList(suppliers, "Proveedor_Id", "Nombre");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editproduct(int id, Product obj, IFormFile image)
        {
            ModelState.Remove("Image");
            ModelState.Remove("Fecha");
            if (id != obj.Producto_Id)
            {
                return Json(new { success = false, message = "Producto no encontrada" });
            }

            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Producto_Id == id);

                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Producto no encontrada" });
                }

                existingProduct.Descripcion = obj.Descripcion;
                existingProduct.Categoria_Id = obj.Categoria_Id;
                existingProduct.Categories = _context.Categories.Find(obj.Categoria_Id);
                existingProduct.Marca_Id = obj.Marca_Id;
                existingProduct.Brands = _context.Brands.Find(obj.Marca_Id);
                existingProduct.Proveedor_Id = obj.Proveedor_Id;
                existingProduct.Suppliers = _context.Suppliers.Find(obj.Proveedor_Id);
                existingProduct.Cantidad = obj.Cantidad;
                existingProduct.Costo = obj.Costo;
                existingProduct.Precio = obj.Precio;

                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existingProduct.Image = route + fileName;
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Producto actualizado con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Producto_Id == id);
            if (product != null)
            {
                product.Status = false;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Producto desactivado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo desactivar el producto" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Producto_Id == id);
            if (product != null)
            {
                product.Status = true;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Producto activado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo activar el producto" });
        }
    }
}
