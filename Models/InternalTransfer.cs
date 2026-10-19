using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class InternalTransferResponse
    {
        public string ClearBalance { get; set; }
        public string AvailableBalance { get; set; }
    }
    public class MoneyTransferResponse
    {
        //public string ClearBalance { get; set; }
        //public string AvailableBalance { get; set; }
        public string ControlNumber { get; set; }
    }
}