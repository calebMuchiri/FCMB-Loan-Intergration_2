using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace WebApi.Jwt.Models
{
    public class LoanBalanceRequest
    {

        public string OurBranchID { get; set; } 
        public string MobileNumber { get; set;}
        public string RepaymentAccountID { get; set;}
        public string LoanProductID { get; set; } = string.Empty;
        public string OperatorID { get; set; }
        public string UniqueRefID { get; set; }

        
    }
}