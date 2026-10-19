using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class LoanApplicationResponse
    {


        public decimal AppliedAmount { get; set; } = 0;
        public decimal InterestAmount { get; set; } = 0;

        public decimal ChargedAmount { get; set; } = 0;

        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        //public string LoanBalance { get; set; } = "0";
        //public string MaturityDate { get; set; } = DateTime.Now.ToString("dd MMM yyyy");
        //public string LoanBalance { get; set; } = "0";

    }
}