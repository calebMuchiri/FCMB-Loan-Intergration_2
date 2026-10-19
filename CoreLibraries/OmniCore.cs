using BR.ApplicationBlocks.Data;
using BR.Configuration;
using BR.DBClient;
using BRBase;
using BRControls;
using BRDALLibrary;
using BREntities.CurrencyDenominations;
using BREntities.GroupLoanDisbursement;
using BREntities.LoanCalulator;
using BREntities.LoanDisbSchedules;
using BREntities.LoanInitiationDetail;
using BREntities.LoanIntCalcRule;
using BREntities.LoanPreClosureDetail;
using BREntities.Loans;
using BREntities.ProductLoan;
using BREntities.StandingInstructions;
using BREntities.Transactions;
using BREntities.WFLoanApplications;
using BREntities.WFLoanAppraisal;
using BREntities.WFLoanAppraisals;
using Microsoft.JScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniApi.Models.Cards;
using OmniApi.Models.FCMBLoan;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace OmniApi.CoreLibraries
{
    public class OmniCore
    {
        #region FCMBLoans
        public static async Task<List<DisbursementTransferRequest>> LoanDisbursementTransferAsync(UserInfo usrInfo)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var debitAccountNo = ConfigurationManager.AppSettings["DebitAccountNo"];
            var chargeAccountNo = ConfigurationManager.AppSettings["ChargeAccountNo"]; 

            CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] START | RequestId:{requestId} | User:{usrInfo?.strUser} | Branch:{usrInfo?.strBranch} | Timestamp:{startTime:O}");

            BRDataSet dsLoanDisbursements = null;
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

            try
            {
                dsLoanDisbursements = new BRDataSet();
                List<DisbursementTransferRequest> disbursementList;

                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] RequestId:{requestId} - Calling stored procedure p_GetPendingLoanApplications");

                    intfDBHelper.FillDataset(
                        connection,
                        CommandType.StoredProcedure,
                        "p_GetPendingLoanApplications",
                        dsLoanDisbursements,
                        new string[] { "dt_SoapRequestData" });

                    var rowCount = dsLoanDisbursements?.Tables.Count > 0 ? dsLoanDisbursements.Tables[0].Rows.Count : 0;
                    CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] RequestId:{requestId} - Retrieved {rowCount} rows from DB");

                    disbursementList = (dsLoanDisbursements.Tables.Count > 0)
                        ? dsLoanDisbursements.Tables[0]
                            .AsEnumerable()
                            .Select(row => new DisbursementTransferRequest
                            {
                                //DebitAccountNo = row["AccountID"].ToString(),
                                DebitAccountNo = debitAccountNo, // Use fixed debit account from config
                                CreditAccountNo = row["CreditAccountID"].ToString(),
                                IsFees = System.Convert.ToBoolean(row["IsFees"]),
                                Charges = new List<Charges>
                                {
                            new Charges
                            {
                                FeeNarration = "Loan Transfer Request",
                                //Account = row["CreditAccountID"].ToString(),
                                Account = chargeAccountNo,
                                Fee = System.Convert.ToDouble(row["Amount"]) * 0.10 // Example: 10% fee, adjust as needed
                            }
                                },
                                Amount = System.Convert.ToDouble(row["Amount"]),
                                Currency = "NGN",
                                Narration = row["Remarks"].ToString(),
                                Remark = row["Remarks"].ToString(),
                                CustomerReference = row["ApplicationID"].ToString(),
                                location = new Location { Latitude = 0, Longitude = 0 }
                            })
                            .ToList()
                        : new List<DisbursementTransferRequest>();

                    CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] RequestId:{requestId} - Mapped {disbursementList.Count} transfer requests");
                }

                stopwatch.Stop();
                CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] END | RequestId:{requestId} | Success | ElapsedMs:{stopwatch.ElapsedMilliseconds}");
                return await Task.FromResult(disbursementList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] RequestId:{requestId} - EXCEPTION after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}\n{ex.StackTrace}");
                // preserve original behavior of throwing but give good logs — rethrow
                throw;
            }
            finally
            {
                // make sure dataset disposed by caller logic if necessary; log final timestamp
                CoreOmniApiBase.LogEvent($"[LoanDisbursementTransferAsync] RequestId:{requestId} - Finalizing. Start:{startTime:O} | End:{DateTime.UtcNow:O}");
            }
        }

        public static async Task<AccountStatusResponse> CheckAccountStatus(AccountStatus account)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();

            if (account == null)
            {
                CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Invalid input: account is null");
                return new AccountStatusResponse();
            }

            var baseUrl = ConfigurationManager.AppSettings["AccountEnquiry"];
            var endpoint = "AccountStatusV1";
            var queryParam = string.Format("?accountNumber={0}", Uri.EscapeDataString(account.AccountID));
            var clientId = ConfigurationManager.AppSettings["ClientID"];
            var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];

            var password = ConfigurationManager.AppSettings["Password"];

            var utcdate = DateTime.UtcNow;

            var token = GetXTokenHeader(utcdate, clientId, password);

            var requestUrl = string.Format("{0}/{1}/{2}", baseUrl, endpoint, queryParam);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            CoreOmniApiBase.LogEvent($"[CheckAccountStatus] START | RequestId:{requestId} | Timestamp:{startTime:O} | RequestUrl:{requestUrl} | AccountID:{account.AccountID}");

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Add("Accept", "*/*");
                    request.Headers.Add("Cache-Control", "no-cache");
                    request.Headers.Add("UTCTimestamp", utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    request.Headers.Add("x-token", token);
                    if (!string.IsNullOrEmpty(clientId)) request.Headers.Add("client_id", clientId);
                    if (!string.IsNullOrEmpty(subscriptionKey)) request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
                        ? "null"
                        : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");
                    CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Headers: client_id={clientId}, Ocp-Apim-Subscription-Key={maskedSubKey}");

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.SendAsync(request).ConfigureAwait(false);
                    }
                    catch (Exception httpEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - HTTP send failed after {stopwatch.ElapsedMilliseconds}ms. Error: {httpEx.Message}\n{httpEx.StackTrace}");
                        return new AccountStatusResponse();
                    }

                    string responseBody = null;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Failed reading response content after {stopwatch.ElapsedMilliseconds}ms. Error: {readEx.Message}\n{readEx.StackTrace}");
                        return new AccountStatusResponse();
                    }

                    stopwatch.Stop();
                    CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. StatusCode={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

                    if (!response.IsSuccessStatusCode)
                    {
                        CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Non-success status code. Content preview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
                        return new AccountStatusResponse();
                    }

                    try
                    {
                        var result = JsonConvert.DeserializeObject<AccountStatusResponse>(responseBody);

                        // Log a short summary of the parsed response for easier troubleshooting
                        var acctStatus = result?.data?.AccountStatus ?? "<null>";
                        var foracid = result?.data?.Foracid ?? "<null>";
                        CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Deserialized response. AccountStatus: {acctStatus}, Foracid: {foracid}");

                        return result ?? new AccountStatusResponse();
                    }
                    catch (Exception deserEx)
                    {
                        CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(500, responseBody.Length)) : "null")}");
                        return new AccountStatusResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"[CheckAccountStatus] RequestId:{requestId} - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                return new AccountStatusResponse();
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"[CheckAccountStatus] END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
            }
        }

        public static async Task<NameEnquiryResponse> GetClientNameAsync(AccountStatus account)
        {
            DateTime startTime = DateTime.UtcNow;
            string requestId = Guid.NewGuid().ToString();

            if (account == null)
            {
                CoreOmniApiBase.LogEvent(string.Format("[GetClientNameAsync] RequestId:{0} - Account is null", requestId));
                return new NameEnquiryResponse();
            }

            string baseUrl = ConfigurationManager.AppSettings["AccountEnquiry"];
            string endpoint = "nameEnquiry";
            string queryParam = "?accountNumber=" + Uri.EscapeDataString(account.AccountID);

            string clientId = ConfigurationManager.AppSettings["ClientID"];
            string subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];
            string password = ConfigurationManager.AppSettings["Password"];

            DateTime utcdate = DateTime.UtcNow;
            string timeStamp = utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            string token = GetXTokenHeader(utcdate, clientId, password);

            string requestUrl = baseUrl + "/" + endpoint + queryParam;

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            CoreOmniApiBase.LogEvent(string.Format(
                "[GetClientNameAsync] START | RequestId:{0} | Url:{1} | AccountID:{2}",
                requestId, requestUrl, account.AccountID));

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
                    {
                        request.Headers.Add("Accept", "*/*");
                        request.Headers.Add("Cache-Control", "no-cache");
                        request.Headers.Add("UTCTimestamp", timeStamp);
                        request.Headers.Add("x-token", token);

                        if (!string.IsNullOrEmpty(clientId))
                            request.Headers.Add("client_ID", clientId);

                        if (!string.IsNullOrEmpty(subscriptionKey))
                            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                        HttpResponseMessage response = null;

                        try
                        {
                            response = await client.SendAsync(request);
                        }
                        catch (Exception httpEx)
                        {
                            stopwatch.Stop();

                            CoreOmniApiBase.LogEvent(string.Format(
                                "[GetClientNameAsync] RequestId:{0} - HTTP send failed after {1}ms\n{2}",
                                requestId,
                                stopwatch.ElapsedMilliseconds,
                                httpEx.ToString()));

                            return new NameEnquiryResponse();
                        }

                        string responseBody = "";

                        try
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                        }
                        catch (Exception readEx)
                        {
                            stopwatch.Stop();

                            CoreOmniApiBase.LogEvent(string.Format(
                                "[GetClientNameAsync] RequestId:{0} - Failed reading response after {1}ms\n{2}",
                                requestId,
                                stopwatch.ElapsedMilliseconds,
                                readEx.ToString()));

                            return new NameEnquiryResponse();
                        }

                        stopwatch.Stop();

                        CoreOmniApiBase.LogEvent(string.Format(
                            "[GetClientNameAsync] RequestId:{0} - Response received in {1}ms | StatusCode:{2}",
                            requestId,
                            stopwatch.ElapsedMilliseconds,
                            response.StatusCode));

                        if (!response.IsSuccessStatusCode)
                        {
                            CoreOmniApiBase.LogEvent(string.Format(
                                "[GetClientNameAsync] RequestId:{0} - HTTP Error {1}\nResponse:{2}",
                                requestId,
                                response.StatusCode,
                                responseBody));

                            return new NameEnquiryResponse();
                        }

                        try
                        {
                            NameEnquiryResponse result =
                                JsonConvert.DeserializeObject<NameEnquiryResponse>(responseBody);

                            if (result != null && result.data != null)
                            {
                                CoreOmniApiBase.LogEvent(string.Format(
                                    "[GetClientNameAsync] RequestId:{0} - Account Name:{1}",
                                    requestId,
                                    result.data.Acct_Name));
                            }

                            return result ?? new NameEnquiryResponse();
                        }
                        catch (Exception deserEx)
                        {
                            CoreOmniApiBase.LogEvent(string.Format(
                                "[GetClientNameAsync] RequestId:{0} - Deserialization failed\n{1}\nResponse:{2}",
                                requestId,
                                deserEx.ToString(),
                                responseBody));

                            return new NameEnquiryResponse();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent(string.Format(
                    "[GetClientNameAsync] RequestId:{0} - UNHANDLED ERROR\n{1}",
                    requestId,
                    ex.ToString()));

                return new NameEnquiryResponse();
            }
            finally
            {
                DateTime endTime = DateTime.UtcNow;

                CoreOmniApiBase.LogEvent(string.Format(
                    "[GetClientNameAsync] END | RequestId:{0} | Start:{1:O} | End:{2:O}",
                    requestId,
                    startTime,
                    endTime));
            }
        }

        //public static async Task<NameEnquiryResponse> GetClientNameAsync(AccountStatus account)
        //{
        //    var startTime = DateTime.UtcNow;
        //    var requestId = Guid.NewGuid().ToString();

        //    if (account == null)
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Invalid input: account is null");
        //        return new NameEnquiryResponse();
        //    }

        //    var baseUrl = ConfigurationManager.AppSettings["AccountEnquiry"];
        //    var endpoint = "nameEnquiry";
        //    var queryParam = string.Format("?accountNumber={0}", Uri.EscapeDataString(account.AccountID));
        //    var clientId = ConfigurationManager.AppSettings["ClientID"];
        //    var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];
        //    var password = ConfigurationManager.AppSettings["Password"];

        //    // Generate UTC timestamp ONCE - keep the original milliseconds
        //    var utcdate = DateTime.UtcNow;

        //    var timeStamp = utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff");



        //    // Generate token using the stripped timestamp
        //    var token = GetXTokenHeader(utcdate, clientId, password);

        //    var requestUrl = string.Format("{0}/{1}{2}", baseUrl, endpoint, queryParam);

        //    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        //    CoreOmniApiBase.LogEvent($"[GetClientNameAsync] START | RequestId:{requestId} | RequestUrl:{requestUrl} | AccountID:{account.AccountID}");

        //    // Log what we're using
        //    CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} | Timestamp : {timeStamp} |  Token:{token}");

        //    try
        //    {
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        using (var client = new HttpClient())
        //        using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
        //        {
        //            request.Headers.Accept.Clear();
        //            request.Headers.Add("Accept", "*/*");
        //            request.Headers.Add("Cache-Control", "no-cache");
        //            request.Headers.Add("UTCTimestamp", timeStamp); // Use the formatted timestamp WITH milliseconds
        //            request.Headers.Add("x-token", token);

        //            if (!string.IsNullOrEmpty(clientId)) request.Headers.Add("client_ID", clientId);
        //            if (!string.IsNullOrEmpty(subscriptionKey)) request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        //            var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
        //                ? "null"
        //                : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Headers: client_id={clientId}, Ocp-Apim-Subscription-Key={maskedSubKey}");



        //            HttpResponseMessage response;
        //            try
        //            {
        //                // Log TLS protocol and resolved host for server-side diagnostics
        //                CoreOmniApiBase.LogEvent(
        //                    $"[GetClientNameAsync] RequestId:{requestId} - SecurityProtocol: {ServicePointManager.SecurityProtocol} | TargetHost: {new Uri(requestUrl).Host}");
        //                response = await client.SendAsync(request).ConfigureAwait(false);
        //            }
        //            catch (Exception httpEx)
        //            {
        //                stopwatch.Stop();

        //                // Build full exception chain to expose the root cause
        //                var exceptionChain = new System.Text.StringBuilder();
        //                var current = httpEx;
        //                int depth = 0;
        //                while (current != null)
        //                {
        //                    exceptionChain.AppendLine(string.Format("[Level {0}] {1}: {2}", depth, current.GetType().FullName, current.Message));
        //                    var webEx = current as System.Net.WebException;
        //                    if (webEx != null)
        //                    {
        //                        exceptionChain.AppendLine(string.Format("  WebExceptionStatus: {0}", webEx.Status));
        //                        var httpWebResp = webEx.Response as System.Net.HttpWebResponse;
        //                        if (httpWebResp != null)
        //                            exceptionChain.AppendLine(string.Format("  HttpStatusCode: {0} ({1})", (int)httpWebResp.StatusCode, httpWebResp.StatusCode));
        //                    }
        //                    exceptionChain.AppendLine(string.Format("  StackTrace: {0}", current.StackTrace));
        //                    current = current.InnerException;
        //                    depth++;
        //                }

        //                CoreOmniApiBase.LogEvent(
        //                    string.Format("[GetClientNameAsync] RequestId:{0} - HTTP send failed after {1}ms.\nRequestUrl: {2}\nExceptionChain:\n{3}",
        //                        requestId, stopwatch.ElapsedMilliseconds, requestUrl, exceptionChain));

        //                return new NameEnquiryResponse();
        //            }
        //            string responseBody = null;
        //            try
        //            {
        //                responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            }
        //            catch (Exception readEx)
        //            {
        //                stopwatch.Stop();
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Failed reading response content after {stopwatch.ElapsedMilliseconds}ms. Error: {readEx.Message}\n{readEx.StackTrace}");
        //                return new NameEnquiryResponse();
        //            }

        //            stopwatch.Stop();
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. StatusCode={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Non-success status code. Content preview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
        //                return new NameEnquiryResponse();
        //            }

        //            try
        //            {
        //                var result = JsonConvert.DeserializeObject<NameEnquiryResponse>(responseBody);

        //                var acctName = result?.data?.Acct_Name ?? "<null>";
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Deserialized response. Acct_Name: {acctName}");

        //                return result ?? new NameEnquiryResponse();
        //            }
        //            catch (Exception deserEx)
        //            {
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(500, responseBody.Length)) : "null")}");
        //                return new NameEnquiryResponse();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        //        return new NameEnquiryResponse();
        //    }
        //    finally
        //    {
        //        var endTime = DateTime.UtcNow;
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
        //    }
        //}


        //public static async Task<NameEnquiryResponse> GetClientNameAsync(AccountStatus account)
        //{
        //    var startTime = DateTime.UtcNow;
        //    var requestId = Guid.NewGuid().ToString();

        //    if (account == null)
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Invalid input: account is null");
        //        return new NameEnquiryResponse();
        //    }

        //    var baseUrl = ConfigurationManager.AppSettings["AccountEnquiry"];
        //    var endpoint = "nameEnquiry";
        //    var queryParam = string.Format("?accountNumber={0}", Uri.EscapeDataString(account.AccountID));
        //    var clientId = ConfigurationManager.AppSettings["ClientID"];
        //    var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];

        //    // READ PRE-GENERATED VALUES FROM CONFIG
        //    var preGeneratedToken = ConfigurationManager.AppSettings["PreGeneratedToken"];
        //    var preGeneratedTimestamp = ConfigurationManager.AppSettings["PreGeneratedTimestamp"];
        //    var tokenGeneratedAtStr = ConfigurationManager.AppSettings["TokenGeneratedAt"];

        //    // Validate that pre-generated values exist and are not expired
        //    if (string.IsNullOrEmpty(preGeneratedToken) ||
        //        string.IsNullOrEmpty(preGeneratedTimestamp) ||
        //        string.IsNullOrEmpty(tokenGeneratedAtStr))
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - ERROR: Pre-generated credentials not found in config. Please update Web.config with fresh values from external tool.");
        //        return new NameEnquiryResponse();
        //    }

        //    // Check if token has expired (default: 60 seconds)
        //    DateTime tokenGeneratedAt;
        //    if (!DateTime.TryParse(tokenGeneratedAtStr, out tokenGeneratedAt))
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - ERROR: Invalid TokenGeneratedAt format in config.");
        //        return new NameEnquiryResponse();
        //    }

        //    var tokenValiditySeconds = int.Parse(ConfigurationManager.AppSettings["TokenValiditySeconds"] ?? "60");
        //    var tokenAge = (DateTime.UtcNow - tokenGeneratedAt).TotalSeconds;

        //    //if (tokenAge > tokenValiditySeconds)
        //    //{
        //    //    CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - WARNING: Pre-generated token expired. Age: {tokenAge}s, Validity: {tokenValiditySeconds}s. Please refresh credentials in Web.config.");
        //    //    // You can either return error or continue with expired token (will likely fail)
        //    //    return new NameEnquiryResponse();
        //    //}

        //    var requestUrl = string.Format("{0}/{1}{2}", baseUrl, endpoint, queryParam);
        //    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        //    CoreOmniApiBase.LogEvent($"[GetClientNameAsync] START | RequestId:{requestId} | Timestamp:{startTime:O} | RequestUrl:{requestUrl} | AccountID:{account.AccountID}");
        //    CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} | Using PRE-GENERATED credentials | Token:{preGeneratedToken.Substring(0, Math.Min(20, preGeneratedToken.Length))}... | Timestamp:{preGeneratedTimestamp} | TokenAge:{tokenAge:F2}s");

        //    try
        //    {
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        using (var client = new HttpClient())
        //        using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
        //        {
        //            request.Headers.Accept.Clear();
        //            request.Headers.Add("Accept", "*/*");
        //            request.Headers.Add("Cache-Control", "no-cache");
        //            request.Headers.Add("UTCTimestamp", preGeneratedTimestamp); // Use pre-generated timestamp
        //            request.Headers.Add("x-token", preGeneratedToken);           // Use pre-generated token

        //            if (!string.IsNullOrEmpty(clientId)) request.Headers.Add("client_ID", clientId);
        //            if (!string.IsNullOrEmpty(subscriptionKey)) request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        //            var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
        //                ? "null"
        //                : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Headers: client_id={clientId}, Ocp-Apim-Subscription-Key={maskedSubKey}");

        //            HttpResponseMessage response;
        //            try
        //            {
        //                response = await client.SendAsync(request).ConfigureAwait(false);
        //            }
        //            catch (Exception httpEx)
        //            {
        //                stopwatch.Stop();
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - HTTP send failed after {stopwatch.ElapsedMilliseconds}ms. Error: {httpEx.Message}\n{httpEx.StackTrace}");
        //                return new NameEnquiryResponse();
        //            }

        //            string responseBody = null;
        //            try
        //            {
        //                responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            }
        //            catch (Exception readEx)
        //            {
        //                stopwatch.Stop();
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Failed reading response content after {stopwatch.ElapsedMilliseconds}ms. Error: {readEx.Message}\n{readEx.StackTrace}");
        //                return new NameEnquiryResponse();
        //            }

        //            stopwatch.Stop();
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. StatusCode={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Non-success status code. Content preview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
        //                return new NameEnquiryResponse();
        //            }

        //            try
        //            {
        //                var result = JsonConvert.DeserializeObject<NameEnquiryResponse>(responseBody);
        //                var acctName = result?.data?.Acct_Name ?? "<null>";
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - SUCCESS! Deserialized response. Acct_Name: {acctName}");
        //                return result ?? new NameEnquiryResponse();
        //            }
        //            catch (Exception deserEx)
        //            {
        //                CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(500, responseBody.Length)) : "null")}");
        //                return new NameEnquiryResponse();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        //        return new NameEnquiryResponse();
        //    }
        //    finally
        //    {
        //        var endTime = DateTime.UtcNow;
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
        //    }
        //}

        //public static async Task<NameEnquiryResponse> GetClientNameAsync(AccountStatus account)
        //{
        //    if (account == null)
        //    {
        //        var requestId = Guid.NewGuid().ToString();
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - Invalid input: account is null");
        //        return new NameEnquiryResponse();
        //    }

        //    var baseUrl = ConfigurationManager.AppSettings["AccountEnquiry"];
        //    var endpoint = "nameEnquiry";
        //    var queryParam = string.Format("?accountNumber={0}", Uri.EscapeDataString(account.AccountID));
        //    var clientId = ConfigurationManager.AppSettings["ClientID"];
        //    var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];
        //    var password = ConfigurationManager.AppSettings["Password"];

        //    var requestUrl = string.Format("{0}/{1}{2}", baseUrl, endpoint, queryParam);

        //    try
        //    {
        //        var httpCall = new HttpCall();

        //        // Call HttpCall.GetDataAsync with custom header for "client_ID"
        //        var response = await httpCall.GetDataAsync(
        //            requestUrl,
        //            clientId,
        //            password,
        //            subscriptionKey,
        //            request => request.Headers.Add("client_ID", clientId) // Override with correct header name
        //        ).ConfigureAwait(false);

        //        if (!response.Success)
        //        {
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{response.RequestId} - Failed: {response.ErrorMessage}");
        //            return new NameEnquiryResponse();
        //        }

        //        try
        //        {
        //            var result = JsonConvert.DeserializeObject<NameEnquiryResponse>(response.Content);
        //            var acctName = result?.data?.Acct_Name ?? "<null>";
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{response.RequestId} - Deserialized response. Acct_Name: {acctName}");
        //            return result ?? new NameEnquiryResponse();
        //        }
        //        catch (Exception deserEx)
        //        {
        //            var preview = response.Content != null && response.Content.Length > 500
        //                ? response.Content.Substring(0, 500) + "..."
        //                : response.Content ?? "null";
        //            CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{response.RequestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview: {preview}");
        //            return new NameEnquiryResponse();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var requestId = Guid.NewGuid().ToString();
        //        CoreOmniApiBase.LogEvent($"[GetClientNameAsync] RequestId:{requestId} - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        //        return new NameEnquiryResponse();
        //    }
        //}


        // Keep the existing method for backwards compatibility
        private static string GetXTokenHeader(DateTime utcdate, string ClientID, string Password)
        {
            var date = utcdate.ToString("yyyy-MM-ddHHmmss");
            var data = date + ClientID + Password;
            return SHA512(data);
        }

        public static async Task<BaseRetailAccountOpeningResponse> CreateBaseRetailAccountAsync(BaseRetailAccountOpeningRequest request)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();


            if (request == null)
            {
                CoreOmniApiBase.LogEvent($"[CreateBaseRetailAccountAsync] RequestId:{requestId} - Invalid input: request is null");
                return null;
            }

            var baseUrl = ConfigurationManager.AppSettings["FCMBBaseRetailAccountURL"];
            var endpoint = "CreateBaseRetailAccount"; // Adjust if needed
            var requestUrl = $"{baseUrl}/{endpoint}";

            var clientId = ConfigurationManager.AppSettings["ClientID"];
            var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];

            CoreOmniApiBase.LogEvent(
                $"[CreateBaseRetailAccountAsync] START | RequestId:{requestId} | URL:{requestUrl} | Payload:{JsonConvert.SerializeObject(request)}");

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                using (var httpReq = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                {
                    httpReq.Headers.Accept.Clear();
                    httpReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrEmpty(clientId))
                        httpReq.Headers.Add("client_id", clientId);

                    if (!string.IsNullOrEmpty(subscriptionKey))
                        httpReq.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    string jsonPayload = JsonConvert.SerializeObject(request);
                    httpReq.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    CoreOmniApiBase.LogEvent($"[CreateBaseRetailAccountAsync] RequestId:{requestId} - Sending request");

                    var response = await client.SendAsync(httpReq).ConfigureAwait(false);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    CoreOmniApiBase.LogEvent(
                        $"[CreateBaseRetailAccountAsync] RequestId:{requestId} - Response({(int)response.StatusCode}): {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log error response
                        return new BaseRetailAccountOpeningResponse
                        {
                            description = "3rd Party returned an error",
                            code = response.StatusCode.ToString(),
                            data = null
                        };
                    }

                    // Deserialize success response
                    var result = JsonConvert.DeserializeObject<BaseRetailAccountOpeningResponse>(responseContent);
                    return result;
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"[CreateBaseRetailAccountAsync] ERROR RequestId:{requestId} - {ex.Message}\n{ex.StackTrace}");
                return null;
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"[CreateBaseRetailAccountAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
            }
        }

        public static async Task<List<AddLienRequest>> GetLoanDueAmountAsync(UserInfo usrInfo)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] START | RequestId:{requestId} | User:{usrInfo?.strUser} | Branch:{usrInfo?.strBranch} | Timestamp:{startTime:O}");

            BRDataSet dsLoanDueAmount = null;
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

            try
            {
                dsLoanDueAmount = new BRDataSet();
                List<AddLienRequest> dueLoanList;

                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] RequestId:{requestId} - Calling stored procedure p_GetLOPDueAmountRequest");

                    intfDBHelper.FillDataset(
                        connection,
                        CommandType.StoredProcedure,
                        "p_GetLOPDueAmountRequest",
                        dsLoanDueAmount,
                        new string[] { "dt_SoapRequestData" });

                    var rowCount = dsLoanDueAmount?.Tables.Count > 0 ? dsLoanDueAmount.Tables[0].Rows.Count : 0;
                    CoreOmniApiBase.LogEvent($"[p_GetLOPDueAmountRequest] RequestId:{requestId} - Retrieved {rowCount} rows from DB");

                    dueLoanList = (dsLoanDueAmount.Tables.Count > 0)
                        ? dsLoanDueAmount.Tables[0]
                            .AsEnumerable()
                            .Select(row => new AddLienRequest
                            {
                                RowID = System.Convert.ToInt32(row["RowID"]),
                                requestID = Guid.NewGuid().ToString(),
                                accountNo = row["AccountNo"].ToString(),
                                amountValue = System.Convert.ToDecimal(row["AmountValue"]),
                                currencyCode = row["CurrencyCode"].ToString(),
                                rmks = row["Rmks"].ToString(),
                                reasonCode = "040",
                                StartDT = System.Convert.ToDateTime(row["StartDt"]),
                                EndDT = System.Convert.ToDateTime(row["EndDt"])
                            })
                            .ToList()
                        : new List<AddLienRequest>();

                    CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] RequestId:{requestId} - Mapped {dueLoanList.Count} transfer requests");
                }

                stopwatch.Stop();
                CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] END | RequestId:{requestId} | Success | ElapsedMs:{stopwatch.ElapsedMilliseconds}");
                return await Task.FromResult(dueLoanList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] RequestId:{requestId} - EXCEPTION after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}\n{ex.StackTrace}");
                // preserve original behavior of throwing but give good logs — rethrow
                throw;
            }
            finally
            {
                // make sure dataset disposed by caller logic if necessary; log final timestamp
                CoreOmniApiBase.LogEvent($"[GetLoanDueAmountAsync] RequestId:{requestId} - Finalizing. Start:{startTime:O} | End:{DateTime.UtcNow:O}");
            }
        }

        public static async Task<AddLienResponse> AddLienAsync(AddLienRequest request)
        {
            var requestId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            if (request == null)
            {
                CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Invalid input: null request");
                return new AddLienResponse();
            }

            var baseUrl = ConfigurationManager.AppSettings["FCMBAddLienURL"];
            var endpoint = "AddLien";
            var requestUrl = $"{baseUrl}/{endpoint}";

            var clientId = ConfigurationManager.AppSettings["ClientID"];
            var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];
            var password = ConfigurationManager.AppSettings["Password"];

            var utcdate = DateTime.UtcNow;
            var token = GetXTokenHeader(utcdate, clientId, password);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            CoreOmniApiBase.LogEvent(
                $"[AddLienAsync] START | RequestId:{requestId} | Timestamp:{startTime:O} | URL:{requestUrl} | Payload:{JsonConvert.SerializeObject(request)}");

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                using (var req = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                {
                    req.Headers.Accept.Clear();
                    req.Headers.Add("Accept", "*/*");
                    req.Headers.Add("Cache-Control", "no-cache");
                    req.Headers.Add("UTCTimestamp", utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    req.Headers.Add("x-token", token);

                    if (!string.IsNullOrEmpty(clientId))
                        req.Headers.Add("client_id", clientId);

                    if (!string.IsNullOrEmpty(subscriptionKey))
                        req.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
                        ? "null"
                        : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");

                    CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Headers: client_id={clientId}, Ocp-Apim-Subscription-Key={maskedSubKey}");

                    string json = JsonConvert.SerializeObject(request);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.SendAsync(req).ConfigureAwait(false);
                    }
                    catch (Exception httpEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - HTTP send failed after {stopwatch.ElapsedMilliseconds}ms. Error: {httpEx.Message}\n{httpEx.StackTrace}");
                        return new AddLienResponse();
                    }

                    string responseBody = null;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Failed reading response content after {stopwatch.ElapsedMilliseconds}ms. Error: {readEx.Message}\n{readEx.StackTrace}");
                        return new AddLienResponse();
                    }

                    stopwatch.Stop();
                    CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. StatusCode={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

                    if (!response.IsSuccessStatusCode)
                    {
                        CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Non-success status code. Content preview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
                        var result = JsonConvert.DeserializeObject<AddLienResponse>(responseBody);
                        return result ?? new AddLienResponse();
                    }

                    try
                    {
                        var result = JsonConvert.DeserializeObject<AddLienResponse>(responseBody);

                        var lienId = result?.data?.lienId ?? "<null>";
                        var accountId = result?.data?.accountId ?? "<null>";

                        CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Deserialized response. LienId: {lienId}, AccountId: {accountId}");

                        //Update the database with the response data if needed


                        return result ?? new AddLienResponse();
                    }
                    catch (Exception deserEx)
                    {
                        CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(500, responseBody.Length)) : "null")}");
                        return new AddLienResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"[AddLienAsync] RequestId:{requestId} - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                return new AddLienResponse();
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"[AddLienAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
            }
        }



        public static bool UpdateAddLienRequest(UserInfo userInfo, int RowID, string jsonResponse)
        {
            AuditInfo auditInfo = new AuditInfo
            {
                ModuleID = BRModule.Account,
                EventID = BROperation.Add,
                Status = BRStatus.Successful
            };
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(userInfo))
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "p_UpdateLOPDueAmountRequest";
                    command.CommandType = CommandType.StoredProcedure;

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@RowID";
                    param1.Value = RowID;
                    command.Parameters.Add(param1);

                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@JsonData";
                    param2.Value = jsonResponse;
                    command.Parameters.Add(param2);

                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Message = ex.Message;
                auditInfo.Status = BRStatus.Failed;
                return false;
            }
        }



        //public static async Task<DisbursementTransferResponse> ProcessTransferRequestsAsync(DisbursementTransferRequest disbursementTransferRequest)
        //{
        //    var startTime = DateTime.UtcNow;
        //    var requestId = Guid.NewGuid().ToString();
        //    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        //    // CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] START | RequestId:{requestId} | Timestamp:{startTime:O} | PayloadPreview:" +
        //    //$" {JsonConvert.SerializeObject(new { 
        //    // disbursementTransferRequest?.Amount, disbursementTransferRequest?.Currency, disbursementTransferRequest?.CustomerReference })}");

        //    // Log full payload before sending
        //    var fullPayload = JsonConvert.SerializeObject(disbursementTransferRequest, Formatting.Indented);
        //    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] START | RequestId:{requestId} | Timestamp:{startTime:O} | FullPayload:\n{fullPayload}");

        //    var baseUrl = ConfigurationManager.AppSettings["LoanTransferUrl"];
        //    var endpoint = "doTransfer";
        //    var requestUrl = string.Format("{0}/{1}", baseUrl, endpoint);

        //    var clientId = ConfigurationManager.AppSettings["ClientID"];
        //    var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];

        //    var password = ConfigurationManager.AppSettings["Password"];

        //    var utcdate = DateTime.UtcNow;

        //    var token = GetXTokenHeader(utcdate, clientId, password);

        //    try
        //    {
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        using (var client = new HttpClient())
        //        {
        //            var requestBody = JsonConvert.SerializeObject(disbursementTransferRequest, Formatting.Indented);
        //            using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
        //            {
        //                // Prepare headers
        //                client.DefaultRequestHeaders.Accept.Clear();
        //                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
        //                client.DefaultRequestHeaders.Add("UTCTimestamp", utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
        //                client.DefaultRequestHeaders.Add("x-token", token);
        //                if (!string.IsNullOrEmpty(clientId)) client.DefaultRequestHeaders.Remove("client_id");
        //                if (!string.IsNullOrEmpty(clientId)) client.DefaultRequestHeaders.Add("client_id", clientId);
        //                if (!string.IsNullOrEmpty(subscriptionKey)) client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
        //                if (!string.IsNullOrEmpty(subscriptionKey)) client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        //                var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
        //                    ? "null"
        //                    : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");

        //                CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Sending POST {requestUrl} | client_id={clientId} | Ocp-Apim-Subscription-Key={maskedSubKey} | BodyLength:{requestBody?.Length ?? 0}");

        //                HttpResponseMessage response;
        //                string responseBody = null;
        //                try
        //                {
        //                    response = await client.PostAsync(requestUrl, content).ConfigureAwait(false);
        //                }
        //                catch (Exception httpEx)
        //                {
        //                    stopwatch.Stop();
        //                    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - HTTP request failed after {stopwatch.ElapsedMilliseconds}ms: {httpEx.Message}\n{httpEx.StackTrace}");
        //                    return new DisbursementTransferResponse();
        //                }

        //                try
        //                {
        //                    responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //                }
        //                catch (Exception readEx)
        //                {
        //                    stopwatch.Stop();
        //                    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Failed to read response after {stopwatch.ElapsedMilliseconds}ms: {readEx.Message}\n{readEx.StackTrace}");
        //                    return new DisbursementTransferResponse();
        //                }

        //                stopwatch.Stop();
        //                CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. Status={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Non-success status. ResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
        //                    return new DisbursementTransferResponse();
        //                }

        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject<DisbursementTransferResponse>(responseBody);
        //                    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Deserialized response. Code:{result?.Code ?? "<null>"} Description:{(result?.Description != null ? (result.Description.Length > 200 ? result.Description.Substring(0, 200) + "..." : result.Description) : "<null>")}");
        //                    return result ?? new DisbursementTransferResponse();
        //                }
        //                catch (Exception deserEx)
        //                {
        //                    CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview:{(responseBody != null ? responseBody.Substring(0, Math.Min(1000, responseBody.Length)) : "null")}");
        //                    return new DisbursementTransferResponse();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        stopwatch.Stop();
        //        CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - UNHANDLED EXCEPTION after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}\n{ex.StackTrace}");
        //        return new DisbursementTransferResponse();
        //    }
        //    finally
        //    {
        //        CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{DateTime.UtcNow:O}");
        //    }
        //}


        public static async Task<DisbursementTransferResponse> ProcessTransferRequestsAsync(DisbursementTransferRequest disbursementTransferRequest)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Log full payload before sending
            var fullPayload = JsonConvert.SerializeObject(disbursementTransferRequest, Formatting.Indented);
            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] START | RequestId:{requestId} | Timestamp:{startTime:O} | FullPayload:\n{fullPayload}");

            var baseUrl = ConfigurationManager.AppSettings["LoanTransferUrl"];
            var endpoint = "doTransfer";
            var requestUrl = string.Format("{0}/{1}", baseUrl, endpoint);

            var clientId = ConfigurationManager.AppSettings["ClientID"];
            var subscriptionKey = ConfigurationManager.AppSettings["OcpApimSubscriptionKey"];
            var password = ConfigurationManager.AppSettings["Password"];
            

            var utcdate = DateTime.UtcNow;

            var token = GetXTokenHeader(utcdate, clientId, password);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    var requestBody = JsonConvert.SerializeObject(disbursementTransferRequest, Formatting.Indented);
                    using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
                    {
                        // Prepare headers
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                        client.DefaultRequestHeaders.Add("UTCTimestamp", utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        client.DefaultRequestHeaders.Add("x-token", token);
                        if (!string.IsNullOrEmpty(clientId)) client.DefaultRequestHeaders.Remove("client_id");
                        if (!string.IsNullOrEmpty(clientId)) client.DefaultRequestHeaders.Add("client_id", clientId);
                        if (!string.IsNullOrEmpty(subscriptionKey)) client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
                        if (!string.IsNullOrEmpty(subscriptionKey)) client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                        var maskedSubKey = string.IsNullOrEmpty(subscriptionKey)
                            ? "null"
                            : (subscriptionKey.Length > 6 ? subscriptionKey.Substring(0, 3) + "..." + subscriptionKey.Substring(subscriptionKey.Length - 3) : "*****");

                        CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Sending POST {requestUrl} | client_id={clientId} | Ocp-Apim-Subscription-Key={maskedSubKey} | BodyLength:{requestBody?.Length ?? 0}");

                        HttpResponseMessage response;
                        string responseBody = null;
                        try
                        {
                            response = await client.PostAsync(requestUrl, content).ConfigureAwait(false);
                        }
                        catch (Exception httpEx)
                        {
                            stopwatch.Stop();
                            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - HTTP request failed after {stopwatch.ElapsedMilliseconds}ms: {httpEx.Message}\n{httpEx.StackTrace}");
                            return new DisbursementTransferResponse();
                        }

                        try
                        {
                            responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        }
                        catch (Exception readEx)
                        {
                            stopwatch.Stop();
                            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Failed to read response after {stopwatch.ElapsedMilliseconds}ms: {readEx.Message}\n{readEx.StackTrace}");
                            return new DisbursementTransferResponse();
                        }

                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. Status={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={(responseBody?.Length ?? 0)}");

                        if (!response.IsSuccessStatusCode)
                        {
                            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Non-success status. ResponsePreview: {(responseBody != null ? responseBody.Substring(0, Math.Min(200, responseBody.Length)) : "null")}");
                            return new DisbursementTransferResponse();
                        }

                        try
                        {
                            var result = JsonConvert.DeserializeObject<DisbursementTransferResponse>(responseBody);
                            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Deserialized response. Code:{result?.Code ?? "<null>"} Description:{(result?.Description != null ? (result.Description.Length > 200 ? result.Description.Substring(0, 200) + "..." : result.Description) : "<null>")}");
                            return result ?? new DisbursementTransferResponse();
                        }
                        catch (Exception deserEx)
                        {
                            CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponsePreview:{(responseBody != null ? responseBody.Substring(0, Math.Min(1000, responseBody.Length)) : "null")}");
                            return new DisbursementTransferResponse();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] RequestId:{requestId} - UNHANDLED EXCEPTION after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}\n{ex.StackTrace}");
                return new DisbursementTransferResponse();
            }
            finally
            {
                CoreOmniApiBase.LogEvent($"[ProcessTransferRequestsAsync] END | RequestId:{requestId} | Start:{startTime:O} | End:{DateTime.UtcNow:O}");
            }
        }

        public static bool UpdateTransferLoan(UserInfo userInfo, string applicationId, object data)
        {
            AuditInfo auditInfo = new AuditInfo
            {
                ModuleID = BRModule.Account,
                EventID = BROperation.Add,
                Status = BRStatus.Successful
            };

            try
            {
                var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

                using (IDbConnection connection = DBClient.GetConnection(userInfo))
                {
                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "p_UpdateLoanStatus";
                        command.CommandType = CommandType.StoredProcedure;

                        var param1 = command.CreateParameter();
                        param1.ParameterName = "@JsonData";
                        param1.Value = jsonData;
                        command.Parameters.Add(param1);

                        var param2 = command.CreateParameter();
                        param2.ParameterName = "@ApplicationID";
                        param2.Value = applicationId;
                        command.Parameters.Add(param2);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Message = ex.Message;
                auditInfo.Status = BRStatus.Failed;
                return false;
            }
        }

        //private static string GetXTokenHeader(DateTime utcdate, string ClientID, string Password)
        //{
        //    //UTCTimestamp in Sha512 Hash - format without separators for hashing
        //    var date = utcdate.ToString("yyyy-MM-ddHHmmss");
        //    var data = date + ClientID + Password;
        //    return SHA512(data);
        //}

        private static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("x2"));
                return hashedInputStringBuilder.ToString();

            }
        }





        #endregion FCMBLoans

        #region CARD Logic
        //_________________________Cards____________________________________

        static public bool GetSoapRequestData(UserInfo usrInfo, out BRDataSet dsSoapRequestData)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.View;

            //BRDataSet dsDisburseFundTransfer = null;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            string strNewInfo = string.Empty;
            string strLOPMessage = string.Empty;


            try
            {
                dsSoapRequestData = new BRDataSet();
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {

                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
                    arParms[0].Value = usrInfo.strBranch;


                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetSoapRequestData", dsSoapRequestData, new string[] { "dt_SoapRequestData" }, arParms);



                    if (dsSoapRequestData.Tables[0].Rows.Count > 0)
                    {
                        //auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsSoapRequestData.Tables[0]);
                        auditInfo.Message = "Success";
                    }
                    else
                    {
                        auditInfo.Message = "No Pending Disbursement List";
                    }



                    auditInfo.Status = BRStatus.Successful;




                }
                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                dsSoapRequestData = null;
                return false;
            }
            finally
            {
                Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending Disbursement List: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
                //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }
        static public bool GetReplacementCards(UserInfo usrInfo, out BRDataSet dsSoapRequestData)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.View;

            //BRDataSet dsDisburseFundTransfer = null;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            string strNewInfo = string.Empty;
            string strLOPMessage = string.Empty;


            try
            {
                dsSoapRequestData = new BRDataSet();
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {

                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
                    arParms[0].Value = usrInfo.strBranch;


                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetReplacementSoapRequest", dsSoapRequestData, new string[] { "dt_SoapRequestData" }, arParms);



                    if (dsSoapRequestData.Tables[0].Rows.Count > 0)
                    {
                        //auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsSoapRequestData.Tables[0]);
                        auditInfo.Message = "Success";
                    }
                    else
                    {
                        auditInfo.Message = "No Pending Disbursement List";
                    }



                    auditInfo.Status = BRStatus.Successful;




                }
                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                dsSoapRequestData = null;
                return false;
            }
            finally
            {
                Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending Disbursement List: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
                //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }

        public static List<string> GetCardsToBeReplaced(BRDataSet dsReplacementRequest)
        {
            if (dsReplacementRequest?.Tables[0] == null)
                return new List<string>();

            return dsReplacementRequest.Tables[0].AsEnumerable()
                .Select(r => r["CardNumber"].ToString())
                .ToList();
        }


        //public static List<ReplacementRequestXml> GenerateReplacementXmlRequest(BRDataSet dsReplacementRequest) {
        //    var cardReplacementList = dsReplacementRequest.Tables[0].AsEnumerable()
        //        .Select(r => new ReplacementRequestXml
        //        {

        //            ApplicationType = r["ApplicationType"].ToString(),
        //            ApplicationFlowID = r["ApplicationFlowID"].ToString(),
        //            ApplicationStatus = r["ApplicationStatus"].ToString(),
        //            OperatorID = r["OperatorID"].ToString(),
        //            InstitutionID = r["InstitutionID"].ToString(),
        //            AgentID = r["AgentID"].ToString(),
        //            CustomerType = r["CustomerType"].ToString(),
        //            customer = new Customer
        //            {
        //                CustomerCommand = r["CustomerCommand"].ToString(),
        //                CustomerNumber = r["CustomerNumber"].ToString(),
        //                Contract = new Contract
        //                {
        //                    ContractCommand = r["ContractCommand"].ToString(),
        //                    Card = new Models.Cards.Card
        //                    {
        //                        CardCommand = r["CardCommand"].ToString(),
        //                        CardID = r["CardID"].ToString(),
        //                        CardNumber = r["CardNumber"].ToString(),
        //                        CardType = r["CardType"].ToString(),
        //                        Category = r["Category"].ToString(),
        //                        CardDeliveryStatus = r["CardDeliveryStatus"].ToString(),
        //                        PinRequest = r["PinRequest"].ToString(),
        //                        PersoPriority = r["PersoPriority"].ToString(),
        //                        PinMailerRequest = r["PinMailerRequest"].ToString(),
        //                        ReIssueCommand = r["ReIssueCommand"].ToString(),
        //                        ReIssueCardState = r["ReIssueCardState"].ToString(),
        //                        ReIssueProductID = r["ReIssueProductID"].ToString(),
        //                        SequentialNumber = r["SequentialNumber"].ToString(),
        //                        CardState = r["CardState"].ToString(),
        //                        CardStatus = r["CardStatus"].ToString(),
        //                        cardholder =  new Cardholder { 

        //                            CardHolderCommand = r["CardHolderCommand"].ToString(),
        //                            CardHolderName = r["CardHolderName"].ToString(),
        //                            CustomerRelation = r["CustomerRelation"].ToString(),
        //                        }
        //                    }

        //                }
        //            }

        //        }).ToList() ;

        //    return cardReplacementList;



        //}


        public static List<ReplacementRequestXml> GenerateReplacementXmlRequest(GetCardsResponseXMl.Card card)
        {
            if (card == null)
                return new List<ReplacementRequestXml>();

            var firstName = card.CardHolderName != null ? card.CardHolderName.first_name : null;
            var lastName = card.CardHolderName != null ? card.CardHolderName.last_name : null;
            var cardHolderName = string.Format("{0} {1}", firstName, lastName).Trim();

            var cardReplacement = new ReplacementRequestXml
            {
                ApplicationType = "APTPISSA",
                ApplicationFlowID = "5",
                ApplicationStatus = "APST0006",
                OperatorID = "ADMIN",
                InstitutionID = card.inst_id,
                AgentID = "50000039",
                CustomerType = "ENTTPERS",
                customer = new Customer
                {
                    CustomerCommand = "CMMDEXPR",
                    CustomerNumber = card.customer_number,
                    Contract = new Models.Cards.Contract
                    {
                        ContractCommand = "CMMDEXPR",
                        Card = new Models.Cards.Card
                        {
                            CardCommand = "CMMDEXUP",
                            CardID = card.id,
                            CardNumber = card.card_number,
                            CardType = card.card_type,
                            Category = card.category,
                            CardDeliveryStatus = card.delivery_status,
                            PinRequest = "PNRQGENR",
                            PersoPriority = "PRSP0500",
                            PinMailerRequest = "PMRQDONT",
                            ReIssueCommand = "RCMDNEWN",
                            ReIssueCardState = "CSTE0100",
                            ReIssueProductID = card.product_id,
                            SequentialNumber = card.seq_number != null ? card.seq_number.ToString() : string.Empty,
                            CardState = "CSTE0100",
                            CardStatus = "CSTS0014",
                            cardholder = new Cardholder
                            {
                                CardHolderCommand = "CMMDEXPR",
                                CardHolderName = cardHolderName,
                                CustomerRelation = "RSCBEXTR",
                            }
                        }
                    }
                }
            };

            return new List<ReplacementRequestXml> { cardReplacement };
        }


        public static List<SoapRequestData> GenerateXmlRequest(BRDataSet dsSoapRequestData)
        {

            var soapRequestDataList = dsSoapRequestData.Tables[0].AsEnumerable()
                .Select(row => new SoapRequestData
                {
                    ApplicationNumber = row["ApplicationNumber"].ToString(),
                    ApplicationDate = row["ApplicationDate"].ToString(),
                    ApplicationType = row["ApplicationType"].ToString(),
                    ApplicationFlowId = row["ApplicationFlowId"].ToString(),
                    ApplicationStatus = row["ApplicationStatus"].ToString(),
                    OperatorId = row["OperatorId"].ToString(),
                    InstitutionId = row["InstitutionId"].ToString(),
                    AgentNumber = row["AgentNumber"].ToString(),
                    CustomerType = row["CustomerType"].ToString(),
                    Customer = new CustomerData
                    {
                        Command = row["Customer_Command"].ToString(),
                        CustomerNumber = row["CustomerNumber"].ToString(),
                        Contract = new ContractData
                        {
                            Command = row["Contract_Command"].ToString(),
                            ContractType = row["ContractType"].ToString(),
                            ProductNumber = row["ProductNumber"].ToString(),
                            Card = new CardData
                            {
                                Command = row["Card_Command"].ToString(),
                                CardType = row["CardType"].ToString(),
                                Cardholder = new CardholderData
                                {
                                    Command = row["Cardholder_Command"].ToString(),
                                    CardholderName = row["CardholderName"].ToString(),
                                    Notification = new NotificationData
                                    {
                                        Command = row["Notification_Command"].ToString(),
                                        DeliveryChannel = row["DeliveryChannel"].ToString(),
                                        ContactType = row["ContactType"].ToString(),
                                        DeliveryAddress = row["DeliveryAddress"].ToString(),
                                        IsActive = row["IsActive"].ToString()
                                    }
                                }
                            },
                            Service = new[]
                            {
                                new ServiceData { Value = "60000011" },
                                new ServiceData { Value = "60000012" },
                                new ServiceData { Value = "60000014" }
                            },
                            Account = new AccountData
                            {
                                Command = row["Account_Command"].ToString(),
                                AccountNumber = row["AccountNumber"].ToString(),
                                Currency = row["Currency"].ToString(),
                                AccountType = row["AccountType"].ToString(),
                                AccountStatus = row["AccountStatus"].ToString(),
                                AccountObject = new AccountObjectData
                                {
                                    AccountLinkFlag = row["AccountLinkFlag"].ToString(),
                                    IsPosDefault = row["IsPosDefault"].ToString(),
                                    IsAtmDefault = row["IsAtmDefault"].ToString()
                                }
                            }
                        },
                        Person = new PersonData
                        {
                            Command = row["Person_Command"].ToString(),
                            PersonName = new PersonNameData
                            {
                                Surname = row["Surname"].ToString(),
                                FirstName = row["FirstName"].ToString()
                            },
                            IdentityCard = new IdentityCardData
                            {
                                Command = row["IdentityCard_Command"].ToString(),
                                IdType = row["IdType"].ToString(),
                                IdSeries = row["IdSeries"].ToString(),
                                IdNumber = row["IdNumber"].ToString()
                            }
                        },
                        Contact = new ContactData
                        {
                            Command = row["Contact_Command"].ToString(),
                            ContactType = row["ContactType"].ToString(),
                            PreferredLang = "LANGENG"
                        },
                        Address = new AddressData
                        {
                            Command = row["Address_Command"].ToString(),
                            AddressType = row["AddressType"].ToString(),
                            Country = row["Country"].ToString(),
                            AddressName = new AddressNameData
                            {
                                Region = row["Region"].ToString(),
                                City = row["City"].ToString(),
                                Street = row["Street"].ToString()
                            },
                            House = row["House"].ToString(),
                            Apartment = row["Apartment"].ToString()
                        }
                    }
                }).ToList();

            return soapRequestDataList;
        }

        public static string CardReplacementEnvelope(ReplacementRequestXml requestXml)
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                        xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                        xmlns:q0=""urn:ifxforum-org:XSD:1""
                        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                        <soapenv:Body>
                            <application xmlns=""http://sv.bpc.in/SVAP/iss"">
                            <application_type>{requestXml.ApplicationType}</application_type>
                            <application_flow_id>{requestXml.ApplicationFlowID}</application_flow_id>
                            <application_status>{requestXml.ApplicationStatus}</application_status>
                            <operator_id>{requestXml.OperatorID}</operator_id>
                            <institution_id>{requestXml.InstitutionID}</institution_id>
                            <agent_id>{requestXml.AgentID}</agent_id>
                            <customer_type>{requestXml.CustomerType}</customer_type>
                                <customer>
                                <command>{requestXml.customer.CustomerCommand}</command>
                                <customer_number>{requestXml.customer.CustomerNumber}</customer_number>
                                    <contract>
                                    <command>{requestXml.customer.Contract.ContractCommand}</command>
                                        <card>
                                        <command>{requestXml.customer.Contract.Card.CardCommand}</command>
                                        <card_id>{requestXml.customer.Contract.Card.CardID}</card_id>
                                        <card_number>{requestXml.customer.Contract.Card.CardNumber}</card_number>
                                        <card_type>{requestXml.customer.Contract.Card.CardType}</card_type>
                                        <category>{requestXml.customer.Contract.Card.Category}</category>
                                        <card_delivery_status>{requestXml.customer.Contract.Card.CardDeliveryStatus}</card_delivery_status>
                                        <pin_request>{requestXml.customer.Contract.Card.PinRequest}</pin_request>
                                        <perso_priority>{requestXml.customer.Contract.Card.PersoPriority}</perso_priority>
                                        <pin_mailer_request>{requestXml.customer.Contract.Card.PinMailerRequest}</pin_mailer_request>
                                        <reissue_command>{requestXml.customer.Contract.Card.ReIssueCommand}</reissue_command>
                                        <reissue_card_state>{requestXml.customer.Contract.Card.ReIssueCardState}</reissue_card_state>
                                        <reissue_product_id>{requestXml.customer.Contract.Card.ReIssueProductID}</reissue_product_id>
                                        <sequential_number>0</sequential_number>
                                        <card_state>{requestXml.customer.Contract.Card.CardState}</card_state>
                                        <card_status>{requestXml.customer.Contract.Card.CardStatus}</card_status>
                                            <cardholder>
                                                <command>{requestXml.customer.Contract.Card.cardholder.CardHolderCommand}</command>
                                                <cardholder_name>{requestXml.customer.Contract.Card.cardholder.CardHolderName}</cardholder_name>
                                                <customer_relation>{requestXml.customer.Contract.Card.cardholder.CustomerRelation}</customer_relation>
                                            </cardholder>
                                        </card>
                                    </contract>
                                </customer>
                            </application>
                        </soapenv:Body>
                        </soapenv:Envelope>";

        }

        public static string CreateSoapEnvelope(SoapRequestData soapRequestData)
        {
            return $@"
    <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:q0=""urn:ifxforum-org:XSD:1"">
        <soapenv:Header/>
        <soapenv:Body>
            <application xmlns=""http://sv.bpc.in/SVAP/iss"">
                <application_date>{soapRequestData.ApplicationDate}</application_date>
                <application_type>{soapRequestData.ApplicationType}</application_type>
                <application_flow_id>{soapRequestData.ApplicationFlowId}</application_flow_id>
                <application_status>{soapRequestData.ApplicationStatus}</application_status>
                <operator_id>{soapRequestData.OperatorId}</operator_id>
                <institution_id>{soapRequestData.InstitutionId}</institution_id>
                <agent_number>{soapRequestData.AgentNumber}</agent_number>
                <customer_type>{soapRequestData.CustomerType}</customer_type>
                <customer>
                    <command>{soapRequestData.Customer.Command}</command>
                    <customer_number>{soapRequestData.Customer.CustomerNumber}</customer_number>
                    <contract>
                        <command>{soapRequestData.Customer.Contract.Command}</command>
                        <contract_type>{soapRequestData.Customer.Contract.ContractType}</contract_type>
                        <product_number>{soapRequestData.Customer.Contract.ProductNumber}</product_number>
                        <card id=""card_1"">
                            <command>{soapRequestData.Customer.Contract.Card.Command}</command>
                            <card_type>{soapRequestData.Customer.Contract.Card.CardType}</card_type>
                            <cardholder>
                                <command>{soapRequestData.Customer.Contract.Card.Cardholder.Command}</command>
                                <cardholder_name>{soapRequestData.Customer.Contract.Card.Cardholder.CardholderName}</cardholder_name>
                                <notification>
                                    <command>{soapRequestData.Customer.Contract.Card.Cardholder.Notification.Command}</command>
                                    <delivery_channel>{soapRequestData.Customer.Contract.Card.Cardholder.Notification.DeliveryChannel}</delivery_channel>
                                    <contact_type>{soapRequestData.Customer.Contract.Card.Cardholder.Notification.ContactType}</contact_type>
                                    <delivery_address>{soapRequestData.Customer.Contract.Card.Cardholder.Notification.DeliveryAddress}</delivery_address>
                                    <is_active>{soapRequestData.Customer.Contract.Card.Cardholder.Notification.IsActive}</is_active>
                                </notification>
                            </cardholder>
                        </card>
                        <service value=""60000011"">
                            <service_object ref_id=""account_1""/>
                        </service>
                        <service value=""60000012"">
                            <service_object ref_id=""card_1""/>
                        </service>
                        <service value=""60000014"">
                            <service_object ref_id=""card_1""/>
                        </service>
                        <account id=""account_1"">
                            <command>{soapRequestData.Customer.Contract.Account.Command}</command>
                            <account_number>{soapRequestData.Customer.Contract.Account.AccountNumber}</account_number>
                            <currency>{soapRequestData.Customer.Contract.Account.Currency}</currency>
                            <account_type>{soapRequestData.Customer.Contract.Account.AccountType}</account_type>
                            <account_status>{soapRequestData.Customer.Contract.Account.AccountStatus}</account_status>
                            <account_object ref_id=""card_1"">
                                <account_link_flag>{soapRequestData.Customer.Contract.Account.AccountObject.AccountLinkFlag}</account_link_flag>
                                <is_pos_default>{soapRequestData.Customer.Contract.Account.AccountObject.IsPosDefault}</is_pos_default>
                                <is_atm_default>{soapRequestData.Customer.Contract.Account.AccountObject.IsAtmDefault}</is_atm_default>
                            </account_object>
                        </account>
                    </contract>
                    <person>
                        <command>{soapRequestData.Customer.Person.Command}</command>
                        <person_name language=""LANGENG"">
                            <surname>{soapRequestData.Customer.Person.PersonName.Surname}</surname>
                            <first_name>{soapRequestData.Customer.Person.PersonName.FirstName}</first_name>
                        </person_name>
                        <identity_card>
                            <command>{soapRequestData.Customer.Person.IdentityCard.Command}</command>
                            <id_type>{soapRequestData.Customer.Person.IdentityCard.IdType}</id_type>
                            <id_series>{soapRequestData.Customer.Person.IdentityCard.IdSeries}</id_series>
                            <id_number>{soapRequestData.Customer.Person.IdentityCard.IdNumber}</id_number>
                        </identity_card>
                    </person>
                    <contact>
                        <command>{soapRequestData.Customer.Contact.Command}</command>
                        <contact_type>{soapRequestData.Customer.Contact.ContactType}</contact_type>
                        <preferred_lang>LANGENG</preferred_lang>
                    </contact>
                    <address>
                        <command>{soapRequestData.Customer.Address.Command}</command>
                        <address_type>{soapRequestData.Customer.Address.AddressType}</address_type>
                        <country>{soapRequestData.Customer.Address.Country}</country>
                        <address_name>
                            <region>{soapRequestData.Customer.Address.AddressName.Region}</region>
                            <city>{soapRequestData.Customer.Address.AddressName.City}</city>
                            <street>{soapRequestData.Customer.Address.AddressName.Street}</street>
                        </address_name>
                        <house>{soapRequestData.Customer.Address.House}</house>
                        <apartment>{soapRequestData.Customer.Address.Apartment}</apartment>
                    </address>
                </customer>
            </application>
        </soapenv:Body>
    </soapenv:Envelope>";
        }



        public static string createCardRequestXml(CardsRequestXml cardsRequestXml)
        {

            return $@"
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:int=""http://bpc.ru/SVXP/integration"">
                 <soapenv:Header/>
                    <soapenv:Body> 
                        <int:getCardsRequest>
                            <inst_id>{cardsRequestXml.inst_Id}</inst_id>
                            <card_mask>{cardsRequestXml.card_mask}</card_mask>
                            <get_balance>{cardsRequestXml.get_balance}</get_balance>
                            <lang>{cardsRequestXml.lang}</lang>
                         </int:getCardsRequest>
                 </soapenv:Body>
            </soapenv:Envelope>

            ";

        }
        //SOAP
        public static async Task<string> ExecuteSoapRequest(string soapEnvelope, string atmUrl)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(atmUrl);
            myHttpWebRequest.Method = "POST";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            byte[] data = Encoding.ASCII.GetBytes(soapEnvelope);
            myHttpWebRequest.ContentType = "application/xml";
            myHttpWebRequest.ContentLength = data.Length;

            using (Stream requestStream = myHttpWebRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            //log Request
            CoreOmniApiBase.LogEvent($"METHOD: ExecuteSoapRequest \n GetCardsRequest: {myHttpWebRequest}\n");

            using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
            using (Stream responseStream = myHttpWebResponse.GetResponseStream())
            using (StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default))
            {
                return await myStreamReader.ReadToEndAsync();
            }

            
        }

        //public  static async Task<string> ExecuteRequestAsync(string jsonBody, string requestUrl)
        //{
        //    try {
        //        CoreOmniApiBase.LogEvent($"Executing request to {requestUrl} with body: {jsonBody}");
        //        //Bypass SSL certificate validation
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        //        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        //        using var client = new HttpClient();
        //        var response = await client.PostAsync(requestUrl, content);
        //        response.EnsureSuccessStatusCode();
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        return responseBody;

        //    }
        //    catch(Exception ex)
        //    {
        //        CoreOmniApiBase.LogEvent($"Error in ExecuteRequestAsync: {ex.Message}");
        //        throw;
        //    }
        //}

        public static void HandleResponse(string AccountId, string ApplicationId, string response, UserInfo usrInfo)
        {
            var serializer = new XmlSerializer(typeof(Models.Cards.ResponseDataXml.Envelope));
            Models.Cards.ResponseDataXml.Envelope envelope;

            using (var reader = new StringReader(response))

            using (var xmlReader = XmlReader.Create(reader))
            {
                envelope = (Models.Cards.ResponseDataXml.Envelope)serializer.Deserialize(xmlReader);
            }

            CoreOmniApiBase.LogEvent($"Raw Envelope: Code={envelope}");


            if (envelope.Body.Fault != null)
            {
                var faultCode = envelope.Body.Fault.FaultCode ?? "unknown fault code";
                var faultString = envelope.Body.Fault.FaultString ?? "unknown fault string";
                var faultDetail = envelope.Body.Fault.Detail?.FaultType?.Text ?? "unknown fault detail";

                // Log the fault details
                CoreOmniApiBase.LogEvent($"SOAP FAULT: Code={faultCode}, Message={faultString}, Detail={faultDetail}");

                // Update the application status as failed
                var failedParameters = new Dictionary<string, object>
                {
                    {"IsSuccess", 0}, // Mark the application as failed
                    {"FaultCode", faultCode},
                    {"FaultMessage", faultString},
                    {"FaultDetail", faultDetail},
                    {"ApplicationId", ApplicationId},
                    {"AccountNumber", AccountId}
                };

                // Update the card status in the database
                OmniCore.UpdateCardStatus(usrInfo, failedParameters);

                // Throw an exception to indicate the failure
                throw new Exception($"SOAP Fault: {faultString}");
            }

            // If no fault is found, process the successful response
            var cardHolderNumber = envelope.Body.Application.Customer.Contract.Card.Cardholder.CardholderNumber;
            var productNumber = envelope.Body.Application.Customer.Contract.ProductNumber;
            var productID = envelope.Body.Application.Customer.Contract.ProductID;
            var cardID = envelope.Body.Application.Customer.Contract.Card.CardID;
            var cardNumber = envelope.Body.Application.Customer.Contract.Card.CardNumber;
            var expiryDate = envelope.Body.Application.Customer.Contract.Card.ExpirationDate;
            var accountNumber = envelope.Body.Application.Customer.Contract.Account.AccountNumber;

            var parameters = new Dictionary<string, object>
                {
                    {"CardHolderNumber", cardHolderNumber},
                    {"ProductNumber", productNumber},
                    {"ProductID", productID},
                    {"CardID", cardID},
                    {"CardNumber", cardNumber},
                    {"ExpiryDate", expiryDate.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"AccountNumber", accountNumber},
                    {"IsSuccess", 1}, // Mark the application as successful
                    {"ApplicationId", ApplicationId},
                    {"AccountId", AccountId}
                };

            // Update the card status in the database
            OmniCore.UpdateCardStatus(usrInfo, parameters);
        }

        //public static void HandleReplacementResponse(string response, UserInfo usrInfo)
        //{
        //    try
        //    {
        //        var serializer = new XmlSerializer(typeof(Models.Cards.ReplacementResponseXml.Envelope));
        //        Models.Cards.ReplacementResponseXml.Envelope envelope;

        //        using (var reader = new StringReader(response))
        //        using (var xmlReader = XmlReader.Create(reader))
        //        {
        //            envelope = (Models.Cards.ReplacementResponseXml.Envelope)serializer.Deserialize(xmlReader);
        //        }

        //        CoreOmniApiBase.LogEvent($"Raw Envelope: {response}");

        //        // Handle SOAP fault
        //        if (envelope.Body.Fault != null)
        //        {
        //            var faultCode = envelope.Body.Fault.FaultCode ?? "unknown fault code";
        //            var faultString = envelope.Body.Fault.FaultString ?? "unknown fault string";
        //            var faultDetail = envelope.Body.Fault.Detail?.FaultType?.Text ?? "unknown fault detail";

        //            CoreOmniApiBase.LogEvent($"SOAP FAULT: Code={faultCode}, Message={faultString}, Detail={faultDetail}");

        //            var failedParameters = new Dictionary<string, object>
        //    {
        //        {"IsSuccess", 0},
        //        {"FaultCode", faultCode},
        //        {"FaultMessage", faultString},
        //        {"FaultDetail", faultDetail}
        //    };

        //            OmniCore.UpdateReplacedCards(usrInfo, failedParameters);

        //            throw new Exception($"SOAP Fault: {faultString}");
        //        }

        //        // If no fault, process success
        //        var cardHolderNumber = envelope.Body.Application.Customer.Contract.Card.Cardholder.CardholderName;
        //        var productNumber = envelope.Body.Application.Customer.Contract.Card.ReissueProductId;
        //        var productID = envelope.Body.Application.Customer.Contract.Card.ReissueProductId;
        //        var cardID = envelope.Body.Application.Customer.Contract.Card.ReissueCardId;
        //        var cardNumber = envelope.Body.Application.Customer.Contract.Card.CardNumber;
        //        var expiryDate = envelope.Body.Application.Customer.Contract.Card.ReissueExpirationDate;
        //        var reIssuedCardNumber = envelope.Body.Application.Customer.Contract.Card.ReissueCardNumber;

        //        var parameters = new Dictionary<string, object>
        //{
        //    {"CardHolderNumber", cardHolderNumber},
        //    {"ProductNumber", productNumber},
        //    {"ProductID", productID},
        //    {"CardID", cardID},
        //    {"CardNumber", cardNumber},
        //    {"ReIssuedCardNumber", reIssuedCardNumber},
        //    {"ExpiryDate", expiryDate.ToString("yyyy-MM-dd HH:mm:ss")},
        //    {"IsSuccess", 1}
        //};

        //        OmniCore.UpdateCardStatus(usrInfo, parameters);
        //    }
        //    catch (InvalidOperationException xmlEx) // XmlSerializer deserialization issues
        //    {
        //        CoreOmniApiBase.LogEvent($"HandleReplacementResponse - XML Deserialization Failed: {xmlEx.Message}\nResponse: {response}");
        //    }
        //    catch (Exception ex)
        //    {
        //        CoreOmniApiBase.LogEvent($"HandleReplacementResponse - GENERAL ERROR: {ex.GetType().Name} - {ex.Message}\nResponse: {response}");
        //    }
        //}


        public static void HandleReplacementResponse(string response, UserInfo usrInfo, string cardNumber)
        {
            try
            {
                CoreOmniApiBase.LogEvent($"Raw Envelope: {response}");

                if (response.Contains("<soap:Fault>"))
                {
                    // Deserialize SOAP Fault Envelope
                    var serializer = new XmlSerializer(typeof(Models.Cards.ReplacementResponseXml.Envelope));
                    Models.Cards.ReplacementResponseXml.Envelope envelope;

                    using (var reader = new StringReader(response))
                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        envelope = (Models.Cards.ReplacementResponseXml.Envelope)serializer.Deserialize(xmlReader);
                    }

                    var faultCode = envelope.Body.Fault.FaultCode ?? "unknown";
                    var faultString = envelope.Body.Fault.FaultString ?? "unknown";
                    var faultDetail = envelope.Body.Fault.Detail?.FaultType?.Text ?? "unknown";

                    CoreOmniApiBase.LogEvent($"SOAP FAULT: Code={faultCode}, Message={faultString}, Detail={faultDetail}");

                    OmniCore.UpdateReplacedCards(usrInfo, new Dictionary<string, object>
            {
                {"IsSuccess", 0},
                {"FaultCode", faultCode},
                {"FaultMessage", faultString},
                {"FaultDetail", faultDetail},
                {"CardNumber",cardNumber }
            });
                }
                else if (response.Contains("<application"))
                {
                    // Deserialize Application success response
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "http://sv.bpc.in/SVAP/iss");

                    var serializer = new XmlSerializer(typeof(Models.Cards.ReplacementResponseXml.ApplicationEnvelope));
                    Models.Cards.ReplacementResponseXml.ApplicationEnvelope appResponse;

                    using (var reader = new StringReader(response))
                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        appResponse = (Models.Cards.ReplacementResponseXml.ApplicationEnvelope)serializer.Deserialize(xmlReader);
                    }

                    var card = appResponse.Body.Application.Customer.Contract.Card;

                    var parameters = new Dictionary<string, object>
            {
                {"CardHolderNumber", card.Cardholder.CardholderName},
                {"ProductNumber", card.ReissueProductId},
                {"ProductID", card.ReissueProductId},
                {"CardID", card.ReissueCardId},
                {"CardNumber", card.CardNumber},
                {"ReIssuedCardNumber", card.ReissueCardNumber},
                {"ExpiryDate", card.ReissueExpirationDate.ToString("yyyy-MM-dd HH:mm:ss")},
                {"IsSuccess", 1}
            };

                    //OmniCore.UpdateCardStatus(usrInfo, parameters);
                    OmniCore.UpdateReplacedCards(usrInfo, parameters);
                }
                else
                {
                    CoreOmniApiBase.LogEvent("Unhandled SOAP response format.");
                }
            }
            catch (InvalidOperationException xmlEx)
            {
                CoreOmniApiBase.LogEvent($"HandleReplacementResponse - XML Deserialization Failed: {xmlEx.Message}\nResponse: {response}");
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"HandleReplacementResponse - GENERAL ERROR: {ex.GetType().Name} - {ex.Message}\nResponse: {response}");
            }
        }




        public static bool UpdateReplacedCards(UserInfo userInfo, object Data)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.Account;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;
            try
            {
                //serialize parameters to json data
                var jsonData = JsonConvert.SerializeObject(Data, Newtonsoft.Json.Formatting.Indented);

                using (IDbConnection connection = DBClient.GetConnection(userInfo))
                {
                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "p_UpdateReplacedCards";
                        command.CommandType = CommandType.StoredProcedure;
                        IDbDataParameter param = command.CreateParameter();
                        param.ParameterName = "@JsonData";
                        param.Value = jsonData;
                        command.Parameters.Add(param);

                        command.ExecuteNonQuery();
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Message = ex.Message.ToString();
                auditInfo.Status = BRStatus.Failed;
                return false;
            }
        }

        public static bool UpdateCardStatus(UserInfo userInfo, object Data)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.Account;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;
            try
            {
                //serialize parameters to json data
                var jsonData = JsonConvert.SerializeObject(Data, Newtonsoft.Json.Formatting.Indented);

                using (IDbConnection connection = DBClient.GetConnection(userInfo))
                {
                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "p_UpdateCardStatus";
                        command.CommandType = CommandType.StoredProcedure;
                        IDbDataParameter param = command.CreateParameter();
                        param.ParameterName = "@JsonData";
                        param.Value = jsonData;
                        command.Parameters.Add(param);

                        command.ExecuteNonQuery();
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                auditInfo.Message = ex.Message.ToString();
                auditInfo.Status = BRStatus.Failed;
                return false;
            }
        }

        public string Handle(CardStatusCode status)
        {
            switch (status)
            {
                case CardStatusCode.Valid:
                    return "Card is active and usable.";

                case CardStatusCode.CallIssuer:
                case CardStatusCode.WarmCard:
                    return "Card requires issuer review before use.";

                case CardStatusCode.DoNotHonor:
                case CardStatusCode.NotPermitted:
                    return "Transaction declined. Please contact support.";

                case CardStatusCode.LostCardCapture:
                case CardStatusCode.StolenCardCapture:
                case CardStatusCode.FraudPrevention:
                case CardStatusCode.TemporaryBlockedByClient:
                    // Disable the card in DB and notify cardholder
                    return "Card blocked for security reasons.";

                case CardStatusCode.PinAttemptsExceeded:
                case CardStatusCode.ForcedPinChange:
                    return "Please reset or change your PIN.";

                default:
                    return "Unknown or unhandled card status.";
            }
        }

        public static async Task<string> ChangeCardStatusAsync(CardStatusChange.RequestData requestData)
        {
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var appId = ConfigurationManager.AppSettings["AppId"];
            var appVersion = ConfigurationManager.AppSettings["AppVersion"];
            var cardsUrl = ConfigurationManager.AppSettings["CRDBCardsUrl"];

            var startTime = DateTime.UtcNow;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string requestId = Guid.NewGuid().ToString();

            try
            {
                // Log method entry and incoming payload
                CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] START | RequestId: {requestId} | Timestamp: {startTime:O} | Payload: {JsonConvert.SerializeObject(requestData)}");

                // Get access token and log a masked preview
                string accessToken = null;
                try
                {
                    accessToken = await OmniCore.GetCrdbAccessToken();
                    string maskedToken = accessToken == null ? "null" : (accessToken.Length > 10 ? accessToken.Substring(0, 6) + "..." + accessToken.Substring(accessToken.Length - 4) : "********");
                    CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] Retrieved access token (masked): {maskedToken}");
                }
                catch (Exception tokenEx)
                {
                    CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] ERROR getting access token | RequestId: {requestId} | Message: {tokenEx.Message}\nStackTrace: {tokenEx.StackTrace}");
                    throw; // bubble up so caller sees failure
                }

                using (var client = new HttpClient())
                {
                    // Prepare headers
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken ?? string.Empty);

                    // NOTE: header name used by CRDB is "Channel" in other callers — keep consistent
                    client.DefaultRequestHeaders.Remove("Channel");
                    client.DefaultRequestHeaders.Add("Channel", channelId);
                    client.DefaultRequestHeaders.Add("AppId", appId);
                    client.DefaultRequestHeaders.Add("AppVersion", appVersion);
                    client.DefaultRequestHeaders.Add("RequestId", requestId);

                    // Serialize body
                    var requestContent = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                    var content = new StringContent(requestContent, Encoding.UTF8, "application/json");

                    // Log request metadata (don't include full token)
                    CoreOmniApiBase.LogEvent(
                        $"[CRDB][ChangeCardStatus] Sending request | RequestId: {requestId} | URL: {cardsUrl} | Channel: {channelId} | AppId: {appId} | AppVersion: {appVersion} | BodyLength: {requestContent?.Length ?? 0}");

                    HttpResponseMessage response = null;
                    string responseContent = null;

                    try
                    {
                        response = await client.PostAsync(cardsUrl, content);
                    }
                    catch (Exception sendEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] HTTP Send FAILED | RequestId: {requestId} | ElapsedMs: {stopwatch.ElapsedMilliseconds} | Message: {sendEx.Message}\nStackTrace: {sendEx.StackTrace}");
                        throw;
                    }

                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception readEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] Failed reading response content | RequestId: {requestId} | ElapsedMs: {stopwatch.ElapsedMilliseconds} | Message: {readEx.Message}\nStackTrace: {readEx.StackTrace}");
                        throw;
                    }

                    // Log full response (careful with sensitive data)
                    stopwatch.Stop();
                    CoreOmniApiBase.LogEvent(
                        $"[CRDB][ChangeCardStatus] Response received | RequestId: {requestId} | StatusCode: {(int)response.StatusCode} ({response.ReasonPhrase}) | ElapsedMs: {stopwatch.ElapsedMilliseconds}\nResponseBody: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] SUCCESS | RequestId: {requestId} | Returning response content.");
                        return responseContent;
                    }
                    else
                    {
                        var errorMsg = $"CRDB API returned non-success status. RequestId: {requestId} | Status: {(int)response.StatusCode} ({response.ReasonPhrase}) | Content: {responseContent}";
                        CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] ERROR | {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                // include inner exceptions if any
                var inner = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : string.Empty;
                CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] EXCEPTION | RequestId: {requestId} | Message: {ex.Message}{inner}\nStackTrace: {ex.StackTrace}");
                throw; // preserve previous behavior (rethrow)
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"[CRDB][ChangeCardStatus] END | RequestId: {requestId} | Start: {startTime:O} | End: {endTime:O}");
            }
        }


        public static async Task<string> GetCrdbAccessToken()
        {
            var accessTokenUrl = ConfigurationManager.AppSettings["CrdbAccessToken"];
            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];
            var cookie = ConfigurationManager.AppSettings["Cookie"];

            try
            {
                // Set TLS 1.2 or higher
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    // Create form data
                    var formData = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("scope", "common")
                        };

                    var content = new FormUrlEncodedContent(formData);

                    // Encode username and password for Basic Authentication
                    var basicAuthentication = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);
                    client.DefaultRequestHeaders.Add("Cookie", cookie);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0");

                    // Log the request details
                    var requestLog = $"Request URL: {accessTokenUrl}\n" +
                                    $"Headers: Authorization: Basic {basicAuthentication}, Cookie: {cookie}\n" +
                                    $"Form Data: {string.Join(", ", formData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
                    CoreOmniApiBase.LogEvent($"METHOD: GetCrdbAccessToken \n REQUEST: {requestLog}");

                    // Send the request with retry mechanism
                    int retryCount = 3;
                    for (int i = 0; i < retryCount; i++)
                    {
                        try
                        {
                            var response = await client.PostAsync(accessTokenUrl, content);
                            var responseContent = await response.Content.ReadAsStringAsync();

                            // Log the response details
                            var responseLog = $"Status Code: {response.StatusCode}\n" +
                                              $"Response Content: {responseContent}";
                            CoreOmniApiBase.LogEvent($"METHOD: GetCrdbAccessToken \n RESPONSE: {responseLog}");

                            if (response.IsSuccessStatusCode)
                            {
                                dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
                                var access_Token = tokenResponse.access_token;

                                return access_Token;
                            }
                            else
                            {
                                throw new Exception($"ErrorStatusCode : {response.StatusCode} - {responseContent}");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (i == retryCount - 1) throw; // Throw exception on the last retry
                            await Task.Delay(1000); // Wait before retrying
                        }
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                // Log the inner exception and stack trace for more details
                CoreOmniApiBase.LogEvent($"METHOD: GetCrdbAccessToken \n HTTP REQUEST EXCEPTION: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStack Trace: {ex.StackTrace}");
                throw new Exception($"HTTP request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"METHOD: GetCrdbAccessToken \n EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                throw new Exception($"Error: {ex.Message}");
            }

            return null; // This line will never be reached
        }

        public async Task<string> ResetPinCount(PinResetCount pinResetCount)
        {
            string accessToken = await OmniCore.GetCrdbAccessToken();
            string crdbAtmUrl = ConfigurationManager.AppSettings["CRDBCardsUrl"];
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var AppId = ConfigurationManager.AppSettings["AppId"];
            var AppVersion = ConfigurationManager.AppSettings["AppVersion"];
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(crdbAtmUrl));
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("ChannnelId", channelId);
                request.Headers.Add("AppId", AppId);
                request.Headers.Add("AppVersion", AppVersion);
                request.Headers.Add("RequestId", Guid.NewGuid().ToString());

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var requestContent = JsonConvert.SerializeObject(pinResetCount);

                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");

                var requestLog = $"RequestUrl : {crdbAtmUrl} \n " +
                    $"Method : POST \n " +
                     $"Headers: Authorization: Bearer {accessToken}, " +
                                 $"Accept: */*, Content-Type: application/json, " +
                                 $"ChannnelId: {request.Headers.GetValues("ChannelId").First()}," +
                                 $" AppId: {request.Headers.GetValues("AppId").First()}," +
                                 $" AppVersion: {request.Headers.GetValues("AppVersion").First()}, " +
                                 $"RequestId: {request.Headers.GetValues("RequestId").First()}\n" +
                                 $"Content : {request.Content}";
                CoreOmniApiBase.LogEvent($"METHOD: ResetPinCount \n REQUEST: {requestLog}");



                using (var client = new HttpClient())
                {

                    var response = await client.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var responseLog = $"StatusCode : {response.StatusCode} \n" +
                        $"Response Content : {responseContent}";

                    CoreOmniApiBase.LogEvent($"METHOD: ResetPinCount \n REQUEST: {responseLog}");

                    if (response.IsSuccessStatusCode)
                    {
                        var resetPinCountResponse = JsonConvert.DeserializeObject<ResetCountResponse>(responseContent);
                        if (resetPinCountResponse?.responseHeader != null)
                        {
                            if (resetPinCountResponse.responseHeader.StatusCode == 1000)
                            {
                                return resetPinCountResponse.responseHeader.StatusMessage;
                            }
                            else
                            {
                                return $"Error : {resetPinCountResponse.responseHeader.StatusMessage}" +
                                    $" : {resetPinCountResponse.responseHeader.StatusCode}";
                            }
                        }
                        else
                        {
                            return $"Error : Invalid Response from server";
                        }

                    }
                    else
                    {
                        return $"Error : An error occured while processing request";
                    }
                }


            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"METHOD : ResetPinCount \n Error : {ex.ToString()} ");
                return "An Error occured While processing request";
            }
        }

        public static async Task ProcessNewCards(UserInfo usrInfo, List<SoapRequestData> soapRequestDataList)
        {
            string ApplicationId = string.Empty;
            string AccountId = string.Empty;
            var atmUrl = ConfigurationManager.AppSettings["CRDBAtmUrl"];

            foreach (var soapRequestData in soapRequestDataList)
            {
                try
                {
                    ApplicationId = soapRequestData.ApplicationNumber;
                    AccountId = soapRequestData.Customer.Contract.Account.AccountNumber;
                    var soapEnvelope = OmniCore.CreateSoapEnvelope(soapRequestData);

                    CoreOmniApiBase.LogEvent($"METHOD: AtmCardGeneration \n NewCardsProcessing: {soapEnvelope}\n");

                    var response = await OmniCore.ExecuteSoapRequest(soapEnvelope, atmUrl);

                    CoreOmniApiBase.LogEvent($"METHOD: AtmCardGeneration \n RESPONSE: {response}\n");

                    OmniCore.HandleResponse(AccountId, ApplicationId, response, usrInfo);
                }
                catch (WebException ex)
                {
                    try
                    {
                        var errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                        CoreOmniApiBase.LogEvent($"METHOD: AtmCardGeneration \n WEB EXCEPTION: {errorResponse}\n");

                        // Check if the response is XML
                        if (errorResponse.StartsWith("<"))
                        {
                            OmniCore.HandleResponse(AccountId, ApplicationId, errorResponse, usrInfo);
                        }
                        else
                        {
                            CoreOmniApiBase.LogEvent($"Transaction failed for Application {ApplicationId}, Account {AccountId}: {errorResponse}");
                        }
                    }
                    catch (Exception innerEx)
                    {
                        CoreOmniApiBase.LogEvent($"Error reading WebException response: {innerEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: AtmCardGeneration \n EXCEPTION: {ex.Message}\n");
                }
            }
        }

        public static async Task ProcessReplacementCards(UserInfo usrInfo, List<ReplacementRequestXml> soapRequestDataList)
        {
            var atmUrl = ConfigurationManager.AppSettings["CRDBReplacementUrl"];
            var cardNumber = string.Empty;

            foreach (var soapRequestData in soapRequestDataList)
            {
                try
                {
                    cardNumber = soapRequestData.customer.Contract.Card.CardID;
                    var soapEnvelope = OmniCore.CardReplacementEnvelope(soapRequestData);

                    CoreOmniApiBase.LogEvent($"METHOD: ProcessReplacementCards \n RequestEnvelope: {soapEnvelope}\n");

                    var response = await OmniCore.ExecuteSoapRequest(soapEnvelope, atmUrl);

                    CoreOmniApiBase.LogEvent($"METHOD: ProcessReplacementCards \n RESPONSE: {response}\n");

                    OmniCore.HandleReplacementResponse(response, usrInfo,cardNumber);
                }
                catch (WebException ex)
                {
                    try
                    {
                        if (ex.Response != null)
                        {
                            var httpResponse = (HttpWebResponse)ex.Response;

                            using (var stream = httpResponse.GetResponseStream())
                            using (var reader = new StreamReader(stream))
                            {
                                var errorResponse = reader.ReadToEnd();

                                CoreOmniApiBase.LogEvent(
                                    $"METHOD: ProcessReplacementCards \n WEB EXCEPTION (with response): " +
                                    $"HTTP={httpResponse.StatusCode} ({(int)httpResponse.StatusCode}) | WebExceptionStatus={ex.Status} \n" +
                                    $"{errorResponse}\n");

                                if (!string.IsNullOrWhiteSpace(errorResponse) && errorResponse.StartsWith("<"))
                                {
                                    // Try handling SOAP/XML error response
                                    OmniCore.HandleReplacementResponse(errorResponse, usrInfo,cardNumber);
                                }
                                else
                                {
                                    CoreOmniApiBase.LogEvent($"Replacement failed (non-XML response): {errorResponse}");
                                }
                            }
                        }
                        else
                        {
                            // No response (network error, timeout, DNS, TLS issue)
                            CoreOmniApiBase.LogEvent(
                                $"METHOD: ProcessReplacementCards \n WEB EXCEPTION (no response): {ex.Status} - {ex.Message}\n");
                        }
                    }
                    catch (Exception innerEx)
                    {
                        CoreOmniApiBase.LogEvent(
                            $"Error reading WebException response: {innerEx.GetType().Name} - {innerEx.Message}\n" +
                            $"Original WebException: {ex.Status} - {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ProcessReplacementCards \n GENERAL EXCEPTION: {ex.GetType().Name} - {ex.Message}\n");
                }
            }
        }


        public static string ReadWebExceptions(WebException ex)
        {
            using (var stream = ex.Response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    string errorResponse = streamReader.ReadToEnd();
                    CoreOmniApiBase.LogEvent($"Method \n" +
                        $" ReadWebExceptions {errorResponse}");

                    var serializer = new XmlSerializer(typeof(Models.Cards.ResponseDataXml.Envelope));
                    Models.Cards.ResponseDataXml.Envelope envelope;

                    using (StringReader stringReader = new StringReader(errorResponse))
                    {
                        envelope = (Models.Cards.ResponseDataXml.Envelope)serializer.Deserialize(stringReader);

                    }

                    var faultCode = envelope.Body.Fault.FaultCode;
                    var faultString = envelope.Body.Fault.FaultString;
                    var faultDetail = envelope.Body.Fault.Detail?.FaultType?.Text;

                    CoreOmniApiBase.LogEvent($"METHOD: ReadWebExceptions \n SOAP FAULT:" +
                        $" Code={faultCode}, Message={faultString}, Detail={faultDetail}\n");

                    return faultString;

                }
            };

        }

        //END_________________________Cards_____________________________________
        #endregion Cards Logic

        #region Methods for Client/Customer 

        public static string AddEditiBankingNewClientApi(UserInfo usrInfo, string LegacyAccountID, string MobileNumber, BRDataSet dsClientCustomer, string photo, string IdFront, string IdBack, string signature, bool SkipAudit)
        {

            Exception exception;
            string str3;
            AuditInfo info = new AuditInfo
            {
                EventID = BROperation.Add,
                ModuleID = BRModule.Account,
                Status = BRStatus.Successful
            };
            IDBHelper dBHelper = BR.DBClient.DBClient.GetDBHelper(usrInfo);
            string str2 = string.Empty;
            try
            {

                if (photo != "" && photo != null)
                {
                    byte[] photo_ = System.Convert.FromBase64String(photo.ToString());
                    photo = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(photo_));
                }

                if (IdFront != null && IdFront != "")
                {
                    byte[] IdFront_ = System.Convert.FromBase64String(IdFront.ToString());
                    IdFront = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdFront_));
                }

                if (IdBack != null && IdBack != "")
                {
                    byte[] IdBack_ = System.Convert.FromBase64String(IdBack.ToString());
                    IdBack = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdBack_));
                }

                if (signature != null && signature != "")  //
                {
                    byte[] signature_ = System.Convert.FromBase64String(signature.ToString());
                    signature = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(signature_));
                }


                if (!dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Contains("OpenedBy"))
                {
                    dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("OpenedBy", typeof(string));
                    dsClientCustomer.Tables["dt_ClientsMobile"].Rows[0]["OpenedBy"] = usrInfo.strUser;
                }


                if (!dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Contains("CreatedBy"))
                {
                    dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("CreatedBy", typeof(string));
                    dsClientCustomer.Tables["dt_ClientsMobile"].Rows[0]["CreatedBy"] = usrInfo.strUser;
                }

                DataRow dataRow = dsClientCustomer.Tables["dt_ClientsMobile"].Rows[0];
                if (!dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Contains("AccountID"))
                {
                    dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("AccountID", typeof(string));
                    dataRow["AccountID"] = null;
                }
                dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("photo", typeof(string));
                dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("IdFront", typeof(string));
                dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("IdBack", typeof(string));
                dsClientCustomer.Tables["dt_ClientsMobile"].Columns.Add("signature", typeof(string));
                dataRow["NewRecord"] = 1;
                dataRow["photo"] = photo;
                dataRow["IdFront"] = IdFront;
                dataRow["IdBack"] = IdBack;
                dataRow["signature"] = signature;


                string mydata = string.Empty;
                mydata = ConvertDataRowToJson(dataRow);
                string FinalData = string.Empty;
                FinalData = mydata;

                Dictionary<string, string> idictdiffNew = new Dictionary<string, string> {
                    { "RequestID", "Client" },
                    { "Details", FinalData }
                };

                string modifiedJson = JsonConvert.SerializeObject(idictdiffNew, Newtonsoft.Json.Formatting.None);
      
                var jsonObject = JObject.Parse(FinalData);

                string clientType = jsonObject["ClientType"].ToString();

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    //using (IDbTransaction transaction = connection.BeginTransaction())
                    //{
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                        BRDataSet ds_client;
                        ds_client = new BRDataSet();
                        IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
                        arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, int.MaxValue);
                        arParms[0].Value = modifiedJson;
                        arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
                        arParms[1].Value = DateTime.Now.ToString();
                        try
                        {
                            //str2 = (string)dBHelper.ExecuteScalarTypedParams(transaction, "p_AddEditClientsMobile_Indvidual", mydata);//p_AddEditAccountCustomersMobile dataRow
                            //intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditClientsMobile_Indvidual", ds_client, new string[] { "dt_WebServiceStatus", "dt_Client" }, arParms);

                        switch (clientType)
                        {
                            case "I"://"individual":
                                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditClientsMobile_Indvidual", ds_client, new string[] { "dt_WebServiceStatus", "dt_Client" }, arParms);
                                break;
                            case "G": //"group":  //p_AddEditClientsMobile_group
                                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditClientsMobile_Indvidual", ds_client, new string[] { "dt_WebServiceStatus", "dt_Client" }, arParms);
                                break;
                            case "C": //"cooporate":  //p_AddEditClientsMobile_Cooporate
                                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditClientsMobile_Indvidual", ds_client, new string[] { "dt_WebServiceStatus", "dt_Client" }, arParms);
                                break;
                            default:
                                // Handle unexpected clientType values
                                break;
                        }

                        info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_client.Tables[0]);
                        str2 = info.Message;
                        return str2;
                    }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                           // transaction.Rollback();
                            str2 = "Database Error";
                            info.Status = BRStatus.Failed;
                            info.Message = exception.Message;
                            return string.Empty;
                            //throw DBClientUtils.GetDBErrorMessages(exception, usrInfo.strUser, usrInfo.strSystem);
                        }
                   // }
                }

            }
            catch (Exception exception2)
            {
                exception = exception2;
                info.Message = exception2.Message.ToString();
                return string.Empty;
            }
            finally
            {
                dsClientCustomer.Dispose();

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, LegacyAccountID, MobileNumber, "ClientOpening", dsClientCustomer, info.Status.ToString(), JsonConvert.SerializeObject(info.Message).ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "ClientOpening", "RESPONSE", JsonConvert.SerializeObject(info.Message).ToString(), ""));
            }
        }

        public static string ConvertDataRowToJson(DataRow dataRow)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (DataColumn column in dataRow.Table.Columns)
            {
                dictionary[column.ColumnName] = dataRow[column];
            }

            return JsonConvert.SerializeObject(dictionary);
        }

        public static string AddEditiBankingAccountCustomerApi(UserInfo usrInfo,string mydata, BRDataSet dsAccountCustomer)  //string LegacyAccountID, string MobileNumber, BRDataSet dsAccountCustomer, string photo, string IdFront, string IdBack, string signature, bool SkipAudit)
        {

            Exception exception;
            string str3;
            AuditInfo info = new AuditInfo
            {
                EventID = BROperation.Add,
                ModuleID = BRModule.Account,
                Status = BRStatus.Successful
            };
            IDBHelper dBHelper = BR.DBClient.DBClient.GetDBHelper(usrInfo);
            string str2 = string.Empty;
            try
            {

                //if (photo != "" && photo != null)
                //{
                //    byte[] photo_ = System.Convert.FromBase64String(photo.ToString());
                //    photo = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(photo_));
                //}

                //if (IdFront != null && IdFront != "")
                //{
                //    byte[] IdFront_ = System.Convert.FromBase64String(IdFront.ToString());
                //    IdFront = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdFront_));
                //}

                //if (IdBack != null && IdBack != "")
                //{
                //    byte[] IdBack_ = System.Convert.FromBase64String(IdBack.ToString());
                //    IdBack = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdBack_));
                //}

                //if (signature != null && signature != "")  //
                //{
                //    byte[] signature_ = System.Convert.FromBase64String(signature.ToString());
                //    signature = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(signature_));
                //}


                if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("OpenedBy"))
                {
                    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("OpenedBy", typeof(string));
                    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["OpenedBy"] = usrInfo.strUser;
                }


                if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("CreatedBy"))
                {
                    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("CreatedBy", typeof(string));
                    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["CreatedBy"] = usrInfo.strUser;
                }

                DataRow dataRow = dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0];
                if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AccountID"))
                {
                    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("AccountID", typeof(string));
                    dataRow["AccountID"] = null;
                }

                // dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("photo", typeof(string));
                //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdFront", typeof(string));
                //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdBack", typeof(string));
                //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("signature", typeof(string));
                //dataRow["NewRecord"] = 1;
                //dataRow["photo"] = photo;
                //dataRow["IdFront"] = IdFront;
                //dataRow["IdBack"] = IdBack;
                //dataRow["signature"] = signature;
                //string mydata = string.Empty;
                //mydata = ConvertDataRowToJson(dataRow);

                string FinalData = string.Empty;
                FinalData = mydata;

                Dictionary<string, string> idictdiffNew = new Dictionary<string, string> {
                    { "RequestID", "Account" },
                    { "Details", FinalData }
                };

                string modifiedJson = JsonConvert.SerializeObject(idictdiffNew, Newtonsoft.Json.Formatting.None);

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    BRDataSet ds_client;
                    ds_client = new BRDataSet();
                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
                    arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, int.MaxValue);
                    arParms[0].Value = modifiedJson;
                    arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
                    arParms[1].Value = DateTime.Now.ToString();
                    try
                    {
                      intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditAccountCustomers_Mobile", ds_client, new string[] { "dt_WebServiceStatus", "dt_Client" }, arParms);

                        info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_client.Tables[0]);
                        str2 = info.Message;
                        return str2;
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        // transaction.Rollback();
                        str2 = "Database Error";
                        info.Status = BRStatus.Failed;
                        info.Message = exception.Message;
                        return string.Empty;
                        //throw DBClientUtils.GetDBErrorMessages(exception, usrInfo.strUser, usrInfo.strSystem);
                    }
                    // }
                }

            }
            catch (Exception exception2)
            {
                exception = exception2;
                info.Message = exception2.Message.ToString();
                return string.Empty;
            }
            finally
            {
                dsAccountCustomer.Dispose();

               // Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, LegacyAccountID, MobileNumber, "AccountOpening", dsAccountCustomer, info.Status.ToString(), JsonConvert.SerializeObject(info.Message).ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "RESPONSE", JsonConvert.SerializeObject(info.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, /*strMobileNumber*/"", "AccountOpening", dsAccountCustomer, info.Status.ToString(), info.Message.ToString()));
            }
        }



        //public static string AddEditiBankingAccountCustomerApi(UserInfo usrInfo, string MobileNumber, BRDataSet dsAccountCustomer, bool SkipAudit)
        //{

        //    Exception exception;
        //    string str3;
        //    AuditInfo info = new AuditInfo
        //    {
        //        EventID = BROperation.Add,
        //        ModuleID = BRModule.Account,
        //        Status = BRStatus.Successful
        //    };
        //    IDBHelper dBHelper = BR.DBClient.DBClient.GetDBHelper(usrInfo);
        //    string str2 = string.Empty;
        //    try
        //    {

        //        //if (photo != "" && photo != null)
        //        //{
        //        //    byte[] photo_ = System.Convert.FromBase64String(photo.ToString());
        //        //    photo = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(photo_));
        //        //}

        //        //if (IdFront != null && IdFront != "")
        //        //{
        //        //    byte[] IdFront_ = System.Convert.FromBase64String(IdFront.ToString());
        //        //    IdFront = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdFront_));
        //        //}

        //        //if (IdBack != null && IdBack != "")
        //        //{
        //        //    byte[] IdBack_ = System.Convert.FromBase64String(IdBack.ToString());
        //        //    IdBack = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdBack_));
        //        //}

        //        //if (signature != null && signature != "")
        //        //{
        //        //    byte[] signature_ = System.Convert.FromBase64String(signature.ToString());
        //        //    signature = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(signature_));
        //        //}


        //        /*
        //         *   if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("DateOfBirth") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] == null ||
        //            //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] ==string.Empty))            
        //            (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] = DateTime.Now.Date;
        //        }
        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("PassportExpiryDate") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"] == null ||
        //            (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"] = DateTime.Now.Date;
        //        }

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("ValidTo") &&
        //          dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"] == null ||
        //          (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"] = DateTime.Now.Date;
        //        }

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AllowCRTrx") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"].ToString().Equals("0"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"] = false;
        //        }
        //        else
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"] = true;


        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AllowDRTrx") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"].ToString().Equals("0"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = false;
        //        }
        //        else
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = true;

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] == null ||
        //           (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = false;
        //        }
        //        */
        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("ID1"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdentificationTypeID", typeof(string));
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["IdentificationTypeID"] = "ID";

        //        }

        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("OpenedBy"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("OpenedBy", typeof(string));
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["OpenedBy"] = usrInfo.strUser;

        //        }


        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("CreatedBy"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("CreatedBy", typeof(string));
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["CreatedBy"] = usrInfo.strUser;

        //        }


        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("NewRecord", typeof(short));

        //        DataRow dataRow = dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0];
        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AccountID"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("AccountID", typeof(string));
        //            dataRow["AccountID"] = null;
        //        }

        //        //if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("photo"))
        //        //{
        //        //    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("photo", typeof(string));
        //        //    dataRow["photo"] = photo;
        //        //}

        //        //if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("IdFront"))
        //        //{
        //        //    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdFront", typeof(string));
        //        //    dataRow["IdFront"] = IdFront;
        //        //}

        //        //if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("IdBack"))
        //        //{
        //        //    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdBack", typeof(string));
        //        //    dataRow["IdBack"] = IdBack;
        //        //}

        //        //if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("signature"))
        //        //{
        //        //    dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("signature", typeof(string));
        //        //    dataRow["signature"] = signature;
        //        //}

        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("photo", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdFront", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdBack", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("signature", typeof(string));
        //        dataRow["NewRecord"] = 1;
        //        //dataRow["photo"] = photo;
        //        //dataRow["IdFront"] = IdFront;
        //        //dataRow["IdBack"] = IdBack;
        //        //dataRow["signature"] = signature;


        //        using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    str2 = (string)dBHelper.ExecuteScalarTypedParams(transaction, "p_AddEditAccountCustomersMobile", dataRow);
        //                    info.Message = str2;
        //                    transaction.Commit();
        //                    return str2;
        //                }
        //                catch (Exception exception1)
        //                {
        //                    exception = exception1;
        //                    transaction.Rollback();
        //                    str2 = "Database Error";
        //                    info.Status = BRStatus.Failed;
        //                    info.Message = exception.Message;
        //                    return string.Empty;
        //                    //throw DBClientUtils.GetDBErrorMessages(exception, usrInfo.strUser, usrInfo.strSystem);

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception exception2)
        //    {
        //        exception = exception2;
        //        info.Message = exception2.Message.ToString();
        //        return string.Empty;
        //    }
        //    finally
        //    {
        //        dsAccountCustomer.Dispose();

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, ""/*LegacyAccountID*/, MobileNumber, "AccountOpening", dsAccountCustomer, info.Status.ToString(), JsonConvert.SerializeObject(info.Message).ToString()));
        //    }


        //}

        public static string MoneyTransfer(UserInfo usrInfo, string OurBranchID, string AccountID, string Recievername, string RecieverIDNumber, string TrxAmount, string RecieverMobile, string TrxDescriptionID, BRDataSet dsMoneyTransferTrx)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            string strResponse = string.Empty;

            DataRow dataRow = dsMoneyTransferTrx.Tables["dt_MoneyTransfer"].Rows[0];

            var rowDict = dataRow.Table.Columns
           .Cast<DataColumn>()
           .ToDictionary(col => col.ColumnName, col => dataRow[col]);

            // Serialize to JSON
            string jsonString = JsonConvert.SerializeObject(rowDict, Formatting.Indented);

            try
            {

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            strResponse = (string)intfDBHelper.ExecuteScalarTypedParams(transaction, "P_AddMoneyTransfer_Mobile", dataRow);
                            auditInfo.Message = strResponse;
                            transaction.Commit();

                            return strResponse;
                        }
                        catch (Exception exception)
                        {

                            transaction.Rollback();
                            strResponse = "99|Contact Administrator Cannot Continue";
                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = exception.Message;

                            return strResponse;
                        }
                    }
                }
            }
            finally
            {
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MoneyTransfer", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, jsonString, /*dataRow["MobileNumber"].ToString()*/"", "MoneyTransfer", dsMoneyTransferTrx, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                dsMoneyTransferTrx.Dispose();
            }
        }
      
        public static bool GetAccountDetails(UserInfo usrInfo, string mydata, out BRDataSet ds_AccountCustomer)
        {

            AuditInfo info = new AuditInfo
            {
                EventID = BROperation.View,
                ModuleID = BRModule.Account,
                Status = BRStatus.Successful
            };

            string FinalData = string.Empty;

            FinalData = mydata;


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "GetAccountCustomerDetails");
            idictdiffNew.Add("GetAccountCustomerDetails", FinalData);



            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);




            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_AccountCustomer = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 255);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();


            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetAccountCustomerDetail_Mobile", ds_AccountCustomer, new string[] { "dt_WebServiceStatus", "ds_AccountCustomer" }, arParms);
                }


                if (ds_AccountCustomer.Tables.Count >= 2)
                {
                    info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[0]);
                    info.Message = info.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[1]);

                }
                else if (ds_AccountCustomer.Tables.Count >= 1)
                {
                    info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[0]);

                }


                if (ds_AccountCustomer.Tables[0].Rows.Count == 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                info.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                ds_AccountCustomer.Dispose();

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, mydata, "GetAccountDetails", null, info.Status.ToString(), info.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountDetails", "RESPONSE", JsonConvert.SerializeObject(info.Message).ToString(), ""));


                //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
               //ssss Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, /*strMobileNumber*/"", "GetMiniStatement", dsMiniStatement, auditInfo.Status.ToString(), auditInfo.Message.ToString()));

            }

        }

        public static bool GetClientAccountDetails(UserInfo usrInfo, string mydata, out BRDataSet ds_AccountCustomer)
        {

            AuditInfo info = new AuditInfo
            {
                EventID = BROperation.View,
                ModuleID = BRModule.Account,
                Status = BRStatus.Successful
            };

            string FinalData = string.Empty;

            FinalData = mydata;


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "GetAccountDetails");
            idictdiffNew.Add("GetAccountDetails", FinalData);



            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);




            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_AccountCustomer = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 500);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();


            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetCustomerAccounts_Mobile", ds_AccountCustomer, new string[] { "dt_WebServiceStatus", "ds_AccountCustomer" }, arParms);
                }


                if (ds_AccountCustomer.Tables.Count >= 2)
                {
                    info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[0]);
                    info.Message = info.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[1]);

                }
                else if (ds_AccountCustomer.Tables.Count >= 1)
                {
                    info.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountCustomer.Tables[0]);

                }


                if (ds_AccountCustomer.Tables[0].Rows.Count == 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                info.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                ds_AccountCustomer.Dispose();
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, mydata, "GetClientAccountDetails", null, info.Status.ToString(), info.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetClientAccountDetails", "RESPONSE", JsonConvert.SerializeObject(info.Message).ToString(), ""));
            }

        }


        static public bool GetClientByUniqueNumber(UserInfo usrInfo, string mydata, out BRDataSet dsClients)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.Client;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;
            BRDataSet dsSuperVision = new BRDataSet();
            dsClients = new BRDataSet();
            dsSuperVision = new BRDataSet();

            string FinalData = string.Empty;

            FinalData = mydata;

            //string description = string.Empty;
            //description = RemoveSpecialCharacters(FinalData);

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "GetClientDetails");
            idictdiffNew.Add("GetClientDetails", FinalData);

            //string FinalJson = string.Empty;
            //var obj = new { idictdiffNew };

            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(4);

            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 255);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();
            //arParms[1] = intfDBHelper.CreateNewDBParam("IDNumber", SqlDbType.NVarChar, 20);
            //arParms[1].Value = IDNumber;
            //arParms[2] = intfDBHelper.CreateNewDBParam("PhoneNumber", SqlDbType.NVarChar, 20);
            //arParms[2].Value = PhoneNumber;
            //arParms[3] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.NVarChar, 25);
            //arParms[3].Value = AccountID;

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetClientByUniqueNumberMobile", dsClients, new string[] { "GetClientByUniqueNumber" }, arParms);
                }
                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsClients.Tables[0]);

                //if (dsClients == null)
                //    return false;
                //else
                //    return true;
                if (dsClients.Tables[0].Rows.Count == 0)
                {
                    auditInfo.Message = "No Account Found";
                    return false;
                }
                else if (dsClients.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsClients.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsClients.Tables[1]);
                }
                else if (dsClients.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsClients.Tables[0]);
                }
                return true;
            }

            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                ///PassAuditInformation(null, usrInfo, ClientID, string.Empty, auditInfo);
                return false;
                ///throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            finally
            {
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetClientByUniqueNumber", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }

        static public bool GetGroupDetails(UserInfo usrInfo, string mydata, out BRDataSet dsGroup)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.Client;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;
            BRDataSet dsSuperVision = new BRDataSet();
            dsGroup = new BRDataSet();
            dsSuperVision = new BRDataSet();

            string FinalData = string.Empty;
            FinalData = mydata;

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "GroupDetails");
            idictdiffNew.Add("GroupDetails", FinalData);

            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(4);

            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 255);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();


            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetGroupDetails_mobile", dsGroup, new string[] { "GroupDetails" }, arParms);
                }
                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsGroup.Tables[0]);

                if (dsGroup.Tables[0].Rows.Count == 0)
                {
                    auditInfo.Message = "No Account Found";
                    return false;
                }
                else if (dsGroup.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsGroup.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsGroup.Tables[1]);
                }
                else if (dsGroup.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsGroup.Tables[0]);
                }
                return true;
            }

            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                ///PassAuditInformation(null, usrInfo, ClientID, string.Empty, auditInfo);
                return false;
                ///throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            finally
            {
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetGroupDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }

        public static bool GetMiniStatement(UserInfo usrInfo, string mydata, /*string strMobileNumber,*/
            BRDataSet dsMiniStatement, out BRDataSet ds_Ministatement)//, string RecordCount
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.StatementView;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;

            string FinalData = string.Empty;

            FinalData = mydata;


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "MiniStatement");
            idictdiffNew.Add("MiniStatement", FinalData);



            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);



            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_Ministatement = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 255);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();
            //arParms[1] = intfDBHelper.CreateNewDBParam("ClientID", SqlDbType.NVarChar, 30);
            //arParms[1].Value = strClientID;
            //arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.NVarChar, 30);
            //arParms[1].Value = AccountID;
            //arParms[3] = intfDBHelper.CreateNewDBParam("MobileNumber", SqlDbType.NVarChar, 30);
            //arParms[3].Value = strMobileNumber;
            //arParms[3] = intfDBHelper.CreateNewDBParam("FromDate", SqlDbType.SmallDateTime);
            //arParms[3].Value = FromDate;
            //arParms[4] = intfDBHelper.CreateNewDBParam("ToDate", SqlDbType.SmallDateTime);
            //arParms[4].Value = ToDate;
            ////arParms[4] = intfDBHelper.CreateNewDBParam("RecordCount", SqlDbType.NVarChar, 20);
            //arParms[4].Value = RecordCount;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetMiniStatementMobile", ds_Ministatement, new string[] { "dt_WebServiceStatus", "dt_Transactions" }, arParms);
                }



                if (ds_Ministatement.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[1]);

                }
                else if (ds_Ministatement.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]);

                }

                if (ds_Ministatement.Tables[0].Rows.Count == 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                ds_Ministatement.Dispose();
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, /*strMobileNumber*/"", "GetMiniStatement", dsMiniStatement, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
            }

        }
        static string RemoveSpecialCharacters(string input)
        {
            // Define the set of special characters you want to remove
            string specialCharacters = "\\\"";

            // Remove special characters from the input string
            foreach (char c in specialCharacters)
            {
                input = input.Replace(c.ToString(), "'");
            }

            return input;
        }

        public static bool GetAccountBalance(UserInfo usrInfo, string mydata, BRDataSet dsAccountBalance, out BRDataSet ds_AccountBalance)//, string ChargeID
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.CustomerBalance;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;
            string FinalData = string.Empty;

            FinalData = mydata;

            //string description = string.Empty;
            //description = RemoveSpecialCharacters(FinalData);

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "GetAccountBalance");
            idictdiffNew.Add("GetAccountBalance", FinalData);

            //string FinalJson = string.Empty;
            //var obj = new { idictdiffNew };


            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);



            // Serialize the object to JSON
            //   string json = JsonConvert.SerializeObject(obj).ToString();

            //     FinalJson = JsonConvert.SerializeObject(idictdiffNew).ToString();






            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_AccountBalance = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 255);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetAccountBalance_Mobile", ds_AccountBalance, new string[] { "dt_WebServiceStatus", "GetAccountBalance" }, arParms);
                }

                if (ds_AccountBalance.Tables[0].Rows.Count == 0)
                {
                    auditInfo.Message = "No Account Balance";
                    return false;
                }
                else if (ds_AccountBalance.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountBalance.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountBalance.Tables[1]);
                }
                else if (ds_AccountBalance.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountBalance.Tables[0]);
                }


                return true;

            }
            catch (Exception ex)
            {
                auditInfo.Message = ex.Message.ToString();
                return false;
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            finally
            {
                ds_AccountBalance.Dispose();
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountBalance", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, "", "", "GetAccountBalance", dsAccountBalance, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
            }

        }

        static public bool GetCustomerAccountQuery(UserInfo usrInfo, string strAccountID, string strMobileNumber, out BRDataSet dsCustomerQuery)// string ChargeID
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.CustomerQuery;
            auditInfo.Status = BRStatus.Successful;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsCustomerQuery = new BRDataSet();

            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
            arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.NVarChar, 20);
            arParms[1].Value = strAccountID;
            arParms[0] = intfDBHelper.CreateNewDBParam("MobileNumber", SqlDbType.NVarChar, 20);
            arParms[0].Value = strMobileNumber;
            arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
            arParms[2].Value = usrInfo.strUser;

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetCustomerAccountQueryIB", dsCustomerQuery, new string[] { "status", "Customer", "CustomerQuery" }, arParms);

                }

                if (dsCustomerQuery == null)
                    return false;
                else
                {

                    if (dsCustomerQuery.Tables.Count > 1)
                    {
                        auditInfo.Message = auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsCustomerQuery.Tables[0]);
                        auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsCustomerQuery.Tables[0]);

                    }
                    else
                    {
                        auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsCustomerQuery.Tables[0]);

                    }


                    return true;
                }

            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                //PassAuditInformation(null, usrInfo, strAccountID, string.Empty, auditInfo);
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                return false;
            }
            finally
            {
                dsCustomerQuery.Dispose();

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, strAccountID, strMobileNumber, "GetCustomerAccountQuery", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
            }
        }

        //public static string AddEditiBankingAccountCustomerApi(UserInfo usrInfo, string LegacyAccountID, string MobileNumber, BRDataSet dsAccountCustomer, string photo, string IdFront, string IdBack, string signature, bool SkipAudit)
        //{

        //    Exception exception;
        //    string str3;
        //    AuditInfo info = new AuditInfo
        //    {
        //        EventID = BROperation.Add,
        //        ModuleID = BRModule.Account,
        //        Status = BRStatus.Successful
        //    };
        //    IDBHelper dBHelper = BR.DBClient.DBClient.GetDBHelper(usrInfo);
        //    string str2 = string.Empty;
        //    try
        //    {

        //        if (photo != "" && photo != null)
        //        {
        //            byte[] photo_ = System.Convert.FromBase64String(photo.ToString());
        //            photo = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(photo_));
        //        }

        //        if (IdFront != null && IdFront != "")
        //        {
        //            byte[] IdFront_ = System.Convert.FromBase64String(IdFront.ToString());
        //            IdFront = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdFront_));
        //        }

        //        if (IdBack != null && IdBack != "")
        //        {
        //            byte[] IdBack_ = System.Convert.FromBase64String(IdBack.ToString());
        //            IdBack = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(IdBack_));
        //        }

        //        if (signature != null && signature != "")
        //        {
        //            byte[] signature_ = System.Convert.FromBase64String(signature.ToString());
        //            signature = System.Convert.ToBase64String(CoreOmniApiBase.CompressData(signature_));
        //        }


        //        /*
        //         *   if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("DateOfBirth") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] == null ||
        //            //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] ==string.Empty))            
        //            (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] = DateTime.Now.Date;
        //        }
        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("PassportExpiryDate") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"] == null ||
        //            (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["PassportExpiryDate"] = DateTime.Now.Date;
        //        }

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("ValidTo") &&
        //          dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"] == null ||
        //          (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["ValidTo"] = DateTime.Now.Date;
        //        }

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AllowCRTrx") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"].ToString().Equals("0"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"] = false;
        //        }
        //        else
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowCRTrx"] = true;


        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AllowDRTrx") &&
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"].ToString().Equals("0"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = false;
        //        }
        //        else
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = true;

        //        if (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"] == null ||
        //           (dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["DateOfBirth"]).ToString() == string.Empty)
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["AllowDRTrx"] = false;
        //        }
        //        */


        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("OpenedBy"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("OpenedBy", typeof(string));
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["OpenedBy"] = usrInfo.strUser;

        //        }


        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("CreatedBy"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("CreatedBy", typeof(string));
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0]["CreatedBy"] = usrInfo.strUser;

        //        }


        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("NewRecord", typeof(short));

        //        DataRow dataRow = dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Rows[0];
        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("AccountID"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("AccountID", typeof(string));
        //            dataRow["AccountID"] = null;
        //        }

        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("photo"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("photo", typeof(string));
        //            dataRow["photo"] = photo;
        //        }

        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("IdFront"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdFront", typeof(string));
        //            dataRow["IdFront"] = IdFront;
        //        }

        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("IdBack"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdBack", typeof(string));
        //            dataRow["IdBack"] = IdBack;
        //        }

        //        if (!dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Contains("signature"))
        //        {
        //            dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("signature", typeof(string));
        //            dataRow["signature"] = signature;
        //        }

        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("photo", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdFront", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("IdBack", typeof(string));
        //        //dsAccountCustomer.Tables["dt_AccountCustomersMobile"].Columns.Add("signature", typeof(string));
        //        dataRow["NewRecord"] = 1;
        //        //dataRow["photo"] = photo;
        //        //dataRow["IdFront"] = IdFront;
        //        //dataRow["IdBack"] = IdBack;
        //        //dataRow["signature"] = signature;


        //        using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    str2 = (string)dBHelper.ExecuteScalarTypedParams(transaction, "p_AddEditAccountCustomersMobile", dataRow);
        //                    info.Message = str2;
        //                    transaction.Commit();
        //                    return str2;
        //                }
        //                catch (Exception exception1)
        //                {
        //                    exception = exception1;
        //                    transaction.Rollback();
        //                    str2 = "Database Error";
        //                    info.Status = BRStatus.Failed;
        //                    info.Message = exception.Message;
        //                    return string.Empty;
        //                    //throw DBClientUtils.GetDBErrorMessages(exception, usrInfo.strUser, usrInfo.strSystem);

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception exception2)
        //    {
        //        exception = exception2;
        //        info.Message = exception2.Message.ToString();
        //        return string.Empty;
        //    }
        //    finally
        //    {
        //        dsAccountCustomer.Dispose();

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, LegacyAccountID, MobileNumber, "AccountOpening", dsAccountCustomer, info.Status.ToString(), JsonConvert.SerializeObject(info.Message).ToString()));
        //    }


        //}


        #endregion Methods for Client/Customer


        #region Loan On Phone

        static public string LoanApplicationDetails(UserInfo usrInfo, string mydata, BRDataSet dsLoanApp)  //string strAccountID, string strMobileNumber, string loantype,
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.Add;
            auditInfo.ModuleID = BRModule.WFLoanAppraisals;
            string strLOPMessage = string.Empty;
            DataRow drLoanApplicationDetailsRequest = dsLoanApp.Tables[0].Rows[0];

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "LoanApplication");
            idictdiffNew.Add("Details", mydata);

            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);
            var jsonObject = JObject.Parse(mydata);
            string repaymentid = jsonObject["RepaymentAccountID"].ToString();
            string mobilenumber = jsonObject["MobileNumber"].ToString();


            try
            {
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                
                    using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                    {
                    //IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);  
                        BRDataSet ds_LoanApp;
                        ds_LoanApp = new BRDataSet();
                        IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
                        arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, int.MaxValue);
                        arParms[0].Value = modifiedJson;
                        arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
                        arParms[1].Value = DateTime.Now.ToString();
                        //using (IDbTransaction trans = connection.BeginTransaction())
                        {
                        try
                        {  // p_AddEditWFLoanApplicationsMobile  p_LOPRequest
                        //    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_LOPRequest", drLoanApplicationDetailsRequest);
                        //    ///strLOPMessage = "Loan Application Successful";
                        //    trans.Commit();
                        //    auditInfo.Status = BRStatus.Successful;
                        //    auditInfo.Message = strLOPMessage;

                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditWFLoanApplicationsMobile", ds_LoanApp, new string[] { "dt_WebServiceStatus", "dt_LoanApplication" }, arParms);
                        auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_LoanApp.Tables[0]);
                        strLOPMessage = auditInfo.Message;
       
                            return strLOPMessage;
                        }
                        catch (Exception ex)
                        {
                            //trans.Rollback();
                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = ex.Message;
                            return string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                return string.Empty;
            }
            finally
            {

                //Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, strAccountID, strMobileNumber, "LoanApplicationDetails", dsLoanApp, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                //Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, repaymentid, mobilenumber, "LoanApplicationDetails", dsLoanApp, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanApplicationDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));

            }
        }
        static public string LoanApplicationDisbursementRequest(UserInfo usrInfo, string strAccountID, string strMobileNumber, BRDataSet dsLoanApp)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.Add;
            auditInfo.ModuleID = BRModule.LoanDisbursement;
            string strLOPMessage = string.Empty;

            try
            {
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);


                if (!dsLoanApp.Tables["dt_LoanApplicationdisbursement"].Columns.Contains("BR_CheckSum"))
                {
                    dsLoanApp.Tables["dt_LoanApplicationdisbursement"].Columns.Add("BR_CheckSum", typeof(string));
                    dsLoanApp.Tables["dt_LoanApplicationdisbursement"].Rows[0]["BR_CheckSum"] = "0";

                }

                DataRow drLoanApplicationDetailsRequest = dsLoanApp.Tables[0].Rows[0];


                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction trans = connection.BeginTransaction())
                    {
                        try
                        {
                            strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_LoanProcess_Mobile", drLoanApplicationDetailsRequest);


                            ///strLOPMessage = "Loan Application Successful";

                            trans.Commit();

                            auditInfo.Status = BRStatus.Successful;
                            auditInfo.Message = strLOPMessage;

                            return strLOPMessage;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();



                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = ex.Message;
                            return string.Empty;
                        }
                    }
                }


            }
            catch (Exception ex)
            {

                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                return string.Empty;
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, strAccountID, strMobileNumber, "LoanApplicationDisbursementRequest", dsLoanApp, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
            }
        }
        static public bool LoanBalanceDetails(UserInfo usrInfo, string RepaymentAccountID, string MobileNumber, string LoanProductID, BRDataSet ds_LoanBal, out BRDataSet dsLoanOnPhoneBalance)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.Loan;
            string RequestParameter = string.Empty;
            string OperatorID = string.Empty;
            string UniqueRefID = string.Empty;

            dsLoanOnPhoneBalance = new BRDataSet();

            try
            {

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(6);
                //RequestParameter = string.Format("Mobile:{0} CenteAccountID{1} LoanAmount{2} LoanTerm{3} UniqueRefID{4}", MobileNumber, CenteAccountID, LoanAmount.ToString(), LoanTerm.ToString(), UniqueRefID);

                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 5);
                arParms[0].Value = usrInfo.strBranch.ToString();
                arParms[1] = intfDBHelper.CreateNewDBParam("MobileNumber", SqlDbType.NVarChar, 25);
                arParms[1].Value = MobileNumber;
                arParms[2] = intfDBHelper.CreateNewDBParam("RepaymentAccountID", SqlDbType.NVarChar, 50);
                arParms[2].Value = RepaymentAccountID;
                arParms[3] = intfDBHelper.CreateNewDBParam("LoanProductID", SqlDbType.NVarChar, 50);
                arParms[3].Value = LoanProductID;
                arParms[4] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[4].Value = usrInfo.strUser.ToString();
                arParms[5] = intfDBHelper.CreateNewDBParam("UniqueRefID", SqlDbType.NVarChar, 1000);
                arParms[5].Value = UniqueRefID;


                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_LoanOnPhoneBalance", dsLoanOnPhoneBalance, new string[] { "dt_LoanBalanceDetail" }, arParms);
                    auditInfo.Status = BRStatus.Successful;
                    auditInfo.Message = dsLoanOnPhoneBalance.GetXml().ToString();

                }

                if (dsLoanOnPhoneBalance != null && dsLoanOnPhoneBalance.Tables.Count > 0 && dsLoanOnPhoneBalance.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {

                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                return false;
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, RepaymentAccountID, MobileNumber, "LoanBalanceDetails", ds_LoanBal, auditInfo.Status.ToString(), auditInfo.Message.ToString()));

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanBalanceDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));

                dsLoanOnPhoneBalance.Dispose();
            }
        }
        static public bool LoanLimitDetails(UserInfo usrInfo, string RepaymentAccountID, string MobileNumber, BRDataSet ds_Limit, out BRDataSet dsLoanLimit)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.Loan;
            string RequestParameter = string.Empty;

            dsLoanLimit = new BRDataSet();

            try
            {

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
                //RequestParameter = string.Format("Mobile:{0} CenteAccountID{1} LoanAmount{2} LoanTerm{3} UniqueRefID{4}", MobileNumber, CenteAccountID, LoanAmount.ToString(), LoanTerm.ToString(), UniqueRefID);

                arParms[0] = intfDBHelper.CreateNewDBParam("MobileNumber", SqlDbType.NVarChar, 25);
                arParms[0].Value = MobileNumber;
                arParms[1] = intfDBHelper.CreateNewDBParam("RepaymentAccountID", SqlDbType.NVarChar, 50);
                arParms[1].Value = RepaymentAccountID;

                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_LoanOnPhoneLimit", dsLoanLimit, new string[] { "dt_LoanLimit" }, arParms);
                    auditInfo.Status = BRStatus.Successful;
                    auditInfo.Message = dsLoanLimit.GetXml().ToString();

                }
                if (dsLoanLimit != null && dsLoanLimit.Tables.Count > 0 && dsLoanLimit.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {

                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                return false;
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, RepaymentAccountID, MobileNumber, "LoanLimitDetails", ds_Limit, auditInfo.Status.ToString(), auditInfo.Message.ToString()));

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanLimitDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));


                dsLoanLimit.Dispose();
            }
        }

        static public string AddCollateralDetails(UserInfo usrInfo, string mydata, BRDataSet dsCollateralApp)  //string strAccountID, string strMobileNumber, string loantype,
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.Add;
            auditInfo.ModuleID = BRModule.Collateral;
            string strLOPMessage = string.Empty;
            DataRow drCollateralRequest = dsCollateralApp.Tables[0].Rows[0];

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "CollateralDetails");
            idictdiffNew.Add("CollateralDetails", mydata);

            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);
            // var jsonObject = JObject.Parse(mydata);
            // string repaymentid = jsonObject["RepaymentAccountID"].ToString();
            // string mobilenumber = jsonObject["MobileNumber"].ToString();

            try
            {
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    //IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);  
                    BRDataSet ds_CollateralApp;
                    ds_CollateralApp = new BRDataSet();
                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
                    arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, int.MaxValue);
                    arParms[0].Value = modifiedJson;
                    arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
                    arParms[1].Value = DateTime.Now.ToString();
                    //using (IDbTransaction trans = connection.BeginTransaction())
                    {
                        try
                        {
                            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditCollaterals_Mobile", ds_CollateralApp, new string[] { "dt_WebServiceStatus", "dt_CollateralApplication" }, arParms);
                            auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_CollateralApp.Tables[0]);
                            strLOPMessage = auditInfo.Message;
                            return strLOPMessage;
                        }
                        catch (Exception ex)
                        {
                            //trans.Rollback();
                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = ex.Message;
                            return string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                return string.Empty;
            }
            finally
            {
                //Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, strAccountID, strMobileNumber, "LoanApplicationDetails", dsLoanApp, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                //Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, repaymentid, mobilenumber, "LoanApplicationDetails", dsLoanApp, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "CollateralApplicationDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));

            }
        }

        //static private string LoanOnPhonePaymentDetails(UserInfo usrInfo, string AccountID, string MobileNumber, DataRow drLoanOnPhoneRequest)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanPayoff;
        //    string strLOPMessage = string.Empty;

        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_LoanOnPhonePayoff", drLoanOnPhoneRequest);


        //                    ///strLOPMessage = "Loan Application Successful";

        //                    trans.Commit();

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;

        //                    return strLOPMessage;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return string.Empty;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return string.Empty;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanPaymentRequest", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));

        //    }
        //}

        #endregion Loan On Phone

        #region Transactions methods

        public static string LoanRepayment(UserInfo usrInfo, BRDataSet ds_LoanRepayment)//, string ChargeID
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;


            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ///    ds_InternalTransfer = new BRDataSet();
            string strResponse = string.Empty;

            DataRow dataRow = ds_LoanRepayment.Tables["dt_Transactions"].Rows[0];


            try
            {

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            strResponse = (string)intfDBHelper.ExecuteScalarTypedParams(transaction, "p_LoanOnPhonePayoff", dataRow);
                            auditInfo.Message = strResponse;
                            transaction.Commit();

                            return strResponse;
                        }
                        catch (Exception exception)
                        {

                            transaction.Rollback();
                            strResponse = "99|Contact Administrator Cannot Continue";
                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = exception.Message;

                            return strResponse;
                        }
                    }
                }
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "LoanRepayment", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, dataRow["RepaymentAccountID"].ToString(), dataRow["MobileNumber"].ToString(), "LoanRepayment", ds_LoanRepayment, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                ds_LoanRepayment.Dispose();
            }
            /*
            using (IDbConnection connection = DBClient.GetConnection(usrInfo))
            {
                IDataParameter[] arParms = intfDBHelper.GetSPParameters(connection, sp);
                object paramValue = null;
                foreach (IDataParameter param in arParms)
                {
                    paramValue = null;
                    switch (param.ParameterName.Substring(1).ToUpper())
                    {
                        case "OPERATORID":
                            paramValue = usrInfo.strUser.ToString();
                            break;
                        case "OURBRANCHID":
                            paramValue = OurBranchID;
                            break;
                        case "FROMACCOUNTID":
                            paramValue = FromAccountID;
                            break;
                        case "TOBRANCHID":
                            paramValue = ToBranchID;
                            break;
                        case "TOACCOUNTID":
                            paramValue = ToAccountID;
                            break;
                        case "TRXAMOUNT":
                            paramValue = TrxAmount;
                            break;
                        case "CRTRANSACTIONDESCRIPTION":
                            paramValue = crTransactionDescription;
                            break;
                        case "DRTRANSACTIONDESCRIPTION":
                            paramValue = drTransactionDescription;
                            break;
                        case "TRXDESCRIPTIONID":
                            paramValue = TrxDescriptionID;
                            break;
                        case "TRXTRACENO":
                            paramValue = TrxTraceNo;
                            break;
                        case "MOBILENO":
                            paramValue = MobileNo;
                            break;
                        case "EXTRADATA":
                            paramValue = ExtraData;
                            break;
                    }
                    param.Value = paramValue;
                }

                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, sp, ds_InternalTransfer, new string[] { "InternalTransfer" }, arParms);
            }
            auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_InternalTransfer.Tables[0]);
            if (ds_InternalTransfer.Tables[0].Rows.Count == 0)
                return false;
            return true;
                 }
        catch (Exception ex)
        {
            auditInfo.Message = ex.Message.ToString();
            //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            return false;      
        }
            */



        }

        public static bool InternalMoneyTransfer(UserInfo usrInfo, string mydata, out BRDataSet dsInternalTrx)
        {


            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.StatementView;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;

            string FinalData = string.Empty;

            FinalData = mydata;

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(FinalData);

            // Extract individual values
            string fromAccountID = data["FromAccountID"];
            string toAccountID = data["ToAccountID"];


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "ChannelInternalTransfer");
            idictdiffNew.Add("ChannelInternalTransfer", FinalData);



            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);



            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsInternalTrx = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 500);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_ChannelInternalTransfer_Mobile", dsInternalTrx, new string[] { "dt_WebServiceStatus", "dt_Transactions" }, arParms);
                }



                if (dsInternalTrx.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[1]);

                }
                else if (dsInternalTrx.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);

                }

                if (dsInternalTrx.Tables[0].Rows.Count == 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                dsInternalTrx.Dispose();
               // Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, mydata, "GetAccountDetails", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, null, null, "InternalTranfer", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "InternalTranfer", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }

        public static bool InternalTransfer(UserInfo usrInfo, string mydata, out BRDataSet dsInternalTrx)
        {


            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.StatementView;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;

            string FinalData = string.Empty;

            FinalData = mydata;

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(FinalData);

            // Extract individual values
            string channelID = data["ChannelID"];
            string serviceID = data["ServiceID"];
            string requestID = data["RequestID"];
            string operatorID = data["OperatorID"];
            string fromAccountID = data["FromAccountID"];
            string toAccountID = data["ToAccountID"];
            string trxAmount = data["TrxAmount"];


            BRDataSet dataSetTrx = new BRDataSet();

            // Create a DataTable
            DataTable table = new DataTable("TransactionDetails");

            // Define columns for the DataTable
            table.Columns.Add("ChannelID", typeof(string));
            table.Columns.Add("ServiceID", typeof(string));
            table.Columns.Add("RequestID", typeof(string));
            table.Columns.Add("OperatorID", typeof(string));
            table.Columns.Add("FromAccountID", typeof(string));
            table.Columns.Add("ToAccountID", typeof(string));
            table.Columns.Add("TrxAmount", typeof(string));

            // Add a row to the DataTable
            table.Rows.Add(channelID, serviceID, requestID, operatorID, fromAccountID, toAccountID, trxAmount);

            // Add the DataTable to the DataSet
            dataSetTrx.Tables.Add(table);


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "ChannelInternalTransfer");
            idictdiffNew.Add("ChannelInternalTransfer", FinalData);



            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);



            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsInternalTrx = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 500);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_ChannelInternalTransfer_Mobile", dsInternalTrx, new string[] { "dt_WebServiceStatus", "dt_Transactions" }, arParms);
                }



                if (dsInternalTrx.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[1]);

                }
                else if (dsInternalTrx.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);

                }

                if (dsInternalTrx.Tables[0].Rows.Count == 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                dsInternalTrx.Dispose();
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, fromAccountID, toAccountID, "Utilities", dataSetTrx, auditInfo.Status.ToString(), auditInfo.Message.ToString()));

               // Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, mydata, "GetAccountDetails", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountDetails", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }


        public static bool UtilitiesMoneyTransfer(UserInfo usrInfo, string mydata, out BRDataSet dsInternalTrx)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.StatementView;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;

            string FinalData = string.Empty;

            FinalData = mydata;

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(FinalData);

            // Extract individual values
            string channelID = data["ChannelID"];
            string serviceID = data["ServiceID"];
            string requestID = data["RequestID"];
            string operatorID = data["OperatorID"];
            string fromAccountID = data["FromAccountID"];
            string toAccountID = data["ToAccountID"];
            string trxAmount = data["TrxAmount"];

            BRDataSet dataSetTrx = new BRDataSet();

            // Create a DataTable
            DataTable table = new DataTable("TransactionDetails");

            // Define columns for the DataTable
            table.Columns.Add("ChannelID", typeof(string));
            table.Columns.Add("ServiceID", typeof(string));
            table.Columns.Add("RequestID", typeof(string));
            table.Columns.Add("OperatorID", typeof(string));
            table.Columns.Add("FromAccountID", typeof(string));
            table.Columns.Add("ToAccountID", typeof(string));
            table.Columns.Add("TrxAmount", typeof(string));

            // Add a row to the DataTable
            table.Rows.Add(channelID, serviceID, requestID, operatorID, fromAccountID, toAccountID, trxAmount);

            // Add the DataTable to the DataSet
            dataSetTrx.Tables.Add(table);


            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "ChannelInternalTransfer");
            idictdiffNew.Add("ChannelInternalTransfer", FinalData); 




            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);



            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsInternalTrx = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);
            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 500);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_ChannelUtilitiesMoneyTransfer_Mobile", dsInternalTrx, new string[] { "dt_WebServiceStatus", "dt_Transactions" }, arParms);
                }



                if (dsInternalTrx.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[1]);

                }
                else if (dsInternalTrx.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsInternalTrx.Tables[0]);

                }

                if (dsInternalTrx.Tables[0].Rows.Count == 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                dsInternalTrx.Dispose();
            

                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, fromAccountID, toAccountID, "Utilities", dataSetTrx, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
               // Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, mydata, "Utilities", null, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "Utilities", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
           
            }
        }

        //public static string UtilitiesMoneyTransfer(UserInfo usrInfo, string mydata, BRDataSet dsInternalTrx)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.TransferLocal;
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.Status = BRStatus.Successful;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strResponse = string.Empty;

        //    DataRow dataRow = dsInternalTrx.Tables["dt_InternalTransfer"].Rows[0];


        //    try
        //    {

        //        using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strResponse = (string)intfDBHelper.ExecuteScalarTypedParams(transaction, "p_ChannelUtilitiesMoneyTransfer_Mobile", dataRow);
        //                    auditInfo.Message = strResponse;
        //                    transaction.Commit();

        //                    return strResponse;
        //                }
        //                catch (Exception exception)
        //                {

        //                    transaction.Rollback();
        //                    strResponse = "99|Contact Administrator Cannot Continue";
        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = exception.Message;

        //                    return strResponse;
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "InternalTransfer", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, mydata, /*dataRow["MobileNumber"].ToString()*/"", "InternalTransfer", dsInternalTrx, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        dsInternalTrx.Dispose();
        //    }
        //}

        public static bool TransactionReversal(UserInfo usrInfo, string mydata, out BRDataSet dsTrxReversal)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;

            string FinalData = string.Empty;
            FinalData = mydata;

            Dictionary<string, string> idictdiffNew = new Dictionary<string, string>();
            idictdiffNew.Add("RequestID", "ChannelInternalTransferReversal");
            idictdiffNew.Add("ChannelInternalTransferReversal", FinalData);

            string modifiedJson = JsonConvert.SerializeObject(idictdiffNew);

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsTrxReversal = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

            arParms[0] = intfDBHelper.CreateNewDBParam("DataJSON", SqlDbType.NVarChar, 500);
            arParms[0].Value = modifiedJson;
            arParms[1] = intfDBHelper.CreateNewDBParam("DateTime", SqlDbType.SmallDateTime);
            arParms[1].Value = DateTime.Now.ToString();



            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_ChannelInternalTransferReversal_Mobile", dsTrxReversal, new string[] { "dt_WebServiceStatus", "dt_Transactions" }, arParms);
                }


                if (dsTrxReversal.Tables.Count >= 2)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsTrxReversal.Tables[0]);
                    auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(dsTrxReversal.Tables[1]);

                }

                else if (dsTrxReversal.Tables.Count >= 1)
                {
                    auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsTrxReversal.Tables[0]);

                }
                if (dsTrxReversal.Tables[0].Rows.Count == 0)
                {
                    return false;
                }

                return true;
            }

            catch (Exception ex)
            {
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "TransactionReversal", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                dsTrxReversal.Dispose();
            }
        }


        public static string transactionCallback(UserInfo usrInfo, string UniqueID, string PTN, BRDataSet dsTrxReversal)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.TransferLocal;
            auditInfo.EventID = BROperation.Add;
            auditInfo.Status = BRStatus.Successful;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            string strResponse = string.Empty;

            //DataRow dataRow = dsTrxReversal.Tables["dt_TrxReversal"].Rows[0];
            DataRow dataRow = dsTrxReversal.Tables["dt_TransactionCallback"].Rows[0];

            try
            {

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            strResponse = (string)intfDBHelper.ExecuteScalarTypedParams(transaction, "p_ChannelTransactionCallback_Mobile", dataRow);
                            auditInfo.Message = strResponse;
                            transaction.Commit();

                            return strResponse;
                        }
                        catch (Exception exception)
                        {

                            transaction.Rollback();
                            strResponse = "99|Contact Administrator Cannot Continue";
                            auditInfo.Status = BRStatus.Failed;
                            auditInfo.Message = exception.Message;

                            return strResponse;
                        }
                    }
                }
            }
            finally
            {

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "TransactionReversal", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, UniqueID, /*dataRow["MobileNumber"].ToString()*/"", "InternalTransfer", dsTrxReversal, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
                dsTrxReversal.Dispose();
            }
        }
        #endregion Transactions methods

        #region product methods
        public static bool GetProductList(UserInfo usrInfo, BRDataSet dsProduct, out BRDataSet ds_ProductList)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.Products;
            auditInfo.Status = BRStatus.Successful;

            string strRefNo = string.Empty;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_ProductList = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(0);
            //arParms[0] = intfDBHelper.CreateNewDBParam("ProductID", SqlDbType.NVarChar, 20);
            //arParms[0].Value = ProductID;
            //arParms[1] = intfDBHelper.CreateNewDBParam("CurrencyID", SqlDbType.NVarChar, 6);
            //arParms[1].Value = CurrencyID;
            //arParms[2] = intfDBHelper.CreateNewDBParam("ProductTypeID", SqlDbType.NVarChar, 20);
            //arParms[2].Value = ProductTypeID;


            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetMobileProductList", ds_ProductList, new string[] { "dt_WebServiceStatus", "GetProductList" }, arParms);
                }

                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_ProductList.Tables[0]);
                auditInfo.Message = auditInfo.Message + CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_ProductList.Tables[1]);

                if (ds_ProductList.Tables[0].Rows.Count == 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                ds_ProductList.Dispose();
                Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Product List \n Status: {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetProductList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, ""/*AccountID*/, ""/*MobileNumber*/, "GetProductList", dsProduct, auditInfo.Status.ToString(), JsonConvert.SerializeObject(auditInfo.Message).ToString()));
            }
        }

        #endregion product methods

        //#region Thirdparty Methods
        //static public bool GetPendingDisbursementList(UserInfo usrInfo, out BRDataSet dsDisburseFundTransfer)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.TransferLocal;
        //    auditInfo.EventID = BROperation.View;

        //    //BRDataSet dsDisburseFundTransfer = null;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strNewInfo = string.Empty;
        //    string strLOPMessage = string.Empty;


        //    try
        //    {
        //        dsDisburseFundTransfer = new BRDataSet();
        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {

        //            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

        //            arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
        //            arParms[0].Value = usrInfo.strBranch;


        //            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_PendingDisbursement", dsDisburseFundTransfer, new string[] { "dt_PendingDisbursement" }, arParms);



        //            if (dsDisburseFundTransfer.Tables[0].Rows.Count > 0)
        //            {
        //                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsDisburseFundTransfer.Tables[0]);
        //            }
        //            else
        //            {
        //                auditInfo.Message = "No Pending Disbursement List";
        //            }



        //            auditInfo.Status = BRStatus.Successful;




        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        dsDisburseFundTransfer = null;
        //        return false;
        //    }
        //    finally
        //    {
        //        Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending Disbursement List: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
        //        //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //    }
        //}
        //static public bool BRDisbursementValidation(UserInfo usrInfo, BRDataSet dsDisburseValidation)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanDisbursement;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drLoanDisbursementValidation = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drLoanDisbursementValidation = dsDisburseValidation.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AccountDisbursementValidation", drLoanDisbursementValidation);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drLoanDisbursementValidation["AccountID"].ToString(), drLoanDisbursementValidation["MobileNumber"].ToString(), "BRDisbursementValidation", dsDisburseValidation, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}
        //static public bool BRAccountDisbursementTransfer(UserInfo usrInfo, BRDataSet dsAccountDisburseTransactions)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanDisbursement;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drAccountDisburseTransactions = null;
        //    string[] strTransactionDetailsResponse = null;

        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drAccountDisburseTransactions = dsAccountDisburseTransactions.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AccountDisburseTransfer", drAccountDisburseTransactions);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);
        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drAccountDisburseTransactions["AccountID"].ToString(), drAccountDisburseTransactions["MobileNumber"].ToString(), "BRAccountDisbursementTransfer", dsAccountDisburseTransactions, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}

        //static public bool BRAccountDisbursementRetries(UserInfo usrInfo, BRDataSet dsAccountDisbursementRetries)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanDisbursement;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drAccountDisbursementRetries = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drAccountDisbursementRetries = dsAccountDisbursementRetries.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_PendingAccountDisbursementRetries", drAccountDisbursementRetries);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drAccountDisbursementRetries["AccountID"].ToString(), drAccountDisbursementRetries["MobileNumber"].ToString(), "BRAccountDisbursementRetries", dsAccountDisbursementRetries, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}
        //static public bool BRAccountDisbursementExceptionError(UserInfo usrInfo, BRDataSet dsAccountDisbursementError)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanDisbursement;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drAccountDisbursementError = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drAccountDisbursementError = dsAccountDisbursementError.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AccountDisbursementExceptionError", drAccountDisbursementError);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drAccountDisbursementError["AccountID"].ToString(), drAccountDisbursementError["MobileNumber"].ToString(), "BRAccountDisbursementExceptionError//", dsAccountDisbursementError, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}
        //static public bool GetPendingCRBList(UserInfo usrInfo, out BRDataSet dsPendingCRBList)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.WFCRBCheck;
        //    auditInfo.EventID = BROperation.View;

        //    //BRDataSet dsDisburseFundTransfer = null;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strNewInfo = string.Empty;
        //    string strLOPMessage = string.Empty;


        //    try
        //    {
        //        dsPendingCRBList = new BRDataSet();
        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {

        //            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

        //            arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
        //            arParms[0].Value = usrInfo.strBranch;


        //            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_RetrievePendingEnquiry", dsPendingCRBList, new string[] { "dt_PendingCRBList" }, arParms);



        //            if (dsPendingCRBList.Tables[0].Rows.Count > 0)
        //            {
        //                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsPendingCRBList.Tables[0]);
        //            }
        //            else
        //            {
        //                auditInfo.Message = "No Pending CRB List";
        //            }



        //            auditInfo.Status = BRStatus.Successful;




        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        dsPendingCRBList = null;
        //        return false;
        //    }
        //    finally
        //    {
        //        Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending CRB List: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
        //        //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //    }
        //}
        //static public bool BRUpdateAccountCRBDetails(UserInfo usrInfo, BRDataSet dsAccountCRBDetails)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.WFCRBCheck;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drAccountCRBDetails = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drAccountCRBDetails = dsAccountCRBDetails.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_UpdateAccountCRBDetails", drAccountCRBDetails);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drAccountCRBDetails["AccountID"].ToString(), drAccountCRBDetails["MobileNumber"].ToString(), "BRAccountDisbursementRetries", dsAccountCRBDetails, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}
        //static public bool GetPendingSMSNotifications(UserInfo usrInfo, out BRDataSet dsPendingSMSNotifications)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.NotificationMessages;
        //    auditInfo.EventID = BROperation.View;

        //    //BRDataSet dsDisburseFundTransfer = null;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strNewInfo = string.Empty;
        //    string strLOPMessage = string.Empty;


        //    try
        //    {
        //        dsPendingSMSNotifications = new BRDataSet();
        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {

        //            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(0);

        //            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetSMS", dsPendingSMSNotifications, new string[] { "dt_PendingSMSNotifications" }, arParms);



        //            if (dsPendingSMSNotifications.Tables[0].Rows.Count > 0)
        //            {
        //                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsPendingSMSNotifications.Tables[0]);
        //            }
        //            else
        //            {
        //                auditInfo.Message = "No Pending SMS Notifications";
        //            }



        //            auditInfo.Status = BRStatus.Successful;




        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        dsPendingSMSNotifications = null;
        //        return false;
        //    }
        //    finally
        //    {
        //        Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending SMS Notifications: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
        //        //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //    }
        //}
        //static public bool GetPendingEmailNotifications(UserInfo usrInfo, out BRDataSet dsPendingEmailNotifications)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.NotificationMessages;
        //    auditInfo.EventID = BROperation.View;

        //    //BRDataSet dsDisburseFundTransfer = null;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strNewInfo = string.Empty;
        //    string strLOPMessage = string.Empty;


        //    try
        //    {
        //        dsPendingEmailNotifications = new BRDataSet();
        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {

        //            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(0);



        //            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetEmail", dsPendingEmailNotifications, new string[] { "dt_PendingEmailNotifications" }, arParms);



        //            if (dsPendingEmailNotifications.Tables[0].Rows.Count > 0)
        //            {
        //                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(dsPendingEmailNotifications.Tables[0]);
        //            }
        //            else
        //            {
        //                auditInfo.Message = "No Pending Emails Notifications";
        //            }



        //            auditInfo.Status = BRStatus.Successful;




        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        dsPendingEmailNotifications = null;
        //        return false;
        //    }
        //    finally
        //    {
        //        Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending Email Notifications: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
        //        //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //    }
        //}

        //static public bool BRUpdateNotificationStatus(UserInfo usrInfo, BRDataSet dsNotificationStatus)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.NotificationMessages;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drNotificationStatus = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drNotificationStatus = dsNotificationStatus.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_UpdateNotificationsStatus", drNotificationStatus);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drNotificationStatus["AccountID"].ToString(), drNotificationStatus["MobileNumber"].ToString(), "BRUpdateNotificationStatus", dsNotificationStatus, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}

        //static public bool BRUpdateNotificationRetries(UserInfo usrInfo, BRDataSet dsNotificationRetries)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Edit;
        //    auditInfo.ModuleID = BRModule.NotificationMessages;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drNotificationRetries = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drNotificationRetries = dsNotificationRetries.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_UpdateNotificationsRetries", drNotificationRetries);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drNotificationRetries["AccountID"].ToString(), drNotificationRetries["MobileNumber"].ToString(), "BRUpdateNotificationRetries", dsNotificationRetries, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}

        //static public bool GetPendingLoanRecoveryList(UserInfo usrInfo, out BRDataSet ds_PendingLoanRecoveryList)
        //{

        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.ModuleID = BRModule.TransferLocal;
        //    auditInfo.EventID = BROperation.View;

        //    //BRDataSet dsDisburseFundTransfer = null;

        //    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
        //    string strNewInfo = string.Empty;
        //    string strLOPMessage = string.Empty;


        //    try
        //    {
        //        ds_PendingLoanRecoveryList = new BRDataSet();
        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {

        //            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(2);

        //            arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 25);
        //            arParms[0].Value = usrInfo.strBranch;


        //            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetPendingLoanRecovery", ds_PendingLoanRecoveryList, new string[] { "dt_PendingLoanRecoveryList" }, arParms);



        //            if (ds_PendingLoanRecoveryList.Tables[0].Rows.Count > 0)
        //            {
        //                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_PendingLoanRecoveryList.Tables[0]);
        //            }
        //            else
        //            {
        //                auditInfo.Message = "No Pending Loan Recovery";
        //            }



        //            auditInfo.Status = BRStatus.Successful;




        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        ds_PendingLoanRecoveryList = null;
        //        return false;
        //    }
        //    finally
        //    {
        //        Task.Run(() => CoreOmniApiBase.LogEvent(string.Format("Get Pending Loan Recovery List: \n {0}. \n Message: {1};", auditInfo.Status, auditInfo.Message)));
        //        //   Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetPendingDisbursementList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
        //    }
        //}

        //static public bool BRAccountLoanRecoveryRetries(UserInfo usrInfo, BRDataSet dsAccountLoanRecoveryRetries)
        //{
        //    AuditInfo auditInfo = new AuditInfo();
        //    auditInfo.EventID = BROperation.Add;
        //    auditInfo.ModuleID = BRModule.LoanPayoff;
        //    string strLOPMessage = string.Empty;
        //    bool isValid = false;
        //    DataRow drAccountLoanRecoveryRetries = null;
        //    string[] strTransactionDetailsResponse = null;
        //    try
        //    {
        //        IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);



        //        drAccountLoanRecoveryRetries = dsAccountLoanRecoveryRetries.Tables[0].Rows[0];


        //        using (IDbConnection connection = DBClient.GetConnection(usrInfo))
        //        {
        //            using (IDbTransaction trans = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    strLOPMessage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_PendingAccountLoanRecoveryRetries", drAccountLoanRecoveryRetries);

        //                    auditInfo.Status = BRStatus.Successful;
        //                    auditInfo.Message = strLOPMessage;
        //                    trans.Commit();


        //                    strTransactionDetailsResponse = strLOPMessage.Split('|');

        //                    bool.TryParse(strTransactionDetailsResponse[0], out isValid);




        //                    return isValid;
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();



        //                    auditInfo.Status = BRStatus.Failed;
        //                    auditInfo.Message = ex.Message;
        //                    return false;
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //        auditInfo.Status = BRStatus.Failed;
        //        auditInfo.Message = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {

        //        Task.Run(() => CoreOmniApiBase.PassLogInformation(usrInfo, usrInfo.SessionID, drAccountLoanRecoveryRetries["AccountID"].ToString(), drAccountLoanRecoveryRetries["MobileNumber"].ToString(), "BRAccountLoanRecoveryRetries", dsAccountLoanRecoveryRetries, auditInfo.Status.ToString(), auditInfo.Message.ToString()));
        //        ///Task.Run(() => PassLogInformation(usrInfo, usrInfo.SessionID, AccountID, MobileNumber, "LoanRequest", ConvertToDataSet(drLoanOnPhoneRequest), auditInfo.Status.ToString(), auditInfo.Message));
        //    }
        //}
        //#endregion Thirdparty Methods

    }
}

