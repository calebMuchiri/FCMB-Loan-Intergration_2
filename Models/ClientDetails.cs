using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
  
    public class ClientDetails
    {

        public string PhoneNumber { get; set; } = string.Empty;
        public string IDNumber { get; set; } = string.Empty;
        public string ClientID { get; set; }
        //public string AccountID { get; set; }  this will be on another method


    }
}