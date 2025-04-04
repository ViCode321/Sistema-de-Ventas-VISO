using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Models.Views;
using System.Diagnostics;
using System.Security.Claims;

namespace WebAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBContext _context;
            
        public HomeController(ILogger<HomeController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var products = _context.Products
                .OrderByDescending(p => p.Producto_Id)
                .Take(4)
                .Select(p => new ProductViewModel
                {
                    Producto_Id = p.Producto_Id,
                    Image = p.Image!,
                    Descripcion = p.Descripcion!,
                    Precio = p.Precio,
                    MarcaNombre = p.Brands!.Nombre!,
                    CategoriaNombre = p.Categories!.Nombre!
                })
                .ToList();

            var result = _context.TotalCounts
                .FromSqlRaw("EXEC ObtenerCantidadTotal")
                .AsEnumerable()
                .FirstOrDefault();

            var profillosses = _context.totalProfitLosses
                .FromSqlRaw("EXEC ObtenerGananciaPerdidaQuincenal")
                .AsEnumerable()
                .FirstOrDefault();

            var resultInvoices = _context.montoInvoices
                .FromSqlRaw("EXEC ObtenerTotalMontoFinalQuincenal")
                .AsEnumerable()
                .FirstOrDefault();

            var resultPurchases = _context.TotalMontos
                .FromSqlRaw("EXEC ObtenerTotalMontoFinalCompraQuincenal")
                .AsEnumerable()
                .FirstOrDefault();

            ViewBag.Compras = _context.purchaseViews
                .FromSqlRaw("EXEC ObtenerComprasUltimos7Dias")
                .AsEnumerable()
                .Select(result => new PurchaseViewModel
                {
                    Fecha = result.Fecha,
                    TotalCompras = result.TotalCompras ?? 0
                })
                .ToList();

            ViewBag.Ventas = _context.InvoiceViews
                .FromSqlRaw("EXEC ObtenerVentasUltimos7Dias")
                .AsEnumerable()
                .Select(result => new InvoiceViewModel
                {
                    Fecha = result.Fecha,
                    TotalVentas = result.TotalVentas ?? 0
                })
                .ToList();


            // Obtener el rol del usuario
            string rol = "";
            if (User.Identity!.IsAuthenticated)
            {
                rol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!;
            }

            // Obtener el nombre de usuario
            string username = User.Identity.Name!;

            ViewData["username"] = username;
            ViewData["rol"] = rol;
            //Console.WriteLine("Este es el rol: " + rol);

            ViewBag.TotalClients = result?.TotalClients ?? 0;
            ViewBag.TotalSuplliers = result?.TotalSuplliers ?? 0;
            ViewBag.TotalPurchase = result?.TotalPurchase ?? 0;
            ViewBag.TotalInvoices = result?.TotalInvoices ?? 0;

            ViewBag.Perdidas = profillosses?.Perdidas ?? 0;
            ViewBag.Ganancias = profillosses?.Ganancias ?? 0;

            // Envía el total de compras a la vista
            ViewBag.TotalMontoFinal = resultInvoices?.TotalMontoFinal ?? 0;
            ViewBag.TotalMontoCompra = resultPurchases?.TotalMontoFinalCompra ?? 0;

            var dashboardData = _context.DashboardDataViewModels
                .FromSqlRaw("EXEC ObtenerDatosDashboard")
                .AsEnumerable()
                .FirstOrDefault();

            ViewBag.TotalVentas = dashboardData?.TotalVentas ?? 0;
            ViewBag.ProductosVendidos = dashboardData?.ProductosVendidos ?? 0;
            ViewBag.ProductosStockBajo = dashboardData?.ProductosStockBajo ?? 0;
            ViewBag.ClientesAtendidos = dashboardData?.ClientesAtendidos ?? 0;

            ViewBag.TopProductos = _context.TopProductsViews
                .FromSqlRaw("EXEC ObtenerTop5ProductosVendidosHoy")
                .AsEnumerable()
                .Select(result => new TopProductsViewModel
                {
                    CantidadVendida = result.CantidadVendida ?? 0,
                    Producto = result.Producto ?? "Desconocido"
                })
                .ToList();

            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Obtener el código de estado HTTP
            var statusCode = HttpContext.Response.StatusCode;

            // Crear el ErrorViewModel con el código de estado HTTP
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };

            return View(errorViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Signin", "User");
        }

    }
}
