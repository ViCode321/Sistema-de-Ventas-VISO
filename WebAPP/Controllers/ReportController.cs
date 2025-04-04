using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Models.Views;
using project_DBA_VISO.Services.Contract;

namespace WebAPP.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly DBContext _context;

        public ReportController(ILogger<ReportController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Inventoryreport()
        {
            var productsreport = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)
                .ToList();

            ViewBag.TopProductos = _context.TopProductsViews
                .FromSqlRaw("EXEC ObtenerTop5ProductosVendidosMensual")
                .AsEnumerable()
                .Select(result => new TopProductsViewModel
                {
                    CantidadVendida = result.CantidadVendida,
                    Producto = result.Producto
                })
                .ToList();
            return View(productsreport);
        }

        public IActionResult Salesreport()
        {
            var salesreport = _context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                .Where(i => i.Status != false)
                .ToList();
            
            ViewBag.VentasMensuales = _context.SalesReportViewModels
                .FromSqlRaw("EXEC ObtenerVentasMensuales")
                .AsEnumerable()
                .Select(result => new SalesReportViewModel
                {
                    Semana = result.Semana,
                    TotalVentas = result.TotalVentas
                })
                .ToList();

            return View(salesreport);
        }

        public IActionResult Purchasereport()
        {
            var purchasereport = _context.Purchase_Details
                .Include(p => p.Purchases)
                .Include(pd => pd.Products)
                .Include(u => u.Users)
                .Include(s => s.Suppliers)
                .ToList();

            ViewBag.ComprasMensuales = _context.PurchaseReportViewModels
                .FromSqlRaw("EXEC ObtenerComprasMensuales")
                .AsEnumerable()
                .Select(result => new PurchaseReportViewModel
                {
                    Semana = result.Semana,
                    TotalCompras = result.TotalCompras
                })
                .ToList();

            return View(purchasereport);
        }
    }
}
