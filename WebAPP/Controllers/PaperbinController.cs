using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models.Data;

namespace WebAPP.Controllers
{
    [Authorize]
    public class PaperbinController : Controller
    {
        private readonly ILogger<PaperbinController> _logger;
        private readonly DBContext _context;

        public PaperbinController(ILogger<PaperbinController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Productpaper()
        {
            return View(_context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categories)
                .Include(p => p.Suppliers)
                .Where(p => p.Status == false)
                .ToList());
        }

        public IActionResult Salepaper()
        {
            return View(_context.Invoice_details
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Clients)
                .Include(i => i.Invoices)
                    .ThenInclude(inv => inv!.Currencies)
                .Include(p => p.Products)
                    .Include(u => u.Users)
                .Where(i => i.Status == false)
                .ToList());
        }
    }
}
