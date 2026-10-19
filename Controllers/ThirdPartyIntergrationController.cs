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
    public class ThirdPartyIntergrationController : ApiController
    {

        
        ///// <summary>
        ///// Use this API to Get Pending Disbursement List
        ///// </summary>
        ///// <param name="myData"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Pending Disbursement")]
        
        //public IHttpActionResult GetPendingDisbursementList(GetRequestParameter myData)
        //{
            
        //    string UniqueReferenceID = string.Empty;
            
        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;
        //    DataSet resp = null;
        //    BRDataSet ds_PendingDisbursementList = new BRDataSet();
            

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return  CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;


                
        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;


        //        // Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetPendingDisbursementList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));




        //        isValid = OmniCore.GetPendingDisbursementList(usrInfo, out ds_PendingDisbursementList);


        //        if (isValid == true)
        //        {
        //            strResponseCode = "00";
        //            strResponseMessage = "Successful";
        //            if (ds_PendingDisbursementList.Tables[0].Rows.Count > 0)
        //            {

        //               // lstPendingDisbursementResponse = ds_PendingDisbursementList.Tables[0].ToListof<PendingDisbursementResponse>();

        //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(ds_PendingDisbursementList), strResponseMessage);
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //            }
        //            else
        //            {
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, null, "No Records Found");
        //            }



        //        }


        //        else
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300061", "Invalid Pending Disbursement");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}



        ///// <summary>
        ///// Use this API to Validate Customer Disbursement Details
        ///// </summary>
        ///// <param name="myData"></param>
        ///// 
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]

        //[SwaggerResponse(300018, "Invalid Amount")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]

        //public IHttpActionResult BRDisbursementValidation(AccountDisbValidationRequest myData)
        //{

            
        //    BRDataSet ds_DisbursementValidation = new BRDataSet();
        //    string strResponse = string.Empty;
            
        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;



                
                

        //        if (myData.Amount <= 0)
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300018", "Invalid Amount");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300059", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

                

        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;
        //        //myData.OurBranchID = usrInfo.strBranch;

        //        DataTable dtDisbursementValidation = CoreOmniApiBase.dtFromObject(myData, "dt_DisbursementValidation");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRDisbursementValidation", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));

        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRDisbursementValidation \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));


        //        ds_DisbursementValidation.Tables.Add(dtDisbursementValidation);


        //        isValid = OmniCore.BRDisbursementValidation(usrInfo, ds_DisbursementValidation);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Validation Passed";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Validation Failed";
        //        }
        //          resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
          

        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        ///// <summary>
        ///// Use this API to update Customer Disbursement Details
        ///// </summary>
        ///// <param name="myData"></param>
        ///// 

        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRDisbursementTransfer(AccountDisburseTransfer myData)
        //{


        //    BRDataSet ds_DisbursementTransfer = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtDisbursementTransfer = CoreOmniApiBase.dtFromObject(myData, "dt_DisbursementTransfer");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRDisbursementTransfer", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRDisbursementTransfer \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_DisbursementTransfer.Tables.Add(dtDisbursementTransfer);


        //        isValid = OmniCore.BRAccountDisbursementTransfer(usrInfo, ds_DisbursementTransfer);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Successful Transaction";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Failed Transaction";
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
        ///// Use this API to set retry on Customer Disbursement Details
        ///// </summary>
        ///// <param name="myData"></param>
        ///// 
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRAccountDisbursementRetries(AccountDisbursementRetries myData)
        //{


        //    BRDataSet ds_AccountDisbursementRetries = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtAccountDisbursementRetries = CoreOmniApiBase.dtFromObject(myData, "dt_AccountDisbursementRetries");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRAccountDisbursementRetries", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRAccountDisbursementRetries \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_AccountDisbursementRetries.Tables.Add(dtAccountDisbursementRetries);


        //        isValid = OmniCore.BRAccountDisbursementRetries(usrInfo, ds_AccountDisbursementRetries);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Retry Successful";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Retry Failed";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        ///// <summary>
        ///// Use this API to update exception error during  the Customer Disbursement
        ///// </summary>
        ///// <param name="myData"></param>
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRAccountDisbursementExceptionError(AccountDisbursementExceptionError myData)
        //{


        //    BRDataSet ds_AccountDisbursementExceptionError = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtAccountDisbursementExceptionError = CoreOmniApiBase.dtFromObject(myData, "dt_AccountDisbursementExceptionError");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRAccountDisbursementExceptionError", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRAccountDisbursementExceptionError \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));
        //        ds_AccountDisbursementExceptionError.Tables.Add(dtAccountDisbursementExceptionError);


        //        isValid = OmniCore.BRAccountDisbursementExceptionError(usrInfo, ds_AccountDisbursementExceptionError);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Successful Logged Error Transaction";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Failed to Log Error Transaction";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        ///// <summary>
        ///// Use this API to Get Pending List of CRB Details
        ///// </summary>
        ///// <param name="myData"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Pending CRB Details")]
        //public IHttpActionResult GetPendingCRBList(GetRequestParameter myData)
        //{

        //    //BRDataSet ds_InternalTransfer = new BRDataSet();

        //    string UniqueReferenceID = string.Empty;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;
        //    DataSet resp = null;
        //    BRDataSet ds_PendingCRBList = new BRDataSet();


        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;


        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingCRBList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetPendingCRBList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));





        //        isValid = OmniCore.GetPendingCRBList(usrInfo, out ds_PendingCRBList);


        //        if (isValid == true)
        //        {
        //            strResponseCode = "00";
        //            strResponseMessage = "Successful";
        //            if (ds_PendingCRBList.Tables[0].Rows.Count > 1)
        //            {

        //                // lstPendingDisbursementResponse = ds_PendingDisbursementList.Tables[0].ToListof<PendingDisbursementResponse>();

        //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(ds_PendingCRBList), strResponseMessage);
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //            }
        //            else
        //            {
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, null, "No Records Found");
        //            }



        //        }


        //        else
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300061", "Invalid Pending CRB Details");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRUpdateAccountCRBDetails(AccountCRBDetails myData)
        //{


        //    BRDataSet ds_AccountCRBDetails = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtAccountCRBDetails = CoreOmniApiBase.dtFromObject(myData, "dt_AccountCRBDetails");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRUpdateAccountCRBDetails", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRUpdateAccountCRBDetails \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_AccountCRBDetails.Tables.Add(dtAccountCRBDetails);


        //        isValid = OmniCore.BRUpdateAccountCRBDetails(usrInfo, ds_AccountCRBDetails);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Successful Updated CRB Details";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Failed to Update CRB Details";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Pending Emails Details")]
        //public IHttpActionResult GetPendingEmailList(GetRequestParameter myData)
        //{

        //    //BRDataSet ds_InternalTransfer = new BRDataSet();

        //    string UniqueReferenceID = string.Empty;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;
        //    DataSet resp = null;
        //    BRDataSet ds_PendingEmailList = new BRDataSet();


        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;


        //        // Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingEmailList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetPendingEmailList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));




        //        isValid = OmniCore.GetPendingEmailNotifications(usrInfo, out ds_PendingEmailList);


        //        if (isValid == true)
        //        {
        //            strResponseCode = "00";
        //            strResponseMessage = "Successful";
        //            if (ds_PendingEmailList.Tables[0].Rows.Count > 1)
        //            {

        //                // lstPendingDisbursementResponse = ds_PendingDisbursementList.Tables[0].ToListof<PendingDisbursementResponse>();

        //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(ds_PendingEmailList), strResponseMessage);
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //            }
        //            else
        //            {
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, null, "No Records Found");
        //            }



        //        }


        //        else
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300061", "Invalid Pending Email Details");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}


        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Pending SMS Details")]
        //public IHttpActionResult GetPendingSMSList(GetRequestParameter myData)
        //{

        //    //BRDataSet ds_InternalTransfer = new BRDataSet();

        //    string UniqueReferenceID = string.Empty;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;
        //    DataSet resp = null;
        //    BRDataSet ds_PendingSMSList = new BRDataSet();


        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;


        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingSMSList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));

        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetPendingSMSList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));


        //        isValid = OmniCore.GetPendingSMSNotifications(usrInfo, out ds_PendingSMSList);


        //        if (isValid == true)
        //        {
        //            strResponseCode = "00";
        //            strResponseMessage = "Successful";
        //            if (ds_PendingSMSList.Tables[0].Rows.Count > 1)
        //            {

        //                // lstPendingDisbursementResponse = ds_PendingDisbursementList.Tables[0].ToListof<PendingDisbursementResponse>();

        //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(ds_PendingSMSList), strResponseMessage);
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //            }
        //            else
        //            {
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, null, "No Records Found");
        //            }

        //        }


        //        else
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300061", "Invalid Pending Email Details");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRUpdateNotificationStatus(AccountNotificationDetails myData)
        //{


        //    BRDataSet ds_AccountNotificationDetails = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtAccountNotificationDetails = CoreOmniApiBase.dtFromObject(myData, "dt_AccountNotificationDetails");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRUpdateNotificationStatus", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRUpdateNotificationStatus \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_AccountNotificationDetails.Tables.Add(dtAccountNotificationDetails);


        //        isValid = OmniCore.BRUpdateNotificationStatus(usrInfo, ds_AccountNotificationDetails);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Successful Updated Notification Status";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Failed to Update Notification Status";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}


        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRUpdateNotificationRetries(AccountNotificationDetails myData)
        //{


        //    BRDataSet ds_AccountNotificationDetails = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtAccountNotificationDetails = CoreOmniApiBase.dtFromObject(myData, "dt_AccountNotificationDetails");

        //        /// Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRUpdateNotificationRetries", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        /// 
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRUpdateNotificationRetries \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_AccountNotificationDetails.Tables.Add(dtAccountNotificationDetails);


        //        isValid = OmniCore.BRUpdateNotificationRetries(usrInfo, ds_AccountNotificationDetails);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Successful Updated Notification Retry";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Failed to Update Notification Retry";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}

        ///// <summary>
        ///// Use this API to Get Pending Disbursement List
        ///// </summary>
        ///// <param name="myData"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Pending Loan Recovery")]

        //public IHttpActionResult GetPendingLoanRecoveryList(GetRequestParameter myData)
        //{

        //    string UniqueReferenceID = string.Empty;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;
        //    DataSet resp = null;
        //    BRDataSet ds_PendingLoanRecoveryList = new BRDataSet();


        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;


        //        // Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetPendingLoanRecoveryList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));




        //        isValid = OmniCore.GetPendingLoanRecoveryList(usrInfo, out ds_PendingLoanRecoveryList);


        //        if (isValid == true)
        //        {
        //            strResponseCode = "00";
        //            strResponseMessage = "Successful";
        //            if (ds_PendingLoanRecoveryList.Tables[0].Rows.Count > 0)
        //            {

        //                // lstPendingDisbursementResponse = ds_PendingDisbursementList.Tables[0].ToListof<PendingDisbursementResponse>();

        //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(ds_PendingLoanRecoveryList), strResponseMessage);
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //            }
        //            else
        //            {
        //                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, null, "No Records Found");
        //            }



        //        }


        //        else
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300061", "Invalid Pending Disbursement");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}


        //[HttpPost]
        //[SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300058, "Invalid Account")]
        //[SwaggerResponse(300059, "Mobile Number")]
        //public IHttpActionResult BRAccountLoanRecoveryRetries(LoanRecoveryRetries myData)
        //{


        //    BRDataSet ds_LoanRecoveryRetries = new BRDataSet();
        //    string strResponse = string.Empty;

        //    DataSet resp = null;

        //    string strResponseCode = string.Empty;
        //    string strResponseMessage = string.Empty;

        //    if (myData == null)
        //    {
        //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //    }

        //    try
        //    {
        //        bool isValid = false;

        //        if (string.IsNullOrEmpty(myData.AccountID))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Invalid Account");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }

        //        if (string.IsNullOrEmpty(myData.MobileNumber))
        //        {
        //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile Number");
        //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
        //        }



        //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
        //        usrInfo.strUser = CoreOmniApiBase.gOperatorID;
        //        myData.OperatorID = CoreOmniApiBase.gOperatorID;

        //        DataTable dtLoanRecoveryRetries = CoreOmniApiBase.dtFromObject(myData, "dt_LoanRecoveryRetries");

        //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "BRAccountDisbursementRetries", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: BRAccountLoanRecoveryRetries \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

        //        ds_LoanRecoveryRetries.Tables.Add(dtLoanRecoveryRetries);


        //        isValid = OmniCore.BRAccountLoanRecoveryRetries(usrInfo, ds_LoanRecoveryRetries);


        //        if (isValid)
        //        {
        //            strResponseCode = "True";
        //            strResponseMessage = "Retry Successful";
        //        }
        //        else
        //        {
        //            strResponseCode = "False";
        //            strResponseMessage = "Retry Failed";
        //        }
        //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
        //    }
        //}
    }
}
