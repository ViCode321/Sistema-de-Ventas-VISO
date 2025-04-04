using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models.Views;
using project_DBA_VISO.Models;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Models.Views;
using project_DBA_VISO.Services.Contract;
using Rotativa.AspNetCore;
using System.Globalization;
using System.Security.Claims;

namespace WebAPP.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ILogger<SalesController> _logger;
        private readonly DBContext _context;
        private readonly IProductService _productService;

        public SalesController(ILogger<SalesController> logger, DBContext context, IProductService productService)
        {
            _logger = logger;
            _context = context;
            _productService = productService;
        }

        public IActionResult Saleslist()
        {
            return View(_context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                    .Include(u => u.Users)
                .Where(i => i.Status != false)
                .ToList());
        }
        
    
    public IActionResult Salesdetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

             var salesDetails = _context.Invoice_details
                 .Include(i => i.Invoices)
                     .ThenInclude(inv => inv!.Clients)
                 .Include(i => i.Invoices)
                     .ThenInclude(inv => inv!.Currencies)
                 .Include(p => p.Products)
                 .Where(i => i.Factura_Id == id && i.Status != false)
                 .ToList();
            // Obtener los detalles de la venta ordenados por el ID de la factura
            


            if (salesDetails == null || !salesDetails.Any())
            {
                return NotFound();
            }
            var numeroFactura = ObtenerNumeroFactura(id);

            var factura = _context.Invoices.FirstOrDefault(f => f.Factura_Id == id);
            var montoFinalVenta = factura != null ? factura.Monto_final : 0;

            ViewBag.MontoFinalVenta = montoFinalVenta;
            ViewBag.Factura_Id = numeroFactura;

            return View("Salesdetails", salesDetails);
        }

        

        [HttpGet]
        public IActionResult Editsales(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saleDetail = _context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                .Include(u => u.Users)
                .Where(i => i.Factura_Id == id && i.Status != false)
                .ToList();

            if (saleDetail == null || !saleDetail.Any())
            {
                return NotFound();
            }

            var numeroFactura = ObtenerNumeroFactura(id);

            var invoices = _context.Invoices
                .Select(p => new { p.Factura_Id })
                .ToList();

            var products = _context.Products
                .Select(p => new { p.Producto_Id, p.Descripcion })
                .ToList();

            var currencies = _context.Currencies
                .Select(c => new { c.Moneda_Id, c.Tipo })
                .ToList();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            ViewData["userId"] = userId;

            ViewBag.Productos = new SelectList(products, "Producto_Id", "Descripcion");
            ViewBag.Monedas = new SelectList(currencies, "Moneda_Id", "Tipo");
            ViewBag.Invoices = new SelectList(invoices, "Factura_Id");
            ViewBag.Factura_Id = numeroFactura;

            return View("Editsales", saleDetail);
        }

        public IActionResult ExportToExcel()
        {
            var sales = _context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                .Where(i => i.Status != false)
                    .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ventas");
                var currentRow = 1;

                // Encabezados de la tabla
                worksheet.Cell(currentRow, 1).Value = "Factura";
                worksheet.Cell(currentRow, 2).Value = "Cliente";
                worksheet.Cell(currentRow, 3).Value = "Producto";
                worksheet.Cell(currentRow, 4).Value = "Cantidad";
                worksheet.Cell(currentRow, 5).Value = "Precio Unitario";
                worksheet.Cell(currentRow, 6).Value = "Total";

                // Estilo para los encabezados
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Datos de la tabla
                foreach (var sale in sales)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = sale.DetalleFactura_Id;
                    worksheet.Cell(currentRow, 2).Value = sale.Invoices?.Clients?.Nombre;
                    worksheet.Cell(currentRow, 3).Value = sale.Products?.Descripcion;
                    worksheet.Cell(currentRow, 4).Value = sale.Cantidad;
                    worksheet.Cell(currentRow, 5).Value = sale.Precio;
                    worksheet.Cell(currentRow, 6).Value = sale.Total_ventas;
                }

                // Ajustar el ancho de las columnas
                worksheet.Columns().AdjustToContents();

                // Guardar el archivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ventas_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx");

                }
            }
        }

        public IActionResult ExportToPdf()
        {
            var sales = _context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                .Where(i => i.Status == true)
                    .ToList();

            return new ViewAsPdf("SalePdf", sales)
            {
                FileName = $"ListaVentas_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page] / [topage]\" --footer-line --footer-font-size \"12\" --footer-spacing 5 --footer-font-name \"Segoe UI\""
            };
        }




        private int ObtenerNumeroFactura(int? id)
        {
            // Lógica para obtener el número de factura basado en el ID, por ejemplo:
            var invoice = _context.Invoices.FirstOrDefault(i => i.Factura_Id == id);
            if (invoice != null)
            {
                return invoice.Factura_Id;
            }
            return 0;
        }

        [HttpGet]
        public IActionResult ObtenerMontoFinal(int? id)
        {
            if (id == null)
            {
                return BadRequest("El id de la factura es nulo");
            }

            var factura = _context.Invoices.FirstOrDefault(f => f.Factura_Id == id);
            if (factura != null)
            {
                return Ok(factura.Monto_final);
            }

            return NotFound("No se encontró la factura con el id proporcionado");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editsales(int id, Invoice_Detail obj)
        {
            int Factura_Id = Convert.ToInt32(Request.Form["Factura_Id"]);

            if (id != Factura_Id)
            {
                return Json(new { success = false, message = "Venta no encontrada" });
            }
            ModelState.Remove("Invoices.Fecha");
            ModelState.Remove("Invoices.Moneda_Id");
            if (ModelState.IsValid)
            {
                var existingSale = _context.Invoice_details.FirstOrDefault(i => i.Factura_Id == id);
                if (existingSale == null)
                {
                    return Json(new { success = false, message = "Venta no encontrada" });
                }
                if (obj.Invoices != null)
                {
                    var existingInvoice = _context.Invoices.FirstOrDefault(inv => inv.Factura_Id == obj.Factura_Id);
                    if (existingInvoice != null)
                    {
                        existingSale.Invoices = existingSale.Invoices;
                        existingSale.Invoices!.Cliente_Id = obj.Invoices.Cliente_Id;
                        var fecha = obj.Invoices.Fecha;
                        DateTime currentDate = DateTime.Now;
                        string formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
                        fecha = DateTime.ParseExact(formattedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        existingSale.Invoices.Fecha = fecha;
                        existingSale.Invoices.Monto_final = obj.Invoices.Monto_final;
                    }
                }
                // Actualizar los detalles de la venta con los valores del objeto pasado
                existingSale.Usuario_Id = obj.Usuario_Id;
                existingSale.Producto_Id = obj.Producto_Id;
                existingSale.Cantidad = obj.Cantidad;
                existingSale.Precio = obj.Precio;
                existingSale.Total_ventas = obj.Total_ventas;

                _context.SaveChanges();
                return Json(new { success = true, message = "Venta actualizada correctamente" });
                //return RedirectToAction(nameof(Editsales), new { detalleId = existingSale.DetalleFactura_Id });

            }
            return Json(new { success = false, message = "Los datos ingresados son erroneos" });
            //return View(obj);
        }

        [HttpGet]
        public IActionResult GetSaleDetails(int? id)
        {
            var saleDetail = _context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                .Include(u => u.Users)
                .Where(i => i.Factura_Id == id && i.Status != false)
                .Select(p => new SaleDetailsDto
                {
                    DetalleFactura_Id = p.DetalleFactura_Id,
                    Factura_Id = p.Factura_Id,
                    Image = p.Products!.Image,
                    Producto_Descripcion = p.Products!.Descripcion,
                    Cliente_Id = p.Invoices!.Cliente_Id,
                    Cliente_nombre = p.Invoices!.Clients!.Nombre,
                    Cliente_apellido = p.Invoices.Clients!.Apellido,
                    Fecha = p.Invoices!.Fecha,
                    Moneda = p.Invoices.Currencies!.Tipo,
                    Cantidad = p.Cantidad,
                    Precio = p.Precio,
                    Total = p.Total_ventas,
                })
                .ToList();

            return Json(saleDetail);
        }

        public JsonResult SaleslistJson()
        {
            var saleDetail = _context.Invoice_details
                .Where(p => p.Status != false)
                .Select(p => new SaleDetailsDto
                {
                    DetalleFactura_Id = p.DetalleFactura_Id,
                    Factura_Id = p.Factura_Id,
                    Producto_Descripcion = p.Products!.Descripcion,
                    Cliente_nombre = p.Invoices!.Clients!.Nombre,
                    Cliente_apellido = p.Invoices.Clients!.Apellido,
                    Usuario_nombre = p.Users!.Nombre,
                    Fecha = p.Invoices!.Fecha,
                    Moneda = p.Invoices.Currencies!.Tipo,
                    Cantidad = p.Cantidad,
                    Precio = p.Precio,
                    Total = p.Total_ventas,
                })
                .ToList();

            return Json(saleDetail);
        }

        [HttpGet]
        public IActionResult GetProductPrice(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Producto_Id == productId);
            if (product != null)
            {
                return Json(product.Precio);
            }
            return Json(null);
        }

        [HttpGet]
        public IActionResult GetProductsByCategory(int id)
        {
            var products = _context.Products
                .Include(p => p.Categories)
                .Include(p => p.Brands)
                .Where(p => p.Categoria_Id == id && p.Status != false)
                .Select(p => new {
                    p.Producto_Id,
                    p.Descripcion,
                    p.Precio,
                    p.Image,
                    p.Cantidad,
                    CategoryName = p.Categories != null ? p.Categories.Nombre : "",
                    BrandName = p.Brands != null ? p.Brands.Nombre : ""
                })
                .Where(p => p.Cantidad > 0)
                .ToList();

            return Json(products);
        }

        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = _context.Clients
                .Where(c => c.Status != false)
                .ToList();
            return Json(clients);
        }

        [HttpGet]
        public IActionResult GetCurenncies()
        {
            var currencies = _context.Currencies.ToList();
            return Json(currencies);
       }

        [HttpPost]
        public async Task<IActionResult> AddClient(string nombre, string apellido)
        {
            if (ModelState.IsValid)
            {
                // Crear un nuevo cliente
                var cliente = new Client
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Status = true
                };
                _context.Clients.Add(cliente);
                await _context.SaveChangesAsync();

                // Retornar el ID del cliente recién creado
                return Json(new { success = true, clienteId = cliente.Cliente_Id });
            }

            return Json(new { success = false });
        }

        public IActionResult Newsales()
        {
            var model = new NewsalesViewModel
            {
                Categories = _context.Categories
                    .Where(c => c.Status)
                    .ToList(),
                Invoice_details = _context.Invoice_details
                    .Include(i => i.Invoices)
                        .ThenInclude(inv => inv!.Clients)
                    .Include(i => i.Invoices)
                        .ThenInclude(inv => inv!.Currencies)
                    .Include(p => p.Products)
                    .ToList(),
                Products = _context.Products.ToList()
            };
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            ViewData["userId"] = userId;

            return View(model);
        }

        [HttpPost]
        public IActionResult Newsales([FromBody] NewsalesViewModel newSales)
        {
            if (newSales == null)
            {
                //return BadRequest("Datos de compra inválidos");
                return Json(new { success = false, message = "Datos de la venta inválidos"});
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newInvoice = new Invoice
                    {
                        Fecha = DateTime.Now,
                        Cliente_Id = newSales.Invoices.First().Cliente_Id,
                        Monto_final = newSales.Invoices.First().Monto_final,
                        Moneda_Id = newSales.Invoices.First().Moneda_Id,
                    };
                    _context.Invoices.Add(newInvoice);
                    _context.SaveChanges();

                    foreach (var detail in newSales.Invoice_details)
                    {
                        if(detail.Cantidad == 0)
                        {
                            return Json(new { success = false, message = "La cantidad no puede ser 0" });
                        }
                        var newDetalleFactura = new Invoice_Detail
                        {
                            Factura_Id = newInvoice.Factura_Id,
                            Producto_Id = detail.Producto_Id,
                            Cantidad = detail.Cantidad,
                            Precio = detail.Precio,
                            Total_ventas = detail.Total_ventas,
                            Usuario_Id = newSales.Users.First().Usuario_Id,
                        };
                        _context.Invoice_details.Add(newDetalleFactura);
                        _productService.UpdateAmountProduct(detail.Producto_Id, detail.Cantidad, "restar");
                    }
                    _context.SaveChanges();
                    transaction.Commit();
                    return Json(new { success = true, message = "¡Venta realizada con éxito!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error al procesar la venta" + ex.Message);
                    return Json(new { success = false, message = "Error al procesar la Venta." });
                }
            }
            //return Ok();
        }

        [HttpGet]
        public IActionResult GetPay(int numeroFactura)
        {
            var numeroFacturaParam = new SqlParameter("@NumeroFactura", numeroFactura);

            var detallesPago = _context.detail_ClientViewModels
            .FromSqlRaw("EXEC ObtenerPagosPorFactura @NumeroFactura", numeroFacturaParam)
            .ToList();

            // Pasar los detalles del pago a la vista
            return Json(detallesPago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Desactivar el detalle de factura
            var invoiceDetail = _context.Invoice_details.FirstOrDefault(p => p.DetalleFactura_Id == id);
            if (invoiceDetail != null)
            {
                invoiceDetail.Status = false;
                _productService.UpdateAmountProduct(invoiceDetail.Factura_Id, (int)invoiceDetail.Total_ventas, "restar_factura");
                _context.SaveChanges();
                return Json(new { success = true, message = "Venta anulada correctamente" });
            }
            return Json(new { success = false, message = "No se pudo eliminar la venta" });
        }
    }



}
