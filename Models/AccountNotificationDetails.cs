using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountNotificationDetails
    {

        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public int RowID { get; set; } = 0;
        public string OperatorID { get; set; } = "SYS";







    }
}