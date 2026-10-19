using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class SendPinViaSms
    {
        public class SendSmsPinRequestData
        {
            public string CardNumber { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string Phone { get; set; }
        }
        public class SendPinSmsResponse
        {

            public SendPinSmsResponseHeader responseHeader { get; set; }

            public SendPinSmsResponseBody responseBody { get; set; }

        }
        public class SendPinSmsResponseHeader
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public string StatusDesc { get; set; }
            public string RequestId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class SendPinSmsResponseBody
        {
        }
    }
}
