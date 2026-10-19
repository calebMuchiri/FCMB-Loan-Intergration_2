using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountDisbValidationRequest
    {
        
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public double Amount { get; set; }
        public string ApplicationID { get; set; }
        public int ColumnID { get; set; }
        public string OperatorID { get; set; } = "SYS";
        public string OurBranchID { get; set; } = null;



    }
}