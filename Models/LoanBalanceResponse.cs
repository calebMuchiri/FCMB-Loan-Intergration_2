using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class LoanBalanceResponse
    {
   
        public decimal LoanBalance { get; set; }
        public string DueDate { get; set; }
        public string ProductName { get; set; }
        public string ApplicationDate { get; set; }
        public int LoanTerm { get; set; }
        public string Frequency { get; set; }
        public decimal InterestRate { get; set; }

    }
}