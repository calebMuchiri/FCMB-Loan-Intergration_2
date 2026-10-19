using System;
using System.Globalization;
using System.Web;
using System.Collections;
using BRControls;
using Nii.JSON;
using BREntities.LoanInitiationDetail;
using System.Diagnostics;

using System.Data;
using BR.Encryption;
using System.Text;
using System.IO;
using BREntities.WFLoanApplications;
using BREntities.WFLoanAppraisals;
using BREntities.WFLoanAppraisal;
using BREntities.GroupLoanDisbursement;
using BREntities.CurrencyDenominations;
using BRCoreEntities.SystemBranchStatus;
using System.Collections.Specialized;
using System.Linq;
using BRBase;
using BR.Configuration;
using BRCoreDALLibrary;
using System.Web.Script.Serialization;
using System.Reflection;
using BREntities.LoanPreClosureDetail;
using BREntities.Transactions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BRCoreEntities.SystemMessages;
using BREntities.ProductLoan;
using BREntities.LoanIntCalcRule;
using BREntities.LoanCalulator;
using BREntities.Loans;

using System.Web.Http;
using Newtonsoft.Json;
using System.Configuration;

using OmniApi.CoreLibraries;
using System.Security.Cryptography;
using BR.DBClient;
using BR.ApplicationBlocks.Data;
using WebApi.Jwt.Models;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.IO.Compression;

/// <summary>
/// Summary description for Base
/// </summary>
public class CoreOmniApiBase : ApiController
{
    static string Response = string.Empty;
    public static string OmniApiRoute = "OmniApi";
    public static string gOperatorID = string.Empty;
    public static string gSessionID = string.Empty;

    static string strgBRUserName = "";
    static string strgSYSADMIN1UserName = "";
    static string strgBRUserPassword = "";
    static string strgSYSADMIN1Password = "";
    static string strgDatabaseName = "";
    static string strgDBServer = "";

    static string strgConnection = "";
    public static string strgSecurityChecks = "";
    public static string strgModifiedObjects = "";
    public static bool ValidUser = false;
    private const string ResponseErroMessage = "Cannot Continue. Contact Administrator";
    // This constant is used to determine the keysize of the encryption algorithm in bits.
    // We divide this by 8 within the code below to get the equivalent number of bytes.
    private const int Keysize = 256;

    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;


    public CoreOmniApiBase()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    
    #region General
    public static bool _hasValue(object val)
    {
        if (val != null && val.ToString() != string.Empty)
            return true;

        return false;


    }

    private static string FormatWebMethodResponse(StringDictionary myOutPuts)
    {
        StackTrace stackTrace = new StackTrace();
        string MethodName = stackTrace.GetFrame(1).GetMethod().Name;
        string Response = string.Empty;
        string format = "<{0}>{1}</{0}>";
        foreach (DictionaryEntry kv in myOutPuts)
        {
            Response = Response + string.Format(format, kv.Key, kv.Value);
        }
        return string.Format(format, MethodName, Response);

    }

    public enum WFStageID
    {
        APPL, // Application
        APPR, // Appraisal
        SANC, // Sanction
        DISB, // Disbursement
    }


    #endregion

  

    

    #region Error Handling Functions

    public static DataSet GetInvalidSessionIDMessage()
    {
        StackTrace stackTrace = new StackTrace();
        string MethodName = stackTrace.GetFrame(1).GetMethod().Name;
        Response = "Invalid Session";

        DataSet dsR = new DataSet();
        dsR = GetErrorMessage("99", "", Response);

        
        //Response = FormatToJson(true,Response);

        return dsR;

    }

    public static DataTable GetInvalidSessionIDMessageDt()
    {
        StackTrace stackTrace = new StackTrace();
        string MethodName = stackTrace.GetFrame(1).GetMethod().Name;
        Response = "Invalid Session";

        DataSet dsR = new DataSet();
        dsR = GetErrorMessage("99", "", Response);


        //Response = FormatToJson(true,Response);

        return dsR.Tables[0];

    }

    public static DataSet GetErrorMessage(string status, string ResponseNo, string message)
    {
        DataSet dsResponse = new DataSet();

        dsResponse.Tables.Add("status");

        dsResponse.Tables["status"].Columns.Add("WebServiceStatus");
        dsResponse.Tables["status"].Columns.Add("Response");
        dsResponse.Tables["status"].Columns.Add("Message");
        dsResponse.Tables["status"].Rows.Add(status);
        dsResponse.Tables["status"].Rows[0]["Response"] = ResponseNo;
        dsResponse.Tables["status"].Rows[0]["Message"] = message;

        return dsResponse;
    }

