using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_DBA_VISO.Services.Contract
{
    public interface IProductService
    {
        void UpdateAmountProduct(int product_id, int amount, string action);
    }
}
