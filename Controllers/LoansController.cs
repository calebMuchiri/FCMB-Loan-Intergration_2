using BRBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniApi.CoreLibraries;
using OmniApi.Models;
using OmniApi.Models.FCMBLoan;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.Jwt.Filters;
using WebApi.Jwt.Models;

namespace CoreOmniApi.Controllers
{
    //[JwtAuthentication]
    public class LoansController : ApiController
    {


        /// <summary>
        /// This API will Disburse Loan in Bulk to Finnacle Accounts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(200, "OK", typeof(LoanTransferSummaryResponse))]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        [SwaggerResponse(500, "Internal Server Error")]
        public async Task<IHttpActionResult> LoanDisbursementTransfer()
        {
            var usrInfo = CoreOmniApiBase.GetDbParameter();

            var summary = new LoanTransferSummaryResponse
            {
                OverallStatus = "00", // optimistic default
                Results = new List<LoanTransferResult>() // init to avoid NRE
            };

            try
            {
                var disbursementTransferRequests =
                    await OmniCore.LoanDisbursementTransferAsync(usrInfo)
                    ?? new List<DisbursementTransferRequest>();

                foreach (var disbursementRequest in disbursementTransferRequests)
                {
                    var result = new LoanTransferResult
                    {
                        CustomerReference = disbursementRequest.CustomerReference
                    };

                    AccountStatusResponse accountStatusResponse = null; // so catch can access

                    try
                    {
                        // 1) Check credit account status
                        var account = new AccountStatus
                        {
                            AccountID = disbursementRequest.CreditAccountNo
                        };

                        accountStatusResponse = await OmniCore.CheckAccountStatus(account);

                        if (accountStatusResponse == null
                            || accountStatusResponse.Code != "00"
                            || !string.Equals(accountStatusResponse.data != null ? accountStatusResponse.data.AccountStatus : null, "Active", StringComparison.OrdinalIgnoreCase))
                        {
                            result.StatusCode = "99";
                            result.Message = string.Format("Credit account {0} is not active or returned invalid status.", disbursementRequest.CreditAccountNo);
                            summary.OverallStatus = "99";
                            summary.Results.Add(result);
                            continue;
                        }

                        // 2) Process the transfer
                        var response = await OmniCore.ProcessTransferRequestsAsync(disbursementRequest);

                        if (response != null)
                        {
                            result.StatusCode = response.Code;
                            result.Message = response.Description;

                            OmniCore.UpdateTransferLoan(usrInfo, disbursementRequest.CustomerReference, response);

                            if (!string.Equals(response.Code, "00", StringComparison.OrdinalIgnoreCase))
                                summary.OverallStatus = "99";
                        }
                        else
                        {
                            Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(
                                usrInfo, "LoanDisbursementTransfer", "RESPONSE", JsonConvert.SerializeObject(response), ""));
                            Task.Run(() => CoreOmniApiBase.LogEvent(
                                string.Format("METHOD: LoanDisbursementTransfer \n RESPONSE:{0}\n", JsonConvert.SerializeObject(response))));

                            result.StatusCode = "99";
                            result.Message = "No response from FCMB.";
                            summary.OverallStatus = "99";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.StatusCode = "99";
                        result.Message = string.Format("Exception: {0}", ex.Message);
                        summary.OverallStatus = "99";

                        Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(
                            usrInfo, "LoanDisbursementTransfer", "REQUEST", JsonConvert.SerializeObject(accountStatusResponse), ""));
                        Task.Run(() => CoreOmniApiBase.LogEvent(
                            string.Format("METHOD: LoanDisbursementTransfer \n REQUEST:{0}\n", JsonConvert.SerializeObject(accountStatusResponse))));
                    }

                    summary.Results.Add(result);
                }

                summary.OverallMessage = summary.OverallStatus == "00"
                    ? "All loan disbursements completed successfully."
                    : "Some loan disbursements failed or had inactive accounts.";

                return Ok(summary); // Web API 2 Ok(...)
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent(
                    string.Format("METHOD: LoanDisbursementTransfer \n Exception:{0}\n", JsonConvert.SerializeObject(ex))));

