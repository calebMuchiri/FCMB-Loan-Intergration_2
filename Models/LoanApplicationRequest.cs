using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace WebApi.Jwt.Models
{
    public class LoanApplicationRequest
    {
        public string OurBranchID { get; set; }
        public string RequestID { get; set; }
        public string MobileNumber { get; set; }
        public string RepaymentAccountID { get; set; }
        public string LoanAmount { get; set; }
        public string LoanTerm { get; set; }
        public string LoanProductID { get; set; } 
        public string OperatorID { get; set; }
        public string LoanType { get; set; }
		public string ClientID { get; set; }
		public string GroupID { get; set; }

		//public string WFAdvTypeID { get; set; }
		//  public string IsExistingClient { get; set; }r

		//  public string ClientID { get; set; }
		//  public string ProductID { get; set; }
		//  public string PurposeCodeID { get; set; }
		//  public string CreditOfficerID { get; set; }
		//public string LoanPeriodID { get; set; }
		//public string DisbursementDate { get; set; }
		//public string BusinessLineID { get; set; }
		//public string AccountClassID { get; set; }
		//   public string FileNumber { get; set; }
		//public string InterestRate { get; set; }
		//public string CreatedBy { get; set; }





		//public string LoanAmount { get; set; }
		//public string LoanTerm { get; set; }
		//public string RepaymentAccountID { get; set; }

		//@WFAdvTypeID nVarChar(6),  
		//@IsExistingClient Bit,
		//@ClientID    ClientID,  
		//@ProductID ProductID,
		//@RepaymentAccountID  AccountID,  
		//@PurposeCodeID UserSubID,
		//@CreditOfficerID  OperatorID,														
		//@LoanAmount Amount,
		//@LoanTerm    SmallInt,  
		//@LoanPeriodID SystemSubID,
		//@DisbursementDate  SmallDateTime,  
		//@BusinessLineID UserSubID,
		//@AccountClassID   nVarChar(10),  
		//@FileNumber nVarChar(50),  
		//@InterestRate Rate,
		//@CreatedBy    OperatorID,


	}




}