    public static DataSet GetBRErrorMessage(Exception ex)
    {
        StackTrace stackTrace = new StackTrace();
        string MethodName = stackTrace.GetFrame(1).GetMethod().Name;
        ParameterInfo[] info = stackTrace.GetFrame(1).GetMethod().GetParameters();
        DataSet dsResponse = new DataSet();

        object SessionID = null;
                
            foreach (ParameterInfo inf in info)
            {
                if (inf.Name.ToLower().Contains("sessionid"))
                {
                    SessionID = inf.DefaultValue;
                    break;
                }

            }

            if (System.Convert.ToBoolean(BRConfigurationManager.BRAppSettings["AuditRequired"]) == true)
            {
                if (BRConfigurationManager.BRAppSettings["ErrorLogPath"].ToString() != "")
                {
                    string strFileName = BRConfigurationManager.BRAppSettings["ErrorLogPath"].ToString();
                    System.IO.Directory.CreateDirectory(strFileName);

                    string AppendErrorMessage = Environment.NewLine + "Error Message" + ":" + ex.Message
                        + Environment.NewLine + ex.StackTrace
                         + Environment.NewLine + ex.TargetSite
                          + Environment.NewLine + ex.Source +
                        "Date" + ":" + DateTime.Now + Environment.NewLine + "--------------------------" + Environment.NewLine;
                    System.IO.File.AppendAllText(strFileName + "CoreOmniAPI - Error Log - " + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt", AppendErrorMessage);
                }
            }

            Response = ex.Message.ToString();

            dsResponse.Tables.Add("status");

            dsResponse.Tables["status"].Columns.Add("WebServiceStatus");
            dsResponse.Tables["status"].Columns.Add("Response");
            dsResponse.Tables["status"].Columns.Add("Message");
            dsResponse.Tables["status"].Rows.Add("99");
            dsResponse.Tables["status"].Rows[0]["Response"] = "";
            dsResponse.Tables["status"].Rows[0]["Message"] = Response;

           
           //Response = FormatToJson(true,Response);

           return dsResponse;
    }

    public static DataTable GetBRErrorMessageDt(Exception ex)
    {
        StackTrace stackTrace = new StackTrace();
        string MethodName = stackTrace.GetFrame(1).GetMethod().Name;
        ParameterInfo[] info = stackTrace.GetFrame(1).GetMethod().GetParameters();
        DataSet dsResponse = new DataSet();

        object SessionID = null;

        foreach (ParameterInfo inf in info)
        {
            if (inf.Name.ToLower().Contains("sessionid"))
            {
                SessionID = inf.DefaultValue;
                break;
            }

        }

        if (System.Convert.ToBoolean(BRConfigurationManager.BRAppSettings["AuditRequired"]) == true)
        {
            if (BRConfigurationManager.BRAppSettings["ErrorLogPath"].ToString() != "")
            {
                string strFileName = BRConfigurationManager.BRAppSettings["ErrorLogPath"].ToString();
                System.IO.Directory.CreateDirectory(strFileName);

                string AppendErrorMessage = Environment.NewLine + "Error Message" + ":" + ex.Message
                    + Environment.NewLine + ex.StackTrace
                     + Environment.NewLine + ex.TargetSite
                      + Environment.NewLine + ex.Source +
                    "Date" + ":" + DateTime.Now + Environment.NewLine + "--------------------------" + Environment.NewLine;
                System.IO.File.AppendAllText(strFileName + "CoreOmniAPI - Error Log - " + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt", AppendErrorMessage);
            }
        }

        Response = ex.Message.ToString();

        dsResponse.Tables.Add("status");

        dsResponse.Tables["status"].Columns.Add("WebServiceStatus");
        dsResponse.Tables["status"].Columns.Add("Response");
        dsResponse.Tables["status"].Columns.Add("Message");
        dsResponse.Tables["status"].Rows.Add("99");
        dsResponse.Tables["status"].Rows[0]["Response"] = "";
        dsResponse.Tables["status"].Rows[0]["Message"] = Response;


        //Response = FormatToJson(true,Response);

        return dsResponse.Tables[0];
    }

    public static string[] RetrieveBRErrorInfo(Exception ex, UserInfo usrInfo)
    {
        Regex reg = new Regex(@"BREX[UD][IB][0-9]*(.*)*");
        Match mt = reg.Match(ex.Message);
        if (mt.Value.Length != 0)
        {
            int paramIndex = mt.Value.IndexOf("(");
            String strCode = mt.Value.Substring(6, paramIndex == -1 ? mt.Value.Length - 6 : paramIndex - 6);
            if (mt.Value.Substring(4, 2) == "DB")
                return new string[] { strCode, getDBErrMessage(strCode, paramIndex == -1 ? null : GetParameters(mt.Value), usrInfo) };//added by sudharshan
            else if (paramIndex == -1)
                return new string[] { strCode, BR.Resources.GetLocalisedText(strCode) }; //+ "[No:" + strCode + "]" };
            else
                return new string[] { strCode, BR.Resources.GetLocalisedText(strCode, GetParameters(mt.Value)) };// + "[No:" + strCode + "]" };
        }
        else
        {
            //BEGIN --  Temp to display only error message without stack trace
            reg = new Regex(@"BREXMSG\[[A-Za-z -_,:]*\]");
            mt = reg.Match(ex.Message);
            if (mt.Value.Length != 0)
            {
                String strCode = "SYSERR";
                return new string[] { strCode, mt.Value.Substring(8).Remove(mt.Value.Substring(8).Length - 1) };
            }
            else
            {
                if (ex.Message == "The operation has timed out")
                    return new string[] { "1699", BR.Resources.GetLocalisedText("1699") };// + "[No:1699]" };
                else
                    return new string[] { "1697", BR.Resources.GetLocalisedText("1697") };//+ "[No:1697]" };

            }
        }
    }

    private static string getDBErrMessage(string strCode, string[] arguments, UserInfo usrInfo)
    {
        DS_SystemMessages dsSysMessages = null;

        try
        {
            if (dsSysMessages == null)
            {
                dsSysMessages = BRDALSystemModules.GetSysMessages(usrInfo);
                if (dsSysMessages.t_SystemMessages == null)
                {
                    return BR.Resources.GetLocalisedText("1035") + "[No:1035]";
                }
            }
            string selectCriteria = string.Empty;
            int iMessageID = BRBaseConvert.ConvertToInt32(strCode.Trim());
            selectCriteria = string.Format("MessageID = '{0}'", iMessageID.ToString());
            DataRow[] drSelected = dsSysMessages.t_SystemMessages.Select(selectCriteria);

            string strResult = "";

            if (drSelected.Length > 0)
                strResult = GetDescriptionWithParam(drSelected[0]["Description"].ToString(), arguments);// + "[No:" + strCode + "]";
            else if (iMessageID == -2)
                strResult = BR.Resources.GetLocalisedText("1698");// + "[No:1698]";
            else
                strResult = BR.Resources.GetLocalisedText("1023");// +"[No:1023]";

            return strResult;
        }
        catch (Exception ex) { throw (ex); }
        finally
        {
            dsSysMessages.Dispose();
        }
    }

    private static string[] GetParameters(string strErrMessage)
    {
        // This function is for filtering parameters presebt in the exception message
        string[] strParams;
        String strMessage = strErrMessage.Substring(strErrMessage.IndexOf("("));
        strParams = strMessage.Replace("(", string.Empty).Split(')');
        return strParams;
    }

    public static string GetDescriptionWithParam(string Msg, params string[] arguments)
    {
        // This function is for replacing {0},{1}.. argument in the message with values
        if ((Msg == null) || (Msg.Length == 0))
            return Msg;
        else if (arguments != null)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                Msg = Msg.Replace("{" + i + "}", arguments[i]);
            }
        }
        return Msg;
    }

