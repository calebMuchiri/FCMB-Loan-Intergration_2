using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class DisbursementTransferResponse
    {

        public string Code { get; set; }
        public string Description { get; set; }

        public DisbResponseData data { get; set; }

    }

    public class DisbResponseData
    {

    }
}
