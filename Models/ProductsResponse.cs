using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    
    public class ProductsResponse
    {
        public string ProductID { get; set; }
        public string Description { get; set; }
        public string ProductTypeName { get; set; }
        public string CurrencyID { get; set; }
        public decimal Interest { get; set; }
    }
}