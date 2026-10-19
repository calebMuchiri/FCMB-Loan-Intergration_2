using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class BaseRetailAccountOpeningResponse
    {
        public ResponseData data { get; set; }
        public string description { get; set; }
        public string customerId { get; set; }
        public string code { get; set; }
    }

    public class ResponseData
    {
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string cifId { get; set; }
        public bool pnd { get; set; }
        public bool isMandateImageAdded { get; set; }
        public bool isBVNLinked { get; set; }
    }

}
