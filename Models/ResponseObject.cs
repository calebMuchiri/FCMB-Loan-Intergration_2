using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class ResponseObject
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}