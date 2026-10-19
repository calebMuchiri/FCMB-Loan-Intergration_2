using BRBase;
using BREntities.StandingInstructions;
using OmniApi.CoreLibraries;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Jwt.Models;
using System.Resources;
using System.Reflection;
using WebApi.Jwt.Filters;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OmniApi.Controllers
{
   [JwtAuthentication] 
    public class TransactionsController : ApiController
    {

        /// <summary>
        /// Use this API to initiate a Loan Repayment
        /// </summary>
        /// <param name="myData"></param>
        ///  <param name = "LoanRepayment"></param>
        /// <returns></returns>
        [HttpPost]
        //[Route("v1/api/LoanRepayment")]

        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "The Origin and Destined Accounts are the Same")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "Invalid Account")]
        [SwaggerResponse(300018, "Invalid Amount")]
        [SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked")]
        [SwaggerResponse(300007, "Transaction not successful ")]
        private IHttpActionResult LoanRepayment(TransactionRequest myData)
        {

            //BRDataSet ds_InternalTransfer = new BRDataSet();
            string OurBranchID = string.Empty;
            string RepaymentAccountID = string.Empty;
            string MobileNumber = string.Empty;
            //double TrxAmount = 0;
            string LoanProductID = string.Empty;
            double SettlementAmount = 0;
            string OperatorID = string.Empty;
            string UniqueRefID = string.Empty;                         
            bool ValidAccount = false;
            BRDataSet ds_LoanRepayment = new BRDataSet() ;
            string strResponse = string.Empty;
            string[] strTransactionDetailsResponse = null;
            DataSet resp = null;

            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;



            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
           //     CoreOmniApiBase.LogEvent($"LoanRepayment --> Request: {JsonConvert.SerializeObject(myData)}");

                bool isValid = false;
                OurBranchID = myData.OurBranchID;
                MobileNumber = myData.MobileNumber;
                RepaymentAccountID = myData.RepaymentAccountID;
                LoanProductID = myData.LoanProductID;
                SettlementAmount = myData.SettlementAmount;
                OperatorID = myData.OperatorID;
                UniqueRefID = myData.UniqueID;

                ///double.TryParse(myData.TrxAmount, out TrxAmount);

                ///TrxAmount = myData.TrxAmount;


                if (SettlementAmount <=0)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid Amount");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(RepaymentAccountID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(MobileNumber))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                //if (string.IsNullOrEmpty(Trx_ParameterID))
                //{
                //    resp = CoreOmniApiBase.GenResponse("99", "300056", "Trx Parameter ID");
                //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                //}

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
                usrInfo.strUser = CoreOmniApiBase.gOperatorID;

                DataTable dtTransaction = CoreOmniApiBase.dtFromObject(myData, "dt_Transactions");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanRepayment", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: LoanRepayment \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                ds_LoanRepayment.Tables.Add(dtTransaction);


                strResponse = OmniCore.LoanRepayment(usrInfo, ds_LoanRepayment);
                

                if (string.IsNullOrEmpty(strResponse))
                {

                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



                strTransactionDetailsResponse = strResponse.Split('|');

                if (strTransactionDetailsResponse.Length > 0)
                {
                    strResponseCode = strTransactionDetailsResponse[0] != null ? strTransactionDetailsResponse[0] : null;
                    strResponseMessage = strTransactionDetailsResponse.Length > 1 ? strTransactionDetailsResponse[1] : strResponse;
                    //strResponse = string.Format("00|Loan Payment Successfully");

                }
                else
                {
                    strResponseCode = "99";
                    strResponseMessage = strResponse;
                }

                //resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");

                resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                
                //CoreOmniApiBase.LogEvent($"response Error: {JsonConvert.SerializeObject(resp)}");
                CoreOmniApiBase.LogEvent($"METHOD: LoanRepayment \n RESPONSE: { JsonConvert.SerializeObject(resp)}");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                /*
                if (ValidAccount == true)
                {
                    //return ds_InternalTransfer.GetXml().ToString().Replace("<BRDataSet>", "").Replace("</BRDataSet>", "").Trim();
                    

                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                            CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_InternalTransfer.Tables[0]),
                            CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_InternalTransfer.Tables[0]));

                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //        ds_InternalTransfer.GetXml().ToString().Replace("<BRDataSet>", "").Replace("</BRDataSet>", "").Trim(),
                    //        ds_InternalTransfer.GetXml().ToString().Replace("<BRDataSet>", "").Replace("</BRDataSet>", "").Trim());
                }
                else

                {
                    DataSet resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }
                    
                */

            }
            catch (Exception ex)
            {
               //CoreOmniApiBase.LogEvent($"response Error: {ex.Message.ToString()}");
                CoreOmniApiBase.LogEvent($"METHOD: LoanRepayment \n RESPONSE: {ex.Message.ToString()}");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }
            
        }


        /// <summary>
        /// Use this API to initiate an internal transfer transaction
        /// </summary>
        /// <param name="internalTransferRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300055, "Both the branches are not on the same working Date")]
        //[SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        //[SwaggerResponse(300056, "GL account not active in this branch")]
        //[SwaggerResponse(300018, "Cross Currency Transaction will not be allowed")]
        //[SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked ")]
        
        private IHttpActionResult InternalTransfer(Transactions internalTransferRequest)
        {
            bool isValid = false;
            string strResponse = string.Empty;
            bool ValidAccount = false;
           // string strResponseCode = string.Empty;
            string[] strLoanApplicationResponse = null;

            string OurBranchID = string.Empty;
            string FromAccountID = string.Empty;
            string ToBranchID = string.Empty;
            string ToAccountID = string.Empty;
            string TrxAmount = string.Empty;
            string crTransactionDescription = string.Empty;
            string drTransactionDescription = string.Empty;
            string TrxDescriptionID = string.Empty;
            string UniqueID = string.Empty;
            string ChannelID = string.Empty;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsInternalTrx = new BRDataSet();

            InternalTransferResponse internalTransferResponse = new InternalTransferResponse();
            DataSet resp = null;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (internalTransferRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;
           
                string mydata = string.Empty;

                mydata = JsonConvert.SerializeObject(internalTransferRequest).ToString();

                DataTable dtInternalTransfer = CoreOmniApiBase.dtFromObject(internalTransferRequest, "dt_InternalTransfer");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Internal Transfer", "REQUEST", JsonConvert.SerializeObject(internalTransferRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: InternalTransfer \n REQUEST:{JsonConvert.SerializeObject(internalTransferRequest).ToString()}\n"));

                dsInternalTrx.Tables.Add(dtInternalTransfer);

                ValidAccount = OmniCore.InternalTransfer(usrInfo, mydata, out  dsInternalTrx );
                strResponseCode = dsInternalTrx.Tables[0].Rows[0][0].ToString();
                //strResponse = string.Format("00|Loan Application Successfully|{0}|0|0|{1}", LoanAmount.ToString(), LoanAmount.ToString());

                if (ValidAccount == true)
                {

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                }               

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: InternalTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: InternalTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }




        /// <summary>
        /// Use this API to initiate an internal transfer transaction
        /// </summary>
        /// <param name="internalTransferRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300055, "Both the branches are not on the same working Date")]
        //[SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        //[SwaggerResponse(300056, "GL account not active in this branch")]
        //[SwaggerResponse(300018, "Cross Currency Transaction will not be allowed")]
        //[SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked ")]

        private IHttpActionResult UtilitiesMoneyTransfer(Transactions internalTransferRequest)
        {
            bool isValid = false;
            string strResponse = string.Empty;
            bool ValidAccount = false;
            // string strResponseCode = string.Empty;
            string[] strLoanApplicationResponse = null;

            string OurBranchID = string.Empty;
            string FromAccountID = string.Empty;
            string ToBranchID = string.Empty;
            string ToAccountID = string.Empty;
            string TrxAmount = string.Empty;
            string crTransactionDescription = string.Empty;
            string drTransactionDescription = string.Empty;
            string TrxDescriptionID = string.Empty;
            string UniqueID = string.Empty;
            string ChannelID = string.Empty;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsInternalTrx = new BRDataSet();

            InternalTransferResponse internalTransferResponse = new InternalTransferResponse();
            DataSet resp = null;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (internalTransferRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;

                string mydata = string.Empty;

                mydata = JsonConvert.SerializeObject(internalTransferRequest).ToString();

                DataTable dtInternalTransfer = CoreOmniApiBase.dtFromObject(internalTransferRequest, "dt_InternalTransfer");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Utilities", "REQUEST", JsonConvert.SerializeObject(internalTransferRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: Utilities \n REQUEST:{JsonConvert.SerializeObject(internalTransferRequest).ToString()}\n"));

                dsInternalTrx.Tables.Add(dtInternalTransfer);

                //ValidAccount = OmniCore.UtilitiesMoneyTransfer(usrInfo, mydata,  dsInternalTrx);
                ValidAccount = OmniCore.UtilitiesMoneyTransfer(usrInfo, mydata, out dsInternalTrx);
                strResponseCode = dsInternalTrx.Tables[0].Rows[0][0].ToString();
                //strResponse = string.Format("00|Loan Application Successfully|{0}|0|0|{1}", LoanAmount.ToString(), LoanAmount.ToString());

                if (ValidAccount == true)
                {

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                }

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: Utilities \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: Utilities \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }








        /// <summary>
        /// Use this API to initiate an money transfer transaction
        /// </summary>
        /// <param name="moneyTransferRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "GL account not active in this branch")]
        [SwaggerResponse(300018, "Cross Currency Transaction will not be allowed")]
        [SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked ")]
        private IHttpActionResult MoneyTransfer(MoneyTransfer moneyTransferRequest)
        {
            bool isValid = false;
            string strResponse = string.Empty;
            string[] strMoneyTransferResponse = null;

            string OurBranchID = string.Empty;
            string AccountID = string.Empty;
            string TrxAmount = string.Empty;
            string Recievername = string.Empty;
            string RecieverIDNumber = string.Empty;
            string RecieverMobile = string.Empty;
            string TrxDescriptionID = string.Empty;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsMoneyTransferTrx = new BRDataSet();

            MoneyTransferResponse moneyTransferResponse = new MoneyTransferResponse();
            DataSet resp = null;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (moneyTransferRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;

                OurBranchID = moneyTransferRequest.OurBranchID;
                AccountID = moneyTransferRequest.AccountID;
                Recievername = moneyTransferRequest.Recievername;
                RecieverIDNumber = moneyTransferRequest.RecieverIDNumber;
                TrxAmount = moneyTransferRequest.TrxAmount;
                RecieverMobile = moneyTransferRequest.RecieverMobile;
                TrxDescriptionID = moneyTransferRequest.TrxDescriptionID;


                DataTable dtMoneyTransfer = CoreOmniApiBase.dtFromObject(moneyTransferRequest, "dt_MoneyTransfer");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Money Transfer", "REQUEST", JsonConvert.SerializeObject(moneyTransferRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MoneyTransferRequest \n REQUEST:{JsonConvert.SerializeObject(moneyTransferRequest).ToString()}\n"));

                dsMoneyTransferTrx.Tables.Add(dtMoneyTransfer);

                strResponse = OmniCore.MoneyTransfer(usrInfo, OurBranchID, AccountID, Recievername, RecieverIDNumber, TrxAmount, RecieverMobile,
                TrxDescriptionID, dsMoneyTransferTrx);

                //strResponse = string.Format("00|Loan Application Successfully|{0}|0|0|{1}", LoanAmount.ToString(), LoanAmount.ToString());

                if (string.IsNullOrEmpty(strResponse))
                {

                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                strMoneyTransferResponse = strResponse.Split('|');

                if (strMoneyTransferResponse.Length > 0 && (strMoneyTransferResponse[1] == "00" || strMoneyTransferResponse[1] == "99"))
                {
                    strResponseCode = strMoneyTransferResponse[1] != null ? strMoneyTransferResponse[1] : null;

                    if (strResponseCode == "00")
                    {
                        strResponseMessage = "SUCCESS";
                        //moneyTransferResponse.ClearBalance = strMoneyTransferResponse.Length > 2 ? strMoneyTransferResponse[3] : null;
                        //moneyTransferResponse.AvailableBalance = strMoneyTransferResponse.Length > 4 ? strMoneyTransferResponse[5] : null;
                        moneyTransferResponse.ControlNumber = strMoneyTransferResponse.Length > 4 ? strMoneyTransferResponse[5] : null;
                    }


                    //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(moneyTransferResponse), strResponseMessage);

                }
                else
                {
                    if (strMoneyTransferResponse[0] == "99" & strMoneyTransferResponse[1] == "137006")
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "137006", "Cannot Continue. Insufficient Balance");
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");
                    }
                }

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MoneyTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MoneyTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }
        /// <summary>
        /// Use this API to initiate an internal transfer reversal  transaction
        /// </summary>
        /// <param name="ReversalRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "Both the branches are not on the same working Date")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "GL account not active in this branch")]
        [SwaggerResponse(300008, "Invalid transaction, Transaction Blocked ")]

        private IHttpActionResult TransactionReversal(ReversalRequest ReversalRequest)
        {
            bool isValid = false;
            string strResponse = string.Empty;
            string[] strLoanApplicationResponse = null;

            bool ValidAccount = false;
            string ChannelID = string.Empty;
            string ServiceID = string.Empty;
            string RequestID = string.Empty;
            string OperatorID = string.Empty;
            string TrxBatchID = string.Empty;

            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsTrxReversal = new BRDataSet();

            InternalTransferResponse internalTransferResponse = new InternalTransferResponse();
            DataSet resp = null;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (ReversalRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;

                ChannelID = ReversalRequest.ChannelID;
                ServiceID = ReversalRequest.ServiceID;
                RequestID = ReversalRequest.RequestID;
                OperatorID = ReversalRequest.OperatorID;
                TrxBatchID = ReversalRequest.TrxBatchID;

                string mydata = string.Empty;

                mydata = JsonConvert.SerializeObject(ReversalRequest).ToString();

                DataTable dtInternalTransfer = CoreOmniApiBase.dtFromObject(ReversalRequest, "dt_TransactionReversal");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Transaction Reversal", "REQUEST", JsonConvert.SerializeObject(ReversalRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: TransactionReversal \n REQUEST:{JsonConvert.SerializeObject(ReversalRequest).ToString()}\n"));

                dsTrxReversal.Tables.Add(dtInternalTransfer);

                ValidAccount = OmniCore.TransactionReversal(usrInfo, mydata, out dsTrxReversal);
                strResponseCode = dsTrxReversal.Tables[0].Rows[0][0].ToString();

                if (ValidAccount == true)
                {

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                }
             

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: TransactionReversal \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: TransactionReversal \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

        /// <summary>
        /// Use this API to initiate   transaction Callback
        /// </summary>
        /// <param name="ReversalRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "Both the branches are not on the same working Date")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "GL account not active in this branch")]
        [SwaggerResponse(300008, "Invalid transaction, Transaction Blocked ")]

        private IHttpActionResult transactionCallback(transactionCallback CallbackRequest)
        {
            bool isValid = false;
            string strResponse = string.Empty;
            string[] strLoanApplicationResponse = null;

            
        
            
            

            string BatchID = string.Empty;
            string PTN = string.Empty;
    
            string UniqueID = string.Empty;
           // string OperatorID = string.Empty;

            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            BRDataSet dsTrxReversal = new BRDataSet();

            InternalTransferResponse internalTransferResponse = new InternalTransferResponse();
            DataSet resp = null;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

            if (usrInfo == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Session. Cannot Continue");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            if (CallbackRequest == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                isValid = false;

                //BatchID = CallbackRequest.BatchID;
             
                UniqueID = CallbackRequest.UniqueID;
                PTN = CallbackRequest.PTN;
                //  OperatorID = CallbackRequest.OperatorID;


                DataTable dtInternalTransfer = CoreOmniApiBase.dtFromObject(CallbackRequest, "dt_TransactionCallback");

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Transaction Callback", "REQUEST", JsonConvert.SerializeObject(CallbackRequest).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: transactionCallback \n REQUEST:{JsonConvert.SerializeObject(CallbackRequest).ToString()}\n"));

                dsTrxReversal.Tables.Add(dtInternalTransfer);

                strResponse = OmniCore.transactionCallback(usrInfo,  UniqueID, PTN,  dsTrxReversal);

                //strResponse = string.Format("00|Loan Application Successfully|{0}|0|0|{1}", LoanAmount.ToString(), LoanAmount.ToString());

                if (string.IsNullOrEmpty(strResponse))
                {

                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                strLoanApplicationResponse = strResponse.Split('|');

                if (strLoanApplicationResponse.Length > 0 && (strLoanApplicationResponse[1] == "00" || strLoanApplicationResponse[1] == "99"))
                {
                    strResponseCode = strLoanApplicationResponse[1] != null ? strLoanApplicationResponse[1] : null;

                    if (strResponseCode == "00")
                    {
                        strResponseMessage = "SUCCESS";
                       // internalTransferResponse.ClearBalance = strLoanApplicationResponse.Length > 2 ? strLoanApplicationResponse[3] : null;
                       // internalTransferResponse.AvailableBalance = strLoanApplicationResponse.Length > 4 ? strLoanApplicationResponse[5] : null;
                    }


                    //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(""), strResponseMessage);

                }
                else
                {
                    if (strLoanApplicationResponse[0] == "99" & strLoanApplicationResponse[1] == "137006")
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "137006", "Cannot Continue. Insufficient Balance");
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300059", "Cannot Continue. Contact Administrator");
                    }
                }

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: InternalTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: InternalTransfer \n RESPONSE:{JsonConvert.SerializeObject(strResponse).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

    }
}
