using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardPinReset
    {
        public class Requestdata
        {
            public string Cardnumber { get; set; }
            public string CardId { get; set; }
            public string PinBlock { get; set; }
        }
        //Update card pin count to 0

        public class Responsedata
        {
            public ResponseHeader responseHeader { get; set; }
            public Responsebody Responsebody { get; set; }

        }
        public class ResponseHeader
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public string StatusDesc { get; set; }
            public string RequestId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Responsebody
        {
            public string ErrorCode { get; set; }
            public string InternalErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
