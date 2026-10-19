using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace WebApi.Jwt.Models
{
    public class LoanLimitRequest
    {
        public string RepaymentAccountID { get; set; }     
        public string MobileNumber { get; set;}     
    }
}