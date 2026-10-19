using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardStatusChange
    {
        public class RequestData
        {
            public string CardNumber { get; set; }
            public string CardStatusCode { get; set; }
            public string Reason { get; set; }

        }
        public class ResponseData
        {
            public ResponseHeader ResponseHeader { get; set; }
            public ResponseBody ResponseBody { get; set; }

        }
        public class ResponseHeader
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public string StatusDesc { get; set; }
            public string RequestId { get; set; }


        }
        public class ResponseBody
        {
        }

    }
}
