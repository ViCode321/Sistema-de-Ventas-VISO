using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using Rotativa.AspNetCore;
using static System.Net.Mime.MediaTypeNames;

namespace WebAPP.Controllers
{
    [Authorize]
    public class PeopleController : Controller
    {
        private readonly DBContext _context;
        
        public string route = "/img/images/images_supplier/";

        public PeopleController(DBContext context)
        {
            _context = context;
        }

        
        public IActionResult ExportClientsToExcel()
        {
            var clients = _context.Clients.ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Clientes");

                // Headers
                worksheet.Cells[1, 1].Value = "ID Cliente";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Apellido";
                // Add more headers as needed

                // Data
                int row = 2;
                foreach (var client in clients)
                {
                    worksheet.Cells[row, 1].Value = client.Cliente_Id;
                    worksheet.Cells[row, 2].Value = client.Nombre;
                    worksheet.Cells[row, 3].Value = client.Apellido;
                    
                    // Add more data as needed
                    row++;
                }

                // Autofit columns
                worksheet.Cells.AutoFitColumns();

                // Estilo para el encabezado
                using (var range = worksheet.Cells["A1:C1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Guardar el archivo
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Clientes_{DateTime.Now.ToString("yyyy-MM-dd")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public IActionResult ExportClientsToPdf()
        {
            var clients = _context.Clients.ToList();
            return new ViewAsPdf("ClientsPdf", clients)
            {
                FileName = $"ListaClientes_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }

        public IActionResult ExportSuppliersToPdf()
        {
            var suppliers = _context.Suppliers.Include(s => s.Phones).ToList();
            return new ViewAsPdf("SuppliersPdf", suppliers)
            {
                FileName = $"ListaProveedores_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }
        public IActionResult ExportSuppliersToExcel()
        {
            var suppliers = _context.Suppliers.Include(s => s.Phones).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Proveedores");

                // Headers
                worksheet.Cells[1, 1].Value = "ID Proveedor";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Teléfono";
                // Add more headers as needed

                // Data
                int row = 2;
                foreach (var supplier in suppliers)
                {
                    worksheet.Cells[row, 1].Value = supplier.Proveedor_Id;
                    worksheet.Cells[row, 2].Value = supplier.Nombre;
                    worksheet.Cells[row, 3].Value = string.Join(", ", supplier.Phones.Select(p => p.Numero));
                    // Add more data as needed
                    row++;
                }

                // Autofit columns
                worksheet.Cells.AutoFitColumns();

                // Estilo para el encabezado
                using (var range = worksheet.Cells["A1:C1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Guardar el archivo
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Proveedores_{DateTime.Now.ToString("yyyy-MM-dd")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public IActionResult Clientlist()
        {
            var clients = _context.Clients.ToList();
            if (clients.Any(c => c == null))
            {
                clients = clients.Where(c => c != null).ToList();
            }
            return View(clients);
        }

        public JsonResult ClientlistJson()
        {
            var clients = _context.Clients.ToList();
            if (clients.Any(c => c == null))
            {
                clients = clients.Where(c => c != null).ToList();
            }
            return Json(clients);
        }

        public IActionResult Addclient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Addclient(Client obj)
        {
            if (ModelState.IsValid)
            {
                obj.Status = true;
                _context.Clients.Add(obj);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Cliente creado con éxito!" });
            }
            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedClient(int id)
        {
            var cliente = _context.Clients.FirstOrDefault(b => b.Cliente_Id == id);
            if (cliente != null)
            {
                cliente.Status = false;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Cliente desactivado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo desactivadar el estado del cliente" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateConfirmedClient(int id)
        {
            var cliente = _context.Clients.FirstOrDefault(b => b.Cliente_Id == id);
            if (cliente != null)
            {
                cliente.Status = true;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Cliente activado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo activadar el estado del cliente" });
        }


        public IActionResult Supplierlist()
        {
            var suppliers = _context.Suppliers.Include(s => s.Phones).ToList();
            return View(suppliers);
        }

        public JsonResult SupplierlistJson()
        {
            var suppliers = _context.Suppliers.Include(s => s.Phones).ToList();
            return Json(suppliers);
        }

        public IActionResult Addsupplier()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Addsupplier(Supplier obj, List<string> phones, IFormFile image)
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

            if (phones != null)
            {
                foreach (var phone in phones)
                {
                    obj.Phones.Add(new Supplier_Phone { Numero = phone });
                }
            }
            else
            {
                return Json(new { success = false, message = "Los datos ingresados son erróneos" });
            }

            if (ModelState.IsValid)
            {
                obj.Status = true;
                _context.Suppliers.Add(obj);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Proveedor creado con éxito!" });
            }

            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }

        [HttpGet]
        public IActionResult Editsupplier(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Proveedor no encontrado" });
            }

            var supplier = _context.Suppliers
                .Include(s => s.Phones)
                .FirstOrDefault(p => p.Proveedor_Id == id);

            if (supplier == null)
            {
                return Json(new { success = false, message = "Proveedor no encontrado" });
            }
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editsupplier(Supplier obj, List<string> phones, IFormFile image)
        {
            ModelState.Remove("Image");

            var supplierToUpdate = _context.Suppliers
                .Include(s => s.Phones)
                .FirstOrDefault(p => p.Proveedor_Id == obj.Proveedor_Id);

            if (supplierToUpdate == null)
            {
                return Json(new { success = false, message = "Proveedor no encontrado" });
            }

            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + route, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                supplierToUpdate.Image = route + fileName;
            }

            supplierToUpdate.Nombre = obj.Nombre;
            supplierToUpdate.Direccion = obj.Direccion;
            supplierToUpdate.Status = true;

            supplierToUpdate.Phones.Clear();
            if (phones != null)
            {
                foreach (var phone in phones)
                {
                    supplierToUpdate.Phones.Add(new Supplier_Phone { Numero = phone });
                }
            }

            if (ModelState.IsValid)
            {
                _context.Suppliers.Update(supplierToUpdate);
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Proveedor actualizado con éxito!" });
            }

            return Json(new { success = false, message = "Los datos ingresados son erróneos" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedSupplier(int id)
        {
            var supplier = _context.Suppliers.FirstOrDefault(b => b.Proveedor_Id == id);
            if (supplier != null)
            {
                supplier.Status = false;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Proveedor desactivado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo desactivadar el estado del cliente" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivateConfirmedSupplier(int id)
        {
            var supplier = _context.Suppliers.FirstOrDefault(b => b.Proveedor_Id == id);
            if (supplier != null)
            {
                supplier.Status = true;
                _context.SaveChanges();
                return Json(new { success = true, message = "¡Proveedor activado correctamente!" });
            }
            return Json(new { success = false, message = "No se pudo activadar el estado del cliente" });
        }

    }
}