    #endregion Error Handling Functions




    #region Session token management

    public static string RequestConnectionSession(DataTable dtUserInfo)
    {
        string mySystem = string.Empty;
        string SessionID = string.Empty;
        string strbankID = string.Empty;
        string strBranchID = string.Empty;
        string strUserName = string.Empty;
        string strPassword = string.Empty;
        string IPAddress = string.Empty;
        string EncryptPassword = string.Empty;
        string strLanguageID = string.Empty;
        string strEncSession = string.Empty;
        string imgFile = string.Empty;
        ushort uAccessLevel;
        string RoleID;
        string strNewSession = string.Empty;
        bool bisService = true;
        string message = string.Empty;


        System.Guid guid = System.Guid.NewGuid();
        UserInfo usrInfo = new UserInfo();
        foreach (DataRow row in dtUserInfo.Rows)
        {
            SessionID = "";
            strBranchID = row[0].ToString();
            mySystem = row[1].ToString();
            strbankID = row[2].ToString();
            strUserName = row[3].ToString().ToUpper();
            strPassword = row[4].ToString();
            IPAddress = row[5].ToString();
        }
        usrInfo.CType = WebSecurity.EncryptionHelper.Encrypt("~External" + strUserName);
        //ffff
        ASPUtils.SessionID = guid.ToString() + usrInfo.strSystem + usrInfo.strBank;
        usrInfo.strBank = strbankID;
        usrInfo.strBranch = strBranchID;
        usrInfo.strUser = strUserName.ToUpper();
        usrInfo.strSystem = mySystem;
        usrInfo.MachineIP = IPAddress;
        usrInfo.SessionID = guid.ToString() + usrInfo.strSystem + usrInfo.strBank + usrInfo.strUser + usrInfo.MachineIP;
        usrInfo.isService = bisService;

        string strDBServerName = BRConfigurationManager.BRAppSettings["DBServerName"];
        string strDatabaseName = BRConfigurationManager.BRAppSettings["DatabaseName"];
        string strBRUserName = BRConfigurationManager.BRAppSettings["BRUserName"];
        string strBRUserPassword = BRConfigurationManager.BRAppSettings["BRUserPassword"];
        string strBRDBType = BRConfigurationManager.BRAppSettings["BRDBType"];
        string strRoleID = string.Empty;
        string strTokenDuration = BRConfigurationManager.BRAppSettings["TokenDuration"];
        string strFrequency = BRConfigurationManager.BRAppSettings["Frequency"];

        string myIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
        string myHostName = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).HostName.ToString();
        usrInfo.strDBServerName = strDBServerName;
        usrInfo.strDatabaseName = strDatabaseName;
        usrInfo.strBRUserName = strBRUserName;
        usrInfo.strBRUserPassword = strBRUserPassword;
        usrInfo.strBRDBType = strBRDBType;
        usrInfo.strUser = gOperatorID;

        EncryptPassword = BREncryption.EncryptText(ASPUtils.CreateEncryptData(usrInfo, strPassword));

        //    var macAddr =
        //(
        //    from nic in NetworkInterface.GetAllNetworkInterfaces()
        //    where nic.OperationalStatus == OperationalStatus.Up
        //    select nic.GetPhysicalAddress().ToString()
        //).FirstOrDefault();


        ValidUser = AuthenticateUser(usrInfo, EncryptPassword, out strLanguageID, out uAccessLevel, out RoleID);




        if (ValidUser == true)
        {
            string FullipHostName = string.Empty;
            if (myIP == "")
                FullipHostName = usrInfo.MachineIP;
            else
                FullipHostName = myIP + "  " + myHostName;
            Dictionary<string, string> sessionItems = new Dictionary<string, string>();
            sessionItems.Add("uid", strUserName);
            sessionItems.Add("sys", mySystem);
            sessionItems.Add("bra", strBranchID);
            var json = new JavaScriptSerializer().Serialize(sessionItems.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString()));
            SessionID = WebSecurity.EncryptionHelper.Encrypt(ASPUtils.SessionID + "~" + json);
            BRDataSet ds_IsTokenValid = new BRDataSet();
            ValidateSessionID(usrInfo, out ds_IsTokenValid, new object[] { SessionID, "ADD", FullipHostName, strFrequency, strTokenDuration });


