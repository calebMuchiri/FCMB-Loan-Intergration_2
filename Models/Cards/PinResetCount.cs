using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class PinResetCount
    {
        public string CardNumber { get; set; }
    }
    public class ResetCountResponse
    {
        public ResetCountResponseHeader responseHeader { get; set; }
        public ResetCountResponseBody responseBody { get; set; }
    }

    public class ResetCountResponseHeader
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string StatusDesc { get; set; }
        public string RequestId { get; set; }
    }
    public class ResetCountResponseBody
    {
    }
}
