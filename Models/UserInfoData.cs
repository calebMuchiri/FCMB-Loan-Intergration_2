using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    /// <summary>
    /// Token parameters 
    /// </summary>
    public class UserInfoDataModel
    {
        public ICollection<UserInfoData> data { get; set; }
    }
    public class UserInfoData: CheckUserData
    {
        //public string BranchID { get; set; }
        //public string System { get; set; }
        //public string BankID { get; set; }
        //private string Password { get; set; }
        //public string MachineIP { get; set; }
        //public string MacAddress { get; set; }

        /// <summary>
        /// This is the consumer key provided to you by the vendor
        /// </summary>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// This is the Consumer Secret provided to you by the vendor
        /// </summary>
        public string ConsumerSecret { get; set; }
    }

    public class CheckUserData
    {
        private string ConsumerKey { get; set; }
        private string ConsumerSecret { get; set; }
    }
}