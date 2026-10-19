using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountDisbursementRetries
    {
        
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public int ColumnID { get; set; }
        public string UniqueRefID { get; set; } = string.Empty;
        public int Retries { get; set; } = 0;
        public string OperatorID { get; set; } = "SYS";






    }
}