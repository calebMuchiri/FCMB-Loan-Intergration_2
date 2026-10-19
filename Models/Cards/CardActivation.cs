using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardActivation
    {
        public class CardActivationRequestData
        {
            public string Cardnumber { get; set; }
            public string Phonenumber { get; set; }
            public DateTime ExpiryDate { get; set; }
        }

        public class CardActivationResponseData
        {
            public CardActivationResponseHeader crdbResponseHeader { get; set; }
            public CardActivationResponseBody crdbResponseBody { get; set; }
        }
        public class CardActivationResponseHeader
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public string StatusDesc { get; set; }
            public string RequestId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class CardActivationResponseBody
        {
            public string ErrorCode { get; set; }
            public string InternalErrorCode { get; set; }
            public string ErrorMessage { get; set; }

        }
    }
}
