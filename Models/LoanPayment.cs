using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace WebApi.Jwt.Models
{
    public class LoanPayment
    {

        [Required(ErrorMessage = " {0} is Required Parameter")]
        public string OurBranchID { get; set; }

        [Required(ErrorMessage = " {0} is Required Parameter")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = " {0} is Required Parameter")]
        public string RepaymentAccountID { get; set; }

        [Required(ErrorMessage = "{0} is not Supplied")]
        public string Amount { get; set; }

        [Required(ErrorMessage = " {0} is Required Parameter")]
        public string LoanProductID { get; set; } = string.Empty;

        [Required(ErrorMessage = " {0} is Required Parameter")]
        public string UniqueReferenceID { get; set; }
    }
}