using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardPinSet
    {
        public class CardPinSetRequestData
        {
            public string CardNumber { get; set; }
            public string CardId { get; set; }
            public string PinBlock { get; set; }
        }
        public class CardPinSetResponse
        {

            public PinsetResponseHeader responseHeader { get; set; }

            public PinsetResponseBody responseBody { get; set; }

        }
        public class PinsetResponseHeader
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public string StatusDesc { get; set; }
            public string RequestId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class PinsetResponseBody
        {
        }
    }
}