                return Content(HttpStatusCode.InternalServerError, new LoanTransferSummaryResponse
                {
                    OverallStatus = "99",
                    OverallMessage = "Critical error occurred while processing disbursements.",
                    Results = new List<LoanTransferResult>()
                });
            }
        }




        /// <summary>
        /// Initiates a Loan Request to Customer and Money is disbursed to the Repayment Account
        /// </summary>
        /// <param name="loanApplicationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult LoanApplicationRequest(LoanApplicationRequest loanApplicationRequest)
        {
            bool isValid = false;
            //string strSessionID = string.Empty;           
            string strResponse = string.Empty;
            string strResponseMessages = string.Empty;
            string strResponseStatus = string.Empty;
            string strLoanApplicationResponse = null;
            string OurBranchID = string.Empty;
            string RequestID = string.Empty;
            string MobileNumber = string.Empty;
            string RepaymentAccountID = string.Empty;
            double LoanAmount = 0;
            string LoanTerm = string.Empty;
            string LoanProductID = string.Empty;
            string OperatorID = string.Empty;           
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsLoanApp = new BRDataSet();
            
            LoanApplicationResponse loanApplicationResponse = new LoanApplicationResponse();
            DataSet resp = null;
            if (loanApplicationRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }


            if (!ModelState.IsValid) return BadRequest(ModelState);

  
                DataSet MyApplication = new DataSet();
                //strSessionID = myData.SessionID;

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                if (usrInfo == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (loanApplicationRequest == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                try
                {
                    isValid = false;
                    
                    double.TryParse(loanApplicationRequest.LoanAmount, out LoanAmount);
                    
                    ///TrxAmount = myData.TrxAmount;

                    if (LoanAmount <= 0)
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid Loan Amount");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    if (string.IsNullOrEmpty(loanApplicationRequest.RepaymentAccountID))
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    if (string.IsNullOrEmpty(loanApplicationRequest.MobileNumber))
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300056", "add Mobile Number");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    if (string.IsNullOrEmpty(loanApplicationRequest.LoanType))
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300056", "add Loan Type");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                usrInfo.strUser = CoreOmniApiBase.gOperatorID;
                
                    loanApplicationRequest.OperatorID = usrInfo.strUser;
                    DataTable dtLoanApplication = CoreOmniApiBase.dtFromObject(loanApplicationRequest, "dt_LoanApplication");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Loan Application", "REQUEST", JsonConvert.SerializeObject(loanApplicationRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: LoanApplicationRequest \n REQUEST:{JsonConvert.SerializeObject(loanApplicationRequest).ToString()}\n"));
                dsLoanApp.Tables.Add(dtLoanApplication);

                string mydata = string.Empty;
                mydata = JsonConvert.SerializeObject(loanApplicationRequest).ToString();
                strResponse = OmniCore.LoanApplicationDetails(usrInfo, mydata, dsLoanApp);
               // strResponse = OmniCore.LoanApplicationDetails(usrInfo, loanApplicationRequest.RepaymentAccountID, loanApplicationRequest.MobileNumber, loanApplicationRequest.LoanType, dsLoanApp);


                    if (string.IsNullOrEmpty(strResponse))
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                JArray jsonArray = JArray.Parse(strResponse);
                    foreach (JObject jsonObject in jsonArray)
                    {
                        strResponseMessages = jsonObject["Data"]?.ToString();
                        strLoanApplicationResponse = jsonObject["Response"]?.ToString();
                        strResponseCode = jsonObject["Status"]?.ToString();
                  

                    //if ( strResponseCode == "99")
                    //{
                    //    strResponseMessages = strLoanApplicationResponse[3];
                    //}
                    //else if (strResponseCode == "000")
                    //{
                    //    strResponseMessages = "loan application successfull";
                    //}
                    //else
                    //{
                    //    strResponseMessages = "failed";
                    //}
                }
                resp = CoreOmniApiBase.GenResponse(strResponse, "", "");
                //resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponse, strResponseMessages);


                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: LoanApplicationDetails \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: LoanApplicationDetails \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

        //[HttpPost]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        //[SwaggerResponse(500, "Internal Server Error")]
        //public IHttpActionResult LoanApplicationDisbursementRequest(LoanApplicationRequest loanApplicationRequest)
        //{
        //    bool isValid = false;
        //    //string strSessionID = string.Empty;           
        //    string strResponse = string.Empty;
        //    string[] strLoanApplicationResponse = null;
        //    string RequestID = string.Empty;
        //    string Mobile = string.Empty;
        //    string AccountID = string.Empty;
        //    double LoanAmount = 0;
        //    string LoanTerm = string.Empty;
        //    string ProductID = string.Empty;
        //    string CreatedBy = string.Empty;
        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    BRDataSet dsLoanApp = new BRDataSet();

        //    LoanApplicationResponse loanApplicationResponse = new LoanApplicationResponse();
        //    DataSet resp = null;
        //    if (loanApplicationRequest == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }


        //    if (!ModelState.IsValid) return BadRequest(ModelState);


        //    DataSet MyApplication = new DataSet();
        //    //strSessionID = myData.SessionID;

        //    UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

        //    if (usrInfo == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    if (loanApplicationRequest == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        isValid = false;



        //        double.TryParse(loanApplicationRequest.LoanAmount, out LoanAmount);

        //          ///TrxAmount = myData.TrxAmount;



        //        if (LoanAmount <= 0)
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid Loan Amount");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }


        //        if (string.IsNullOrEmpty(loanApplicationRequest.RepaymentAccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(loanApplicationRequest.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;

        //        DataTable dtLoanApplication = CoreOmniApiBase.dtFromObject(loanApplicationRequest, "dt_LoanApplicationdisbursement");

        //        // Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanApplicationDisbursement", "REQUEST", JsonConvert.SerializeObject(loanApplicationRequest).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: LoanApplicationDisbursement \n REQUEST:{JsonConvert.SerializeObject(loanApplicationRequest).ToString()}\n"));

        //        dsLoanApp.Tables.Add(dtLoanApplication);


        //        strResponse = OmniCore.LoanApplicationDisbursementRequest(usrInfo, loanApplicationRequest.RepaymentAccountID, loanApplicationRequest.MobileNumber, dsLoanApp);

        //        //strResponse = string.Format("00|Loan Application Successfully");

        //        if (string.IsNullOrEmpty(strResponse))
        //        {

        //            resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");

        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        strLoanApplicationResponse = strResponse.Split('|');

        //        if (strLoanApplicationResponse.Length > 0)
        //        {
        //            strResponseCode = strLoanApplicationResponse[0] != null ? strLoanApplicationResponse[0] : null;
        //            strResponseMessage = strLoanApplicationResponse.Length > 1 ? strLoanApplicationResponse[1] : strResponse;

        //        }
        //        else
        //        {
        //            strResponseCode = "99";
        //            strResponseMessage = strResponse;
        //        }

        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);

        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }

        //}
        /// <summary>
        /// Initiates a Loan Balance
        /// </summary>
        /// <param name="loanBalanceRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult GetLoanBalance(LoanBalanceRequest loanBalanceRequest)
        {
            bool isValid = false;
            //string strSessionID = string.Empty;           
            string response = string.Empty;
            string wfStageID = string.Empty;
            DataSet dsLoanApp = new DataSet();
            string tableName = string.Empty;
            string WFStageID = string.Empty;
            LoanBalanceRequest responseObject = new LoanBalanceRequest();
            bool ValidAccount = false;
            DataSet resp = null;
            BRDataSet ds_LoanBalance = new BRDataSet();
            BRDataSet ds_LoanBal = new BRDataSet();
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            IList<LoanBalanceResponse> lstLoanBalanceResponse = null;
            if (loanBalanceRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }


            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                DataSet MyApplication = new DataSet();
                //strSessionID = myData.SessionID;

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                if (usrInfo == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetLoanBalance", "REQUEST", JsonConvert.SerializeObject(loanBalanceRequest).ToString(), ""));

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanBalance \n REQUEST:{JsonConvert.SerializeObject(loanBalanceRequest).ToString()}\n"));

                DataTable dtLoanBalance = CoreOmniApiBase.dtFromObject(loanBalanceRequest, "dtLoanBalance");
                ds_LoanBal.Tables.Add(dtLoanBalance);

                ValidAccount = OmniCore.LoanBalanceDetails(usrInfo, loanBalanceRequest.RepaymentAccountID, loanBalanceRequest.MobileNumber, loanBalanceRequest.LoanProductID,
                     ds_LoanBal, out ds_LoanBalance);

                if (ValidAccount == true)
                {

            
                        strResponseCode = ds_LoanBalance.Tables[0].Rows[0][0].ToString();
                        //strResponseMessage = ds_LoanBalance.Tables[0].Rows[0][1].ToString();

                    if (ds_LoanBalance.Tables.Count > 1 && ds_LoanBalance.Tables[1].Rows.Count >0)
                    {
                        lstLoanBalanceResponse = ds_LoanBalance.Tables[1].ToListof<LoanBalanceResponse>();

                        resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(lstLoanBalanceResponse), strResponseMessage);
                    }
                    else {
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                    }
          
                }
                else
                {
                     resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");
                }
               Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanBalance \n RESPONSE:{JsonConvert.SerializeObject(resp).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanBalance \n RESPONSE:{JsonConvert.SerializeObject(resp).ToString()}\n"));           
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));   
            }
        }

        /// <summary>
        /// Initiates a Loan Limit
        /// </summary>
        /// <param name="loanLimitRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult GetLoanLimit(LoanLimitRequest loanLimitRequest)
        {
            bool isValid = false;
            //string strSessionID = string.Empty;           
            string response = string.Empty;
            string wfStageID = string.Empty;
            DataSet dsLoanApp = new DataSet();
            string tableName = string.Empty;
            string WFStageID = string.Empty;
            LoanLimitResponse responseObject = new LoanLimitResponse();
            bool ValidAccount = false;
            DataSet resp = null;
            BRDataSet ds_LoanLimit = new BRDataSet();
            BRDataSet ds_Limit = new BRDataSet();
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            string strLimit = string.Empty;
            IList<LoanLimitResponse> lstLoanLimitResponse = null;
            if (loanLimitRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }


            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                DataSet MyApplication = new DataSet();
                //strSessionID = myData.SessionID;

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                if (usrInfo == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetLoanLimit", "REQUEST", JsonConvert.SerializeObject(loanLimitRequest).ToString(), ""));

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanLimit \n REQUEST:{JsonConvert.SerializeObject(loanLimitRequest).ToString()}\n"));

                DataTable dtLimit = CoreOmniApiBase.dtFromObject(loanLimitRequest, "dt_Limit");
                ds_Limit.Tables.Add(dtLimit);

                ValidAccount = OmniCore.LoanLimitDetails(usrInfo, loanLimitRequest.RepaymentAccountID, loanLimitRequest.MobileNumber, ds_Limit, out ds_LoanLimit);

                if (ValidAccount == true)
                {
                    strResponseCode = ds_LoanLimit.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = "";
                    strLimit = "";
                    //strResponseMessage = ds_LoanLimit.Tables[0].Rows[0][0].ToString();
                    //strLimit = ds_LoanLimit.Tables[0].Rows[0][0].ToString();

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, strLimit, strResponseMessage);

                    //if (ds_LoanLimit.Tables.Count > 1 && ds_LoanLimit.Tables[1].Rows.Count > 0)
                    //{
                    //    // strLimit = ds_LoanLimit.Tables[1].ToListof<LoanLimitResponse>();
                    //   lstLoanLimitResponse = ds_LoanLimit.Tables[1].ToListof<LoanLimitResponse>();
                    ///
                    //} else {
                    //    resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                    //}


                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);



                }
                else
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exists");
                     Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanLimit \n RESPONSE:{JsonConvert.SerializeObject(resp).ToString()}\n"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



                //if (responseObject.ResponseCode == "00")
                //{


                //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, responseObject, responseObject);
                //}

                //else
                //{
                //    resp = CoreOmniApiBase.GenResponse("99", responseObject.ResponseMessage, "Response Error");
                //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                //}

            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetLoanLimit \n RESPONSE:{JsonConvert.SerializeObject(resp).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }
        }

        ///// <summary>
        ///// Initiates a Loan Payment
        ///// </summary>
        ///// <param name="LoanPayment"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        //[SwaggerResponse(500, "Internal Server Error")]

        private IHttpActionResult LoanPaymentDetails(LoanPayment loanPayment)
        {

            bool isValid = false;
            //string strSessionID = string.Empty;           
            string response = string.Empty;
            string wfStageID = string.Empty;
            DataSet dsLoanApp = new DataSet();
           string tableName = string.Empty;
           string WFStageID = string.Empty;
           LoanPaymentResponse responseObject = new LoanPaymentResponse();
            DataSet resp = null;
            if (loanPayment == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }


          if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
               DataSet MyApplication = new DataSet();
                //strSessionID = myData.SessionID;

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                if (usrInfo == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }


               // responseObject = CoreOmniApiBase.LoanPaymentDetails(usrInfo, loanPayment);
               if (responseObject.ResponseCode == "00")
                {


                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, responseObject, responseObject);
                }

                else
               {
                    resp = CoreOmniApiBase.GenResponse("99", responseObject.ResponseMessage, "Response Error");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



            }
            catch (Exception ex)
            {
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }




        /// <summary>
        /// Use this API to Add new  collateral in BRNET.  
        /// </summary>
            /// <remarks>
           /// Below are codes for diffrent collateraltypeid ==>
          ///  ==>      07 FARM EQUIPMENT /  002 MOTORVEHICLES / 001 HOUSEHOLD  / 006 LIVESTOCK / 008 SHARES /003 LAND PROPERTIES / 004 BUSINESS ASSET
         /// </remarks>
        /// <param name="myData"></param>
        /// <returns></returns>

        [HttpPost]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Session. Cannot Continue")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult AddCollateral(CollateralRequest collateralrequest)
        {
            bool isValid = false;   
            string strResponse = string.Empty;
            string strResponseMessages = string.Empty;
            string strResponseStatus = string.Empty;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            string[] strAddCollateralResponse = null;

            string CollateralID = string.Empty;
            string Description = string.Empty;
            string OurBranchID = string.Empty;
            string CollateralTypeID = string.Empty;
            string OwnerClientID = string.Empty;
            string LodgedDate = string.Empty;
            string IsInsured = string.Empty;
            string NatureOfChargeID = string.Empty;
            string Remarks = string.Empty;

            string amount = string.Empty;
            string cost = string.Empty;
            string marketvalue = string.Empty;
            string forcedsalevalue = string.Empty;

            BRDataSet dsCollateralApp = new BRDataSet();

            //LoanApplicationResponse loanApplicationResponse = new LoanApplicationResponse();
            CollateralResponse collateralresponse = new CollateralResponse();
            DataSet resp = null;
            if (collateralrequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }


            if (!ModelState.IsValid) return BadRequest(ModelState);


            DataSet MyApplication = new DataSet();
            //strSessionID = myData.SessionID;

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (collateralrequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;

                double amt = double.Parse(collateralrequest.amount); 
                double cst = double.Parse(collateralrequest.cost); 
                double marvalue = double.Parse(collateralrequest.marketvalue); 
                double salevalue = double.Parse(collateralrequest.forcedsalevalue); 


                if (amt <= 0 || string.IsNullOrEmpty(collateralrequest.amount))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid  Amount");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (cst <= 0 || string.IsNullOrEmpty(collateralrequest.cost))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid  cost ");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (marvalue <= 0 || string.IsNullOrEmpty(collateralrequest.marketvalue))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid  Marktet value ");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (salevalue <= 0 || string.IsNullOrEmpty(collateralrequest.forcedsalevalue))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid  sale value ");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(collateralrequest.Description))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "add collateral description ");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(collateralrequest.OurBranchID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "add branch");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(collateralrequest.CollateralTypeID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "add collateral Type");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(collateralrequest.IsInsured))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "insurance cannot be null");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(collateralrequest.NatureOfChargeID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "charge id");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



                usrInfo.strUser = CoreOmniApiBase.gOperatorID;

                DataTable dtCollateralApplication = CoreOmniApiBase.dtFromObject(collateralrequest, "dt_CollateralApplication");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Collateral Application", "REQUEST", JsonConvert.SerializeObject(collateralrequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: Collateral Application \n REQUEST:{JsonConvert.SerializeObject(collateralrequest).ToString()}\n"));
                dsCollateralApp.Tables.Add(dtCollateralApplication);

                string mydata = string.Empty;
                mydata = JsonConvert.SerializeObject(collateralrequest).ToString();

                var MyData = JObject.Parse(mydata);

                // Add more values
                MyData["CreatedOn"] = usrInfo.dtLastAccessTime ;
                MyData["CreatedBy"] = usrInfo.strUser;
                MyData["NewRecord"] = 1;

               string mydata1 = JsonConvert.SerializeObject(MyData).ToString();

                strResponse = OmniCore.AddCollateralDetails(usrInfo, mydata1, dsCollateralApp);
             
                if (string.IsNullOrEmpty(strResponse))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }


                JArray jsonArray = JArray.Parse(strResponse);
                foreach (JObject jsonObject in jsonArray)
                {
                    strResponseMessages = jsonObject["CollateralID"].ToString();
                    strAddCollateralResponse = strResponse.Split('|');
                    strResponseCode = strAddCollateralResponse[1];

                    if (strResponseCode == "05")
                    {
                        //strResponseMessages = jsonObject["ErrorMessage"].ToString();
                        strResponseMessages = "failed";
                    }
                    else if (strResponseCode == "000")
                    {
                        strResponseMessages = "Success";
                    }
                    else
                    {
                        strResponseMessages = "failed";
                    }
                }

                resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponse, strResponseMessages);

                ////JObject jsonObject = JObject.Parse(strResponse);

                //JArray jsonarray = JArray.Parse(strResponse);

                //strResponseCode = jsonarray["Status"].ToString();
                //if (strResponseCode == "05")
                //{
                //    strResponseMessages = jsonarray["ErrorMessage"].ToString();
                //}
                //if (strResponseCode == "000")
                //{
                //    strResponseMessages = strResponse;
                //}

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CollateralRequest \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CollateralRequest \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

    }









}

