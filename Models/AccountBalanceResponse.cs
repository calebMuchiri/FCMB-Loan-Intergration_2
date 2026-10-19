using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class AccountBalanceResponse
    {
        public string ClearBalance { get; set; }
        public string AvailableBalance { get; set; }
    }
}