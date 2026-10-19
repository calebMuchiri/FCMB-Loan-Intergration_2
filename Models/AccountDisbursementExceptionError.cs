using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountDisbursementExceptionError
    {
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public int ColumnID { get; set; }
        public string ErrorDescription { get; set; } = string.Empty;
        public string OperatorID { get; set; } = "SYS";

    }
}