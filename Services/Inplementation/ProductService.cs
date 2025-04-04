using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models.Data;
using project_DBA_VISO.Services.Contract;
using System;

namespace project_DBA_VISO.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly DBContext _context;

        public ProductService(DBContext context)
        {
            _context = context;
        }
        public void UpdateAmountProduct(int product_id, int amount, string action)
        {
            var id = new SqlParameter("@id", product_id);
            var cantidad = new SqlParameter("@cantidad", amount);
            var accion = new SqlParameter("@accion", action);
            _context.Database.ExecuteSqlRaw("EXEC ActualizarCantidad @id, @cantidad, @accion", id, cantidad, accion);
        }
    }
}