            //SessionID = JsonConvert.SerializeObject(new { Error = !ValidUser, SessionID = SessionID }, Newtonsoft.Json.Formatting.None);


        }
        else
        {
            message = "Invalid Credentials";
            //SessionID = JsonConvert.SerializeObject(new { Error = !ValidUser, message = message}, Newtonsoft.Json.Formatting.Indented);
        }


        return SessionID;

    }
    static public bool AuthenticateUser(UserInfo usrInfo, string strEncryptedPassword, out string strLanguageID, out ushort uAccessLevel, out string strRoleID)
    {
        strLanguageID = string.Empty;
        uAccessLevel = 0;
        try
        {
            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                strLanguageID = "";
                uAccessLevel = 0;
                strRoleID = "";
                DataSet dsTemp = new DataSet();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = usrInfo.strBranch;
                arParms[1] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[1].Value = usrInfo.strUser;
                arParms[2] = intfDBHelper.CreateNewDBParam("IPAddress", SqlDbType.NVarChar, 15);
                arParms[2].Value = usrInfo.MachineIP;

                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_Authenticateuser", dsTemp, new string[] { "DT_Temp" }, arParms);
                if (dsTemp.Tables["DT_Temp"] == null)
                    return false;
                if (dsTemp.Tables["DT_Temp"].Rows.Count != 0)
                {
                    //string logInStatus = dsTemp.Tables["DT_Temp"].Rows[0]["IsLoggedIn"].ToString().ToUpper();
                    bool isLoggedIn = ((dsTemp.Tables["DT_Temp"].Rows[0]["IsLoggedIn"].ToString().ToUpper() == "TRUE") || (dsTemp.Tables["DT_Temp"].Rows[0]["IsLoggedIn"].ToString().ToUpper() == "1")) ? true : false; //&& (dsTemp.Tables["DT_Temp"].Rows[0]["IPAddress"].ToString() != usrInfo.MachineIP.ToString().Substring(0, dsTemp.Tables["DT_Temp"].Rows[0]["IPAddress"].ToString().Length))) ? true : false;
                    if (isLoggedIn && !usrInfo.IsAdmin)
                    {
                        strLanguageID = dsTemp.Tables["DT_Temp"].Rows[0]["LanguageID"].ToString() + ":" + dsTemp.Tables["DT_Temp"].Rows[0]["ChangePasswordAtNextLogon"].ToString() + ":" + dsTemp.Tables["DT_Temp"].Rows[0]["Status"].ToString();
                        return false;
                    }
                    if (dsTemp.Tables["DT_Temp"].Rows[0]["Password"].ToString() == strEncryptedPassword)
                    {
                        strLanguageID = dsTemp.Tables["DT_Temp"].Rows[0]["LanguageID"].ToString() + ":" + dsTemp.Tables["DT_Temp"].Rows[0]["ChangePasswordAtNextLogon"].ToString() + ":" + dsTemp.Tables["DT_Temp"].Rows[0]["Status"].ToString();
                        uAccessLevel = System.Convert.ToUInt16(dsTemp.Tables["DT_Temp"].Rows[0]["Accesslevel"]);
                        if (dsTemp.Tables["DT_Temp"].Rows[0]["RoleID"] != null)
                            strRoleID = dsTemp.Tables["DT_Temp"].Rows[0]["RoleID"].ToString();
                        if (dsTemp.Tables["DT_Temp"].Rows[0]["Status"].ToString() == 1.ToString())
                        {

                            UpdateLoginStatus(usrInfo, 'S', 1017);
                        }


                        return true;
                    }
                    else
                    {
                        UpdateLoginStatus(usrInfo, 'F', 000283);
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
        }
        return false;
    }
    static public void UpdateLoginStatus(UserInfo usrInfo, char CSuccess, int IMessageID)
    {
        try
        {
            AuditInfo auditInfo = new AuditInfo();
            if (CSuccess.ToString() == "L")
            {
                auditInfo.EventID = BROperation.LoggedOut;
                auditInfo.ModuleID = BRModule.LogOut;
            }
            else
            {
                auditInfo.EventID = BROperation.LoggedIn;
                auditInfo.ModuleID = BRModule.Login;
            }

            if (CSuccess.ToString() == "F")
                auditInfo.Status = BRStatus.Failed;
            else
                auditInfo.Status = BRStatus.Successful;
            auditInfo.MsgID = IMessageID;
            auditInfo.Message = BR.Resources.GetLocalisedText(IMessageID.ToString());
            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                using (IDbTransaction trans = connection.BeginTransaction())
                {
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);
                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                    arParms[0].Value = usrInfo.strBranch;
                    arParms[1] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                    arParms[1].Value = usrInfo.strUser;
                    arParms[2] = intfDBHelper.CreateNewDBParam("Success", SqlDbType.Char, 1);
                    arParms[2].Value = CSuccess;
                    arParms[3] = intfDBHelper.CreateNewDBParam("IPAddress", SqlDbType.NVarChar, 15);
                    arParms[3].Value = usrInfo.MachineIP;
                    arParms[4] = intfDBHelper.CreateNewDBParam("MessageID", SqlDbType.Int);
                    arParms[4].Value = IMessageID;

                    intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_UpdateLoginStatus", arParms);
                    BRDALSystemModules.PassAuditInformation(trans, usrInfo, string.Empty, string.Empty, auditInfo);
                    trans.Commit();
                }
            }
        }
        catch (Exception ex)
        {
            throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
        }
    }
    static public bool ValidateSessionID(UserInfo usrInfo, out BRDataSet ds_IsTokenValid, params object[] paramArray)
    {

        AuditInfo auditInfo = new AuditInfo();
        auditInfo.ModuleID = BRModule.Client;
        auditInfo.EventID = BROperation.View;
        auditInfo.Status = BRStatus.Successful;
        BRDataSet dsSuperVision = new BRDataSet();
        ds_IsTokenValid = new BRDataSet();
        dsSuperVision = new BRDataSet();
        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(7);

        string sessionID = (paramArray.Length > 0 && paramArray[0] != null ? paramArray[0].ToString() : string.Empty);
        string EventID = (paramArray.Length > 1 && paramArray[1] != null ? paramArray[1].ToString() : string.Empty);
        string IpAddress = (paramArray.Length > 2 && paramArray[2] != null ? paramArray[2].ToString() : string.Empty);
        string strFrequency = (paramArray.Length > 3 && paramArray[0] != null ? paramArray[3].ToString() : string.Empty);
        string strTokenDuration = (paramArray.Length > 4 && paramArray[4] != null ? paramArray[4].ToString() : "0");


        arParms[0] = intfDBHelper.CreateNewDBParam("TokenID", SqlDbType.NVarChar, 1000);
        arParms[0].Value = sessionID;
        arParms[1] = intfDBHelper.CreateNewDBParam("EventID", SqlDbType.NVarChar, 10);
        arParms[1].Value = EventID;
        arParms[2] = intfDBHelper.CreateNewDBParam("IpAddress", SqlDbType.NVarChar, 100);
        arParms[2].Value = IpAddress;
        arParms[3] = intfDBHelper.CreateNewDBParam("Frequency", SqlDbType.NVarChar, 20);
        arParms[3].Value = strFrequency;
        arParms[4] = intfDBHelper.CreateNewDBParam("TokenDuration", SqlDbType.NVarChar, 2);
        arParms[4].Value = int.Parse(strTokenDuration);
        arParms[5] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
        arParms[5].Value = usrInfo.strBranch;
        arParms[6] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 15);
        arParms[6].Value = usrInfo.strUser;
        try
        {
            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GatewayAPIAccess", ds_IsTokenValid, new string[] { "status" }, arParms);
            }

            if (ds_IsTokenValid.Tables[0].Rows.Count == 0)
                return false;
            else
                return true;
        }
        catch (Exception ex)
        {
            auditInfo.Status = BRStatus.Failed;
            auditInfo.Message = ex.Message;
            //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            return false;
        }
        finally
        {
            ds_IsTokenValid.Dispose();

            Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        }
    }
    public static bool ValidateUser(DataTable dtUserInfo)
    {
        string mySystem = string.Empty;
        string SessionID = string.Empty;
        string strbankID = string.Empty;
        string strBranchID = string.Empty;
        string strUserName = string.Empty;
        string strPassword = string.Empty;
        string IPAddress = string.Empty;
        string EncryptPassword = string.Empty;
        string strLanguageID = string.Empty;
        string strEncSession = string.Empty;
        string imgFile = string.Empty;
        ushort uAccessLevel;
        string RoleID;
        string strNewSession = string.Empty;
        bool bisService = true;
        string message = string.Empty;

        AuditInfo auditInfo = new AuditInfo();
        auditInfo.ModuleID = BRModule.Login;
        auditInfo.EventID = BROperation.LoggedIn;

        try
        {


            System.Guid guid = System.Guid.NewGuid();
            UserInfo usrInfo = new UserInfo();
            foreach (DataRow row in dtUserInfo.Rows)
            {
                SessionID = "";
                strBranchID = BRConfigurationManager.BRAppSettings["defaultBranch"];
                //mySystem = row[1].ToString();
                //strbankID = row[2].ToString();
                strUserName = row[0].ToString();
                strPassword = row[1].ToString();
                //IPAddress = row[5].ToString();
            }
            usrInfo.CType = WebSecurity.EncryptionHelper.Encrypt("~External" + strUserName);
            //ffff
            ASPUtils.SessionID = guid.ToString() + usrInfo.strSystem + usrInfo.strBank;
            usrInfo.strBank = strbankID;
            usrInfo.strBranch = strBranchID;
            usrInfo.strUser = strUserName.ToUpper();
            usrInfo.strSystem = mySystem;
            usrInfo.MachineIP = IPAddress;
            usrInfo.SessionID = guid.ToString() + usrInfo.strSystem + usrInfo.strBank + usrInfo.strUser + usrInfo.MachineIP;
            usrInfo.isService = bisService;

            string strDBServerName = BRConfigurationManager.BRAppSettings["DBServerName"];
            string strDatabaseName = BRConfigurationManager.BRAppSettings["DatabaseName"];
            string strBRUserName = BRConfigurationManager.BRAppSettings["BRUserName"];
            string strBRUserPassword = BRConfigurationManager.BRAppSettings["BRUserPassword"];
            string strBRDBType = BRConfigurationManager.BRAppSettings["BRDBType"];
            string strRoleID = string.Empty;
            string strTokenDuration = BRConfigurationManager.BRAppSettings["TokenDuration"];
            string strFrequency = BRConfigurationManager.BRAppSettings["Frequency"];

            string myIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
            string myHostName = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).HostName.ToString();
            usrInfo.strDBServerName = strDBServerName;
            usrInfo.strDatabaseName = strDatabaseName;
            usrInfo.strBRUserName = strBRUserName;
            usrInfo.strBRUserPassword = strBRUserPassword;
            usrInfo.strBRDBType = strBRDBType;
            usrInfo.strUser = gOperatorID;

            EncryptPassword = BREncryption.EncryptText(ASPUtils.CreateEncryptData(usrInfo, strPassword));


            ValidUser = AuthenticateUser(usrInfo, EncryptPassword, out strLanguageID, out uAccessLevel, out RoleID);


            auditInfo.Status = BRStatus.Successful;
            auditInfo.Message = string.Format("ValidUser: {0}", ValidUser);

            return ValidUser;
        }
        catch (Exception ex)
        {
            auditInfo.Status = BRStatus.Failed;
            auditInfo.Message = string.Format("ValidUser: {0}", ValidUser);
            return false;
        }

        finally
        {
            Task.Run(() => LogEvent(JsonConvert.SerializeObject(auditInfo).ToString()));
        }


    }
    public static UserInfo GetDbParameter()
    {

        UserInfo usrInfo = null;
        string strSystem = ConfigurationManager.AppSettings["DatabaseName"];

        AuditInfo auditInfo = new AuditInfo();
        auditInfo.ModuleID = BRModule.SystemSecurity;
        auditInfo.EventID = BROperation.LoggedIn;

        try
        {
            string strDBServerName = ConfigurationManager.AppSettings["DBServerName"];
            string strDatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
            string strBRUserName = ConfigurationManager.AppSettings["BRUserName"];
            string strBRUserPassword = ConfigurationManager.AppSettings["BRUserPassword"];
            string strBRDBType = ConfigurationManager.AppSettings["BRDBType"];
            bool isService = System.Convert.ToBoolean(ConfigurationManager.AppSettings["isService"]);


            usrInfo = new UserInfo();
            usrInfo.strSystem = strSystem;
            //usrInfo.MachineIP = IPNetworking.GetIP4Address();
            usrInfo.strDBServerName = strDBServerName;
            usrInfo.strDatabaseName = strDatabaseName;
            usrInfo.strBRUserName = strBRUserName;
            usrInfo.strBRUserPassword = strBRUserPassword;
            usrInfo.strBRDBType = strBRDBType;
            usrInfo.isService = isService;
            usrInfo.strUser = gOperatorID;

            auditInfo.Status = BRStatus.Successful;
            auditInfo.Message = "Get DB Parameter";

            return usrInfo;
        }

        catch (Exception ex)
        {
            auditInfo.Status = BRStatus.Failed;
            auditInfo.Message = ex.Message;
            return null;

        }
        finally
        {
            Task.Run(() => LogEvent(auditInfo.ToString()));
        }
    }
    public static UserInfo ValidateRequest(string SessionID, out bool IsValid)
    {
        UserInfo usrinfo = new UserInfo();
        try
        {
            BRDataSet ds_IsTokenValid = new BRDataSet();

            string plaintext = WebSecurity.EncryptionHelper.Decrypt(SessionID);

            string[] System = plaintext.Split(new char[] { '~' });
            string data_text = System[System.Length - 1];
            string mySystem = string.Empty;
            string strUserName = string.Empty;
            string strbankID = string.Empty;
            string strBranchID = string.Empty;
            string IPAddress = string.Empty;
            string tempSessionID = string.Empty;

            IDictionary param = JsonFacade.fromJSON((string)data_text);
            mySystem = param["sys"].ToString();
            strUserName = param["uid"].ToString();
            strBranchID = param["bra"].ToString();


            string strDBServerName = ConfigurationManager.AppSettings["DBServerName"];
            string strDatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
            string strBRUserName = ConfigurationManager.AppSettings["BRUserName"];
            string strBRUserPassword = ConfigurationManager.AppSettings["BRUserPassword"];
            string strBRDBType = ConfigurationManager.AppSettings["BRDBType"];

            usrinfo.strSystem = mySystem;
            usrinfo.strUser = strUserName;
            usrinfo.strDBServerName = strDBServerName;
            usrinfo.strDatabaseName = strDatabaseName;
            usrinfo.strBRUserName = strBRUserName;
            usrinfo.strBRUserPassword = strBRUserPassword;
            usrinfo.strBRDBType = strBRDBType;
            usrinfo.strBank = strbankID;
            usrinfo.strBranch = strBranchID;
            usrinfo.MachineIP = IPAddress;

            tempSessionID = WebSecurity.EncryptionHelper.Encrypt(System[0] + "External" + strUserName + DateTime.Now.ToString());
            IsValid = WebSecurity.EncryptionHelper.DecodeSecurity(tempSessionID);

            if (IsValid && SessionID != null)
            {
                IsValid = ValidateSessionID(usrinfo, out ds_IsTokenValid, new object[] { SessionID, "VALIDATE" });
                if (IsValid
                    && ds_IsTokenValid != null
                    && ds_IsTokenValid.Tables.Count > 0
                    && ds_IsTokenValid.Tables.Contains("dt_WebServiceStatus")
                    && ds_IsTokenValid.Tables["dt_WebServiceStatus"].Rows.Count > 0)
                    usrinfo.strBank = ds_IsTokenValid.Tables["dt_WebServiceStatus"].Rows[0]["BankID"].ToString();

            }

        }
        catch (Exception ex)
        {
            IsValid = false;
        }
        return usrinfo;
    }

    #endregion Session token management


    #region Code Util configuration

    public static string FormatToJson(bool isError, string strRespnse)
    {
        string strResponse = strRespnse;
        bool error = isError;
        strRespnse = JsonConvert.SerializeObject(new { Error = isError, Response = strResponse }, Newtonsoft.Json.Formatting.Indented);

        return strRespnse;
    }

    public static string DtToJSON(DataTable table)
    {
        string JSONString = string.Empty;
        JSONString = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented);
        return JSONString;
    }

    public static DataTable ListToDt<T>(List<T> lst)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);

        //Get all the properties
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in Props)
        {
            //Defining type of data column gives proper data table 
            var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
            //Setting column names as Property names
            dataTable.Columns.Add(prop.Name, type);
        }
        foreach (T item in lst)
        {
            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                //inserting property values to datatable rows
                values[i] = Props[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);
        }
        //put a breakpoint here and check datatable
        return dataTable;
    }

    public static DataTable dtFromObject(object o, string TableName)
    {
        DataTable dt = new DataTable(TableName);

        DataRow dr = dt.NewRow();
        dt.Rows.Add(dr);

        o.GetType().GetProperties().ToList().ForEach(f =>
        {
            try
            {
                f.GetValue(o, null);
                dt.Columns.Add(f.Name, f.PropertyType);
                dt.Rows[0][f.Name] = f.GetValue(o, null);
            }
            catch { }
        });
        return dt;
    }
    
    public static string EnCrypt(string plainText, string passPhrase)
    {
        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return System.Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    public static string DeCrypt(string cipherText, string passPhrase)
    {
        if (!CheckBase64(cipherText))
        {
            return "Invalid Credentials";
        }

        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = System.Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    private static bool CheckBase64(string base64String)
    {
        if (base64String == null || base64String.Length == 0 || base64String.Length % 4 != 0
           || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
            return false;

        try
        {
            System.Convert.FromBase64String(base64String);
            return true;
        }
        catch (Exception exception)
        {
            // Handle the exception
        }
        return false;
    }

    public static string DtToString(DataTable dt)
    {
        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        Dictionary<string, object> row;
        foreach (DataRow dr in dt.Rows)
        {
            row = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                row.Add(col.ColumnName, dr[col]);
            }
            rows.Add(row);
        }
        return serializer.Serialize(rows);
    }


    public static DataSet GenResponse(string msgcode, string Response, string Message)
    {
        DataSet dsResponsex = new DataSet();
        dsResponsex.Tables.Add("status");
        dsResponsex.Tables["status"].Columns.Add("WebServiceStatus");
        dsResponsex.Tables["status"].Columns.Add("Response");
        dsResponsex.Tables["status"].Columns.Add("Message");

        dsResponsex.Tables["status"].Rows.Add("99");
        dsResponsex.Tables["status"].Rows[0]["WebServiceStatus"] = msgcode;
        dsResponsex.Tables["status"].Rows[0]["Response"] = Response;
        dsResponsex.Tables["status"].Rows[0]["Message"] = Message;

        return dsResponsex;
    }

    public static DataSet GenResponse(string msgcode, string Message)
    {
        DataSet dsResponsex = new DataSet();
        dsResponsex.Tables.Add("status");
        dsResponsex.Tables["status"].Columns.Add("WebServiceStatus");
        dsResponsex.Tables["status"].Columns.Add("Message");

        dsResponsex.Tables["status"].Rows.Add("99");
        dsResponsex.Tables["status"].Rows[0]["WebServiceStatus"] = msgcode;
        dsResponsex.Tables["status"].Rows[0]["Message"] = Message;

        return dsResponsex;
    }

    public static string ResponseReturn(string val)
    {
        string str = string.Empty;

        if (string.IsNullOrEmpty(val))
        {
            str = "null";
        }
        else
        {
            str = "\"" + val + "\"";
        }

        return str;
    }

    public static string DataTableToJSONWithJSONNet(DataTable datatable)
    {
        string JSONString = string.Empty;
        JSONString = JsonConvert.SerializeObject(datatable);
        return JSONString;
    }

    private static BRDataSet ConvertToDataSet(object obj)
    {
        BRDataSet dsPassLogInformation = new BRDataSet();

        DataTable dt = new DataTable("dt_PassLogInformation");
        dt.Columns.Add("LogInformation", typeof(string));

        try
        {
            dt.Rows.Add(JsonConvert.SerializeObject(obj));
            dsPassLogInformation.Tables.Add(dt);
            return dsPassLogInformation;

        }
        catch (Exception ex)
        {

            LogException("", "ConvertToDataSet", ex);
            return null;
        }


    }

    public static byte[] CompressData(object obj)
    {
        byte[] inb = SerializeXmlBinary(obj);
        byte[] outb;
        using (MemoryStream ostream = new MemoryStream())
        {
            using (DeflateStream df = new DeflateStream(ostream, System.IO.Compression.CompressionMode.Compress, true))
            {
                df.Write(inb, 0, inb.Length);
            }
            outb = ostream.ToArray();
        }
        return outb;
    }

    public static byte[] SerializeXmlBinary(object obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (XmlDictionaryWriter wtr = XmlDictionaryWriter.CreateBinaryWriter(ms))
            {
                NetDataContractSerializer serializer; serializer = new NetDataContractSerializer();
                serializer.WriteObject(wtr, obj);
                ms.Flush();
            }
            return ms.ToArray();
        }
    }

    public static object DeSerializeXmlBinary(byte[] bytes)
    {
        using (XmlDictionaryReader rdr = XmlDictionaryReader.CreateBinaryReader(bytes, XmlDictionaryReaderQuotas.Max))
        {
            NetDataContractSerializer serializer; serializer = new NetDataContractSerializer();
            serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
            return serializer.ReadObject(rdr);
        }
    }
    #endregion Code Util configuration


    #region audit
    public static bool AddEditAPIAuditLog(UserInfo usrInfo, string MethodName, string Action, string strAddEditAPIAuditLogXML, string UniqueAuditID)
    {
        AuditInfo auditInfo = new AuditInfo();
        auditInfo.EventID = BROperation.Add;
        auditInfo.Status = BRStatus.Successful;

        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        BRDataSet dsAddEditAPIAuditLogXML = new BRDataSet();



        IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);
        arParms[0] = intfDBHelper.CreateNewDBParam("MethodName", SqlDbType.NVarChar);
        arParms[0].Value = MethodName;
        arParms[1] = intfDBHelper.CreateNewDBParam("Action", SqlDbType.NVarChar);
        arParms[1].Value = Action;
        arParms[2] = intfDBHelper.CreateNewDBParam("UpdateCount", SqlDbType.Int);
        arParms[2].Value = 2;
        arParms[3] = intfDBHelper.CreateNewDBParam("AuditID", SqlDbType.NVarChar);
        arParms[3].Value = UniqueAuditID;
        arParms[4] = intfDBHelper.CreateNewDBParam("DetailRecords", SqlDbType.Xml);
        arParms[4].Value = strAddEditAPIAuditLogXML;




        try
        {
            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditAPIAuditLog", dsAddEditAPIAuditLogXML, new string[] { "AddEditAPIAuditLog" }, arParms);
            }

            //if (dsAddEditAPIAuditLogXML.Tables[0].Rows.Count == 0)
            //    return false;
            return true;
        }
        catch (Exception ex)
        {
            auditInfo.Status = BRStatus.Failed;
            auditInfo.Message = ex.Message;
            //BRDALSystemModules.PassAuditInformation(null, usrInfo, "dsAddEditAPIAuditLogXML", string.Empty, auditInfo);
            //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            return true;
        }
        finally
        {
            dsAddEditAPIAuditLogXML.Dispose();

            Task.Run(() => LogEvent(JsonConvert.SerializeObject(auditInfo).ToString()));
        }
    }
    static public void PassLogInformation(UserInfo usrInfo, string SessionID, string AccountID, string MobileNumber, string RequestName, BRDataSet RequestParameter, string ResponseCode, string ResponseMessage)
    {

        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

        IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(9);

        arParms[0] = intfDBHelper.CreateNewDBParam("RequestName", SqlDbType.VarChar, 100);
        arParms[0].Value = RequestName;
        arParms[1] = intfDBHelper.CreateNewDBParam("RequestParameter", SqlDbType.Xml);
        arParms[1].Value = RequestParameter == null ? null : RequestParameter.GetXml();
        arParms[2] = intfDBHelper.CreateNewDBParam("ResponseCode", SqlDbType.VarChar, 10);
        arParms[2].Value = ResponseCode;
        arParms[3] = intfDBHelper.CreateNewDBParam("ResponseMessage", SqlDbType.VarChar, 1000);
        arParms[3].Value = ResponseMessage;
        arParms[4] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
        arParms[4].Value = usrInfo.strUser;
        arParms[5] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
        arParms[5].Value = usrInfo.strBranch;
        arParms[6] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.VarChar, 50);
        arParms[6].Value = AccountID;
        arParms[7] = intfDBHelper.CreateNewDBParam("MobileNumber", SqlDbType.VarChar, 20);
        arParms[7].Value = MobileNumber;
        arParms[8] = intfDBHelper.CreateNewDBParam("SessionID", SqlDbType.NVarChar, 5000);
        arParms[8].Value = CoreOmniApiBase.gSessionID;

        try
        {



            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                using (IDbTransaction transact = connection.BeginTransaction())
                {
                    intfDBHelper.ExecuteScalar(transact, CommandType.StoredProcedure, "p_AddAuditLogInformation", arParms);
                    transact.Commit();
                }
            }


        }


        catch (Exception ex)
        {
            // DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            Task.Run(() => LogException(SessionID, "PassLogInformation", ex));
        }


    }

    public static void LogException(string SessionID, string strEvent, Exception ex)
    {
        try
        {
            DateTime Date = DateTime.Now;
            string ExceptionLogPath = System.AppDomain.CurrentDomain.BaseDirectory;
            ExceptionLogPath = Path.Combine(ExceptionLogPath, "ExceptionLog");
            if (!Directory.Exists(ExceptionLogPath))
                Directory.CreateDirectory(ExceptionLogPath);
            ExceptionLogPath = Path.Combine(ExceptionLogPath, DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt");
            if (ExceptionLogPath != "")
            {
                string AppendErrorMessage = string.Format("Event : {0} \t SessionID: {1} \n Error Message: {2} Date :{3} \n--------------------------\n", strEvent, SessionID, ex.ToString(), DateTime.Now.ToString());
                //string AppendErrorMessage = string.Format("Error Message" + ":" + ex.ToString() + Environment.NewLine + "Date" + ":" + DateTime.Now + Environment.NewLine + "--------------------------" +
                //Environment.NewLine;
                System.IO.File.AppendAllText(ExceptionLogPath, AppendErrorMessage);
            }
        }
        catch (Exception e)
        {
            Task.Run(() => DBClientUtils.GetDBErrorMessages(e, "LogException", "BRCenteService"));
        }
    }

    public static void LogEvent(string Event)
    {
        try
        {
            DateTime Date = DateTime.Now;
            string EventLogPath = System.AppDomain.CurrentDomain.BaseDirectory;
            EventLogPath = Path.Combine(EventLogPath, "EventLog");
            if (!Directory.Exists(EventLogPath))
                Directory.CreateDirectory(EventLogPath);
            EventLogPath = Path.Combine(EventLogPath, DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt");

            if (EventLogPath != "")
            {
                string AppendErrorMessage = Event + Environment.NewLine + "Date" + ":" + DateTime.Now + Environment.NewLine + "--------------------------" +
                    Environment.NewLine;
                System.IO.File.AppendAllText(EventLogPath, AppendErrorMessage);
            }
        }
        catch (Exception ex)
        {
            //Task.Run(() => DBClientUtils.GetDBErrorMessages(ex, "LogException", "BRCenteService"));
        }
    }
    #endregion audit


}

public static class CoreOmniApiBaseExtension
{
    public static List<T> ToListof<T>(this DataTable dt)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        int intvalue = 0;
        decimal decimalValue = 0;
        var columnNames = dt.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();
        var objectProperties = typeof(T).GetProperties(flags);
        var targetList = dt.AsEnumerable().Select(dataRow =>
        {
            var instanceOfT = Activator.CreateInstance<T>();

            foreach (var properties in objectProperties.Where(properties => columnNames.Contains(properties.Name) && dataRow[properties.Name] != DBNull.Value))
            {
                var propertiesType = properties.PropertyType.Name;
                //var propertiesvalue = dataRow[properties.Name];

                switch (propertiesType)
                {
                    case "Int32":
                        int.TryParse(dataRow[properties.Name].ToString(), out intvalue);
                        properties.SetValue(instanceOfT, intvalue, null);
                        break;

                    case "Decimal":
                        decimal.TryParse(dataRow[properties.Name].ToString(), out decimalValue);
                        properties.SetValue(instanceOfT, decimalValue, null);
                        break;
                        
                    default:
                        properties.SetValue(instanceOfT, dataRow[properties.Name], null);
                        break;
                }
                

                
            }
            return instanceOfT;
        }).ToList();

        return targetList;
    }
}