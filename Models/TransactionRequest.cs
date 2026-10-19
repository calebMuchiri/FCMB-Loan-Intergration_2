using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
   
    public class TransactionRequest
    {  
        public string OurBranchID { get; set; }
	    public string MobileNumber { get; set; }
        public string RepaymentAccountID { get; set;}
        public string LoanProductID { get; set; } = string.Empty;
        public double SettlementAmount { get; set; }
        public string OperatorID { get; set; } = string.Empty;
        public string UniqueID { get; set; }    
    }
    

    public class transactionCallback
    {
    


      
        public string UniqueID { get; set; }
        public string PTN { get; set; }
        public string OperatorID { get; set; } = string.Empty;
    }

    public class ReversalRequest
    {
        public string ChannelID { get; set; }

        public string ServiceID { get; set; }

        public string RequestID { get; set; }

        public string OperatorID { get; set; }

        public string TrxBatchID { get; set; }
    }

    public class Transactions
    {
    //    public string OurBranchID { get; set; }
    //    public string FromAccountID { get; set; }
    //    public string ToBranchID { get; set; }
    //    public string ToAccountID { get; set; }
    //    public string TrxAmount { get; set; }
    //    public string crTransactionDescription { get; set; }
    //    public string drTransactionDescription { get; set; }
    //    public string TrxDescriptionID { get; set; }
    //    public string UniqueID { get; set; }
    //    public string ChannelID { get; set; }

        public string ChannelID { get; set; }

        public string ServiceID { get; set; }

        public string RequestID { get; set; }

        public string OperatorID { get; set; }

        public string FromAccountID { get; set; }

        public string ToAccountID { get; set; }

        public string TrxAmount { get; set; }

    }

    public class MoneyTransfer
    {
        public string OurBranchID { get; set; }
        public string AccountID { get; set; }
        public string Recievername { get; set; }
        public string RecieverIDNumber { get; set; }
        public string TrxAmount { get; set; }
        public string RecieverMobile { get; set; }
        public string TrxDescriptionID { get; set; }
    }
}