using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models.Views;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Models.Views;
using project_DBA_VISO.Services.Contract;
using System.Security.Claims;
using Rotativa.AspNetCore;

namespace WebAPP.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly ILogger<PurchaseController> _logger;
        private readonly DBContext _context;
        private readonly IProductService _productService;

        public PurchaseController(ILogger<PurchaseController> logger, DBContext context, IProductService productService)
        {
            _logger = logger;
            _context = context;
            _productService = productService;
        }

        public IActionResult Purchaselist()
        {
            return View(_context.Purchase_Details
                .Include(p => p.Purchases)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Include(s => s.Suppliers)
                .ToList());
        }

        public IActionResult ExportToExcel()
        {
            var purchaseDetails = _context.Purchase_Details
                .Include(p => p.Purchases)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Include(s => s.Suppliers)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Purchase Details");

                // Headers
                worksheet.Cells[1, 1].Value = "N° Compra";
                worksheet.Cells[1, 2].Value = "Compra";
                worksheet.Cells[1, 3].Value = "Cantidad";
                worksheet.Cells[1, 4].Value = "Precio Unitario";
                worksheet.Cells[1, 5].Value = "Total";
                worksheet.Cells[1, 6].Value = "Fecha";
                worksheet.Cells[1, 7].Value = "Proveedor";
                worksheet.Cells[1, 8].Value = "Usuario";

                // Data
                int row = 2;
                foreach (var detail in purchaseDetails)
                {
                    worksheet.Cells[row, 1].Value = detail.DetalleCompra_Id;
                    worksheet.Cells[row, 2].Value = detail.Products!.Descripcion;
                    worksheet.Cells[row, 3].Value = detail.Cantidad;
                    worksheet.Cells[row, 4].Value = detail.Precio_Unitario;
                    worksheet.Cells[row, 5].Value = detail.Total;
                    worksheet.Cells[row, 6].Value = detail.Purchases!.Fecha.ToShortDateString();
                    worksheet.Cells[row, 7].Value = detail.Suppliers!.Nombre;
                    worksheet.Cells[row, 8].Value = detail.Users!.Nombre;
                    row++;
                }

                // Autofit columns
                worksheet.Cells.AutoFitColumns();

                // Estilo para el encabezado
                using (var range = worksheet.Cells["A1:H1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Guardar el archivo
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ListaCompra_{DateTime.Now.ToString("yyyy-MM-dd")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public IActionResult ExportToPdf()
        {
            var purchaseDetails = _context.Purchase_Details
                .Include(p => p.Purchases)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Include(s => s.Suppliers)
                .ToList();
            return new ViewAsPdf("PurchasePdf", purchaseDetails)
            {
                FileName = $"ListaCompas_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }

        public IActionResult Newpurchase()
        {
              
            var supplier = _context.Suppliers
                .Select(s => new { s.Proveedor_Id, s.Nombre })
                .ToList();

            var products = _context.Products
                .Select(p => new { p.Producto_Id, p.Descripcion })
                .ToList();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            ViewData["userId"] = userId;

            ViewBag.Proveedores = new SelectList(supplier, "Proveedor_Id", "Nombre");
            ViewBag.Productos = new SelectList(products, "Producto_Id", "Descripcion");

            return View(nameof(Newpurchase));
        }

        [HttpPost]
        public IActionResult Newpurchase([FromBody] NewexpenseViewModel newExpense)
        {
            if (newExpense == null)
            {
                return Json(new { success = false, message = "Datos de compra inválidos." });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newPurchase = new Purchase
                    {
                        Fecha = DateTime.Now,
                        Total_Compra = newExpense.Purchase_Details.Sum(d => d.Total),
                    };

                    _context.Purchases.Add(newPurchase);
                    _context.SaveChanges();

                    foreach (var detail in newExpense.Purchase_Details)
                    {
                        var newDetalleCompra = new Purchase_Detail
                        {
                            Compra_Id = newPurchase.Compra_Id,
                            Usuario_Id = newExpense.Users.First().Usuario_Id,
                            Producto_Id = detail.Producto_Id,
                            Cantidad = detail.Cantidad,
                            Precio_Unitario = detail.Precio_Unitario,
                            Total = detail.Total,
                            Proveedor_Id = detail.Proveedor_Id
                        };

                        _context.Purchase_Details.Add(newDetalleCompra);
                        _productService.UpdateAmountProduct(detail.Producto_Id, detail.Cantidad, "sumar");
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return Json(new { success = true, message = "¡Compra realizada con éxito!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Error al procesar la Compra." });
                }
            }
        }


        /*[HttpPost]
        public IActionResult Newpurchase([FromBody] NewexpenseViewModel newExpense)
        {
            if (newExpense == null)
            {
                return Json(new { success = false, message = "Datos de compra inválidos." });

            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newPurchase = new Purchase
                    {
                        Fecha = DateTime.Now,
                        Total_Compra = newExpense.Purchases.First().Total_Compra,
                        Proveedor_Id = newExpense.Purchases.First().Proveedor_Id
                    };
                    _context.Purchases.Add(newPurchase);
                    _context.SaveChanges();

                    foreach (var detail in newExpense.Purchase_Details)
                    {
                        var newDetalleCompra = new Purchase_Detail
                        {
                            Compra_Id = newPurchase.Compra_Id,
                            Usuario_Id = newExpense.Users.First().Usuario_Id,
                            Producto_Id = detail.Producto_Id,
                            Cantidad = detail.Cantidad,
                            Precio_Unitario = detail.Precio_Unitario,
                            Total = detail.Total
                        };
                        _context.Purchase_Details.Add(newDetalleCompra);
                        _productService.UpdateAmountProduct(detail.Producto_Id, detail.Cantidad, "sumar");
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return Json(new { success = true, message = "¡Compra realizada con éxito!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    //Console.WriteLine("Error al procesar la venta" + ex.Message);
                    return Json(new { success = false, message = "Error al procesar la Compra." });

                    //return BadRequest("Error al procesar la venta");
                }
            }
            //return Ok();
        }
        */

        /*[HttpGet]
        public IActionResult Editpurchase(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseDetails = _context.Purchase_Details
                .Include(p => p.Purchases)
                    //.ThenInclude(s => s.Suppliers)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Where(p_d => p_d.Compra_Id == id)
                .ToList();
            if (purchaseDetails == null || !purchaseDetails.Any())
            {
                return NotFound();
            }

            var numeroCompra = ObtenerNumeroCompra(id);

            var supplier = _context.Suppliers
                .Select(s => new { s.Proveedor_Id, s.Nombre })
                .ToList();

            var products = _context.Products
                .Select(p => new { p.Producto_Id, p.Descripcion })
                .ToList();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            ViewData["userId"] = userId;

            ViewBag.Proveedores = new SelectList(supplier, "Proveedor_Id", "Nombre");
            ViewBag.Productos = new SelectList(products, "Producto_Id", "Descripcion");
            ViewBag.Compra_Id = numeroCompra;
            return View("Editpurchase", purchaseDetails);

        }
        */

        [HttpGet]
        public IActionResult ObtenerMontoFinal(int? id)
        {
            if (id == null)
            {
                return BadRequest("El id de la compra es nulo");
            }

            var compra = _context.Purchases.FirstOrDefault(f => f.Compra_Id == id);
            if (compra != null)
            {
                return Ok(compra.Total_Compra);
            }

            return NotFound("No se encontró la factura con el id proporcionado");
        }
        
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editpurchase(Purchase_Detail obj)
        {
            if (obj == null)
            {
                return Json(new { success = false, message = "Los datos ingresados son erroneos" });
            }

            int Compra_Id = obj.Compra_Id;

            if (ModelState.IsValid)
            {
                var existingSale = _context.Purchase_Details.FirstOrDefault(i => i.Compra_Id == Compra_Id);
                if (existingSale == null)
                {
                    return Json(new { success = false, message = "Compra no encontrada" });
                }
                if (obj.Purchases != null)
                {
                    var existingPurchase = _context.Purchases.FirstOrDefault(inv => inv.Compra_Id == obj.Compra_Id);
                    if (existingPurchase != null)
                    {
                        existingSale.Purchases = existingSale.Purchases;
                        existingSale!.Purchases!.Fecha = obj.Purchases.Fecha;
                        existingSale!.Purchases.Total_Compra = obj.Purchases.Total_Compra;
                        //existingSale.Purchases.Proveedor_Id = obj.Purchases.Proveedor_Id;
                    }
                }
                // Actualizar los detalles de la venta con los valores del objeto pasado
                existingSale.Usuario_Id = obj.Usuario_Id;
                existingSale.Producto_Id = obj.Producto_Id;
                existingSale.Cantidad = obj.Cantidad;
                existingSale.Precio_Unitario = obj.Precio_Unitario;
                existingSale.Total = obj.Total;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Compra actualizada correctamente" });
            }

            return Json(new { success = false, message = "Los datos ingresados son erroneos" });
        }
        */

        private int ObtenerNumeroCompra(int? id)
        {
            var purchase = _context.Purchases.FirstOrDefault(i => i.Compra_Id == id);
            if (purchase != null)
            {
                return purchase.Compra_Id;
            }
            return 0;
        }

        [HttpGet]
        public IActionResult GetPurchaseDetails(int? id)
        {
            var purchaseDetails = _context.Purchase_Details
                .Include(p => p.Purchases)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Include(s => s.Suppliers)
                .Where(p_d => p_d.Compra_Id == id)
                .Select(p => new PurchaseDetailsDto
                {
                    DetalleCompra_Id = p.DetalleCompra_Id,
                    Compra_Id = p.Compra_Id,
                    Fecha = p.Purchases!.Fecha,
                    Proveedor_Nombre = p.Suppliers!.Nombre,
                    Cantidad = p.Cantidad,
                    Precio_Unitario = p.Precio_Unitario,
                    Total = p.Total,
                    Producto_Descripcion = p.Products!.Descripcion
                })
                .ToList();

            return Json(purchaseDetails);
        }

        public JsonResult PurchaselistJson()
        {
            var purchaseDetails = _context.Purchase_Details
                .Select(p => new PurchaseDetailsDto
                {
                    DetalleCompra_Id = p.DetalleCompra_Id,
                    Compra_Id = p.Compra_Id,
                    Fecha = p.Purchases!.Fecha,
                    Proveedor_Nombre = p.Suppliers!.Nombre,
                    Cantidad = p.Cantidad,
                    Precio_Unitario = p.Precio_Unitario,
                    Total = p.Total,
                    Producto_Descripcion = p.Products!.Descripcion
                })
                .ToList();
            return Json(purchaseDetails);        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var purchaseDetail = _context.Purchase_Details.FirstOrDefault(p => p.DetalleCompra_Id == id);
            if (purchaseDetail != null)
            {
                _context.Purchase_Details.Remove(purchaseDetail);
                _productService.UpdateAmountProduct(purchaseDetail.Compra_Id, (int)purchaseDetail.Total, "restar_compra");
                _context.SaveChanges();
                return Json(new { success = true, message = "Compra anulada correctamente" });
            }
            return Json(new { success = false, message = "No se pudo eliminar el venta" });
        }



    }
}
