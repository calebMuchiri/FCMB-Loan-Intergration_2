using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class LoanPaymentResponse
    {
        public string ResponseCode { get; set; }
        //public string MaturityDate { get; set; }
        //public string LoanBalance { get; set; }
         public string ResponseMessage { get; set; }
    }
}