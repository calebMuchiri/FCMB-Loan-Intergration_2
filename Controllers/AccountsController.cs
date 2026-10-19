
using BRBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nii.JSON;
using OmniApi.CoreLibraries;
using OmniApi.Models;
using OmniApi.Models.Cards;
using OmniApi.Models.FCMBLoan;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml.Serialization;
using WebApi.Jwt;
using WebApi.Jwt.Filters;
using WebApi.Jwt.Models;
using static OmniApi.Models.Cards.CardDetailsInquiry;
using static OmniApi.Models.Cards.CardInquiry;
using static OmniApi.Models.Cards.CardPinSet;
using static OmniApi.Models.Cards.SendPinViaSms;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace CoreOmniApi.Controllers
{
   //[JwtAuthentication] 
    public class AccountsController : ApiController
    {

        #region FCMB
        /// <summary>
        /// This API will list all client accounts.
        /// </summary>
        /// <param name="accountStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(200, "OK", typeof(AccountStatusResponse))]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid CustomerIdentifier")]
        [SwaggerResponse(500, "Internal Server Error")]
        public async Task<IHttpActionResult> CheckAccountStatus(AccountStatus accountStatus)
        {
            if (accountStatus == null)
            {
                var resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return Content((HttpStatusCode)300057, resp);
            }

            try
            {
                var usrInfo = CoreOmniApiBase.GetDbParameter();

                // Fire and forget logging
                Task.Run(() =>
                    CoreOmniApiBase.LogEvent(
                        string.Format("METHOD: CheckAccountStatus \n REQUEST:{0}\n",
                            JsonConvert.SerializeObject(accountStatus))));

                var statusResponse = await OmniCore.CheckAccountStatus(accountStatus);

                if (statusResponse != null)
                {
                    return Ok(statusResponse); // return 200 with the response
                }
                else
                {
                    var resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exist");
                    return Content((HttpStatusCode)300059, resp);
                }
            }
            catch (Exception ex)
            {
                var error = CoreOmniApiBase.GetBRErrorMessage(ex);
                return Content(HttpStatusCode.InternalServerError, error);
            }
        }

        /// <summary>
        /// This Api Will grt the Valid ClientName.
        /// </summary>
        /// <param name="accountStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(200, "OK", typeof(AccountStatusResponse))]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid CustomerIdentifier")]
        [SwaggerResponse(500, "Internal Server Error")]
        public async Task<IHttpActionResult> GetClientName(AccountStatus accountStatus)
        {
            if (accountStatus == null)
            {
                var resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return Content((HttpStatusCode)300057, resp);
            }

            try
            {
                var usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() =>
                    CoreOmniApiBase.LogEvent(
                        string.Format("METHOD: GetClientName \n REQUEST:{0}\n",
                            JsonConvert.SerializeObject(accountStatus))));

                var statusResponse = await OmniCore.GetClientNameAsync(accountStatus);

                if (statusResponse != null)
                {
                    return Ok(statusResponse); // return 200 with the response
                }
                else
                {
                    var resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exist");
                    return Content((HttpStatusCode)300059, resp);
                }
            }
            catch (Exception ex)
            {
                var error = CoreOmniApiBase.GetBRErrorMessage(ex);
                return Content(HttpStatusCode.InternalServerError, error);
            }
        }

        /// <summary>
        /// This API will Create a Finacle Base Retail Account using the Third-Party API.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(200, "OK", typeof(BaseRetailAccountOpeningResponse))]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(500, "Internal Server Error")]
        public async Task<IHttpActionResult> RemoteBaseRetailAccountOpening(BaseRetailAccountOpeningRequest request)
        {
            if (request == null)
            {
                var resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return Content((HttpStatusCode)300057, resp);
            }

            try
            {
                var usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() =>
                    CoreOmniApiBase.LogEvent(
                        $"METHOD: RemoteBaseRetailAccountOpening \n REQUEST:{JsonConvert.SerializeObject(request)}"));

                var apiResponse = await OmniCore.CreateBaseRetailAccountAsync(request);

                if (apiResponse != null)
                {
                    return Ok(apiResponse); // 200 OK
                }
                else
                {
                    var resp = CoreOmniApiBase.GenResponse("99", "300059", "Unable to Create Account");
                    return Content((HttpStatusCode)300059, resp);
                }
            }
            catch (Exception ex)
            {
                var error = CoreOmniApiBase.GetBRErrorMessage(ex);
                return Content(HttpStatusCode.InternalServerError, error);
            }
        }

        /// <summary>
        /// This API will Add a Lien to an Account using the Third-Party API.
        /// </summary>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(200, "OK", typeof(AddLienResponse))]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(500, "Internal Server Error")]
        //public async Task<IHttpActionResult> AddLien(AddLienRequest request)
        public async Task<IHttpActionResult> AddLien()
        {
            var usrInfo = CoreOmniApiBase.GetDbParameter();

            try
            {
                var penLaonDueAmounts = await OmniCore.GetLoanDueAmountAsync(usrInfo) ?? new List<AddLienRequest>();

                if (!penLaonDueAmounts.Any())
                {
                    var resp = CoreOmniApiBase.GenResponse("99", "300057", "No pending liens found");
                    return Content((HttpStatusCode)300057, resp);
                }

                var batch = new AddLienBatchResponse { requested = penLaonDueAmounts.Count };

                foreach (var penLoan in penLaonDueAmounts)
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AddLien \n REQUEST:{JsonConvert.SerializeObject(penLoan)}"));

                    try
                    {
                        var AddLienRequest = new AddLienRequest
                        {
                            requestID = penLoan.requestID,
                            accountNo = penLoan.accountNo,
                            amountValue = penLoan.amountValue,
                            currencyCode = penLoan.currencyCode,
                            rmks = penLoan.rmks,
                            reasonCode = penLoan.reasonCode ?? "040", // Default to "040" if null
                            StartDT = penLoan.StartDT,
                            EndDT = penLoan.EndDT,
                        };

                        var apiResponse = await OmniCore.AddLienAsync(AddLienRequest);

                        var result = new AddLienResult
                        {
                            requestId = penLoan?.requestID,
                            accountNo = penLoan?.accountNo,
                            success = apiResponse?.status ?? false,
                            code = apiResponse?.code,
                            description = apiResponse?.description,
                            data = apiResponse?.data
                        };

                        batch.results.Add(result);

                        OmniCore.UpdateAddLienRequest(usrInfo, penLoan.RowID, JsonConvert.SerializeObject(result));

                        if (result.success) batch.succeeded++; else batch.failed++;
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AddLien \n RESPONSE:{JsonConvert.SerializeObject(result)}"));
                    }
                    catch (Exception exReq)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: AddLien - Exception for account {penLoan?.accountNo}: {exReq.Message}\n{exReq.StackTrace}");
                        var fail = new AddLienResult
                        {
                            requestId = penLoan?.requestID,
                            accountNo = penLoan?.accountNo,
                            success = false,
                            code = "500",
                            description = exReq.Message,
                            data = null
                        };
                        batch.results.Add(fail);
                        batch.failed++;
                        OmniCore.UpdateAddLienRequest(usrInfo, penLoan.RowID, JsonConvert.SerializeObject(fail));
                    }
                }

                // Moved return statement outside the foreach loop
                return Ok(batch);
            }
            catch (Exception ex)
            {
                var err = CoreOmniApiBase.GetBRErrorMessage(ex);
                return Content(HttpStatusCode.InternalServerError, err);
            }
        }





        #endregion FCMB




        //----------------------------------------CARDS-------------------------------------------------------


        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "The Origin and Destined Accounts are the Same")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "Invalid Account")]
        [SwaggerResponse(300018, "Invalid Amount")]
        [SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked")]
        [SwaggerResponse(300007, "Transaction not successful")]

        private async Task<IHttpActionResult> GetCardsRequest(CardsRequestXml cardsRequestXml)
        {
            var atmUrl = ConfigurationManager.AppSettings["GetCardRequestUrl"];
            try
            {
                var soapEnvelope = OmniCore.createCardRequestXml(cardsRequestXml);

                CoreOmniApiBase.LogEvent($"METHOD: GetCardsRequest \n GetCardsRequest: {cardsRequestXml}\n");

                var responseXml = await OmniCore.ExecuteSoapRequest(soapEnvelope, atmUrl);
                //var jsonString = JsonConvert.SerializeObject(cardsRequestXml);
                //var response = await OmniCore.ExecuteRequestAsync(jsonString, atmUrl);

                CoreOmniApiBase.LogEvent($"METHOD: GetCardsRequest \n RESPONSE: {responseXml}\n");

                //DESERIALIZE SOAP RESPONSE
                var serializer = new XmlSerializer(typeof(GetCardsResponseXMl.Envelope));
                using (var reader = new StringReader(responseXml))
                {
                    var envelope = (GetCardsResponseXMl.Envelope)serializer.Deserialize(reader);
                    var card = envelope?.Body?.GetCardsResponse?.Card;

                    if (card == null)
                        //{
                        return Content(HttpStatusCode.NotFound, "Card not Found");
                    //}

                    return Ok(card);


                }




            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "The Origin and Destined Accounts are the Same")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "Invalid Account")]
        [SwaggerResponse(300018, "Invalid Amount")]
        [SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked")]
        [SwaggerResponse(300007, "Transaction not successful")]
        private async Task<IHttpActionResult> AtmCardGeneration(string OurBranchID)
        {
            var usrInfo = CoreOmniApiBase.GetDbParameter();
            usrInfo.strUser = CoreOmniApiBase.gOperatorID;
            BRDataSet dsSoapRequestData = new BRDataSet();

            var atmUrl = ConfigurationManager.AppSettings["CRDBAtmUrl"];
            bool hasProcessedRequests = false;



            // Check for new card requests
            if (OmniCore.GetSoapRequestData(usrInfo, out dsSoapRequestData) && dsSoapRequestData.Tables.Count != 0)
            {
                var soapRequestDataList = OmniCore.GenerateXmlRequest(dsSoapRequestData);
                await OmniCore.ProcessNewCards(usrInfo, soapRequestDataList);
                hasProcessedRequests = true;
            }

            if (!hasProcessedRequests)
            {
                var resp = CoreOmniApiBase.GenResponse("99", "300057", "No pending applications found");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }



            var successResp = CoreOmniApiBase.GenResponse("200", "300057", "Success");
            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, successResp, successResp);
        }


        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300055, "The Origin and Destined Accounts are the Same")]
        [SwaggerResponse(300054, "Customer account not active in this branch, please select the correct branch")]
        [SwaggerResponse(300056, "Invalid Account")]
        [SwaggerResponse(300018, "Invalid Amount")]
        [SwaggerResponse(300008, "Not Having Enough Balance, Transaction Blocked")]
        [SwaggerResponse(300007, "Transaction not successful")]
        private async Task<IHttpActionResult> AtmCardReplacement(string OurBranchID)
        {
            var usrInfo = CoreOmniApiBase.GetDbParameter();
            usrInfo.strUser = CoreOmniApiBase.gOperatorID;
            BRDataSet dsSoapRequestData = new BRDataSet();
            var atmUrl = ConfigurationManager.AppSettings["CRDBAtmUrl"];

            // 1. Check for replacement cards
            if (!OmniCore.GetReplacementCards(usrInfo, out dsSoapRequestData))
            {
                var resp = CoreOmniApiBase.GenResponse("99", "300057", "No pending Replacements found");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            // 2. Get list of Card Numbers to be replaced
            var cardNumbersList = OmniCore.GetCardsToBeReplaced(dsSoapRequestData);
            var soapRequestDataList = new List<ReplacementRequestXml>();

            CoreOmniApiBase.LogEvent($"METHOD: CardList \n REQUEST: {cardNumbersList}");

            // 3. Process each card
            foreach (var cardNumber in cardNumbersList)
            {
                try
                {
                    // Create Card Inquiry Payload
                    var requestCard = new CardsRequestXml
                    {
                        inst_Id = "1002",
                        card_mask = cardNumber,
                        get_balance = 0,
                        lang = "LANGENG"
                    };

                    // Call card Enquiry Api
                    var response = await GetCardsRequest(requestCard);
                    var okResult = response as OkNegotiatedContentResult<GetCardsResponseXMl.Card>;
                    if (okResult != null)
                    {
                        var generatedRequests = OmniCore.GenerateReplacementXmlRequest(okResult.Content);
                        soapRequestDataList.AddRange(generatedRequests);
                    }
                }
                catch (Exception ex)
                {
                    CoreOmniApiBase.LogEvent(string.Format("Error processing card {0}: {1}", cardNumber, ex.Message));
                }
            }

            // 4. Process replacements if we have valid requests
            if (soapRequestDataList.Count > 0)
            {
                await OmniCore.ProcessReplacementCards(usrInfo, soapRequestDataList);
                var successResp = CoreOmniApiBase.GenResponse("200", "300057", "Success");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, successResp, successResp);
            }

            // 5. Return error if no valid cards processed
            var errorResp = CoreOmniApiBase.GenResponse("99", "300057", "No valid cards found for replacement");
            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, errorResp, errorResp);
        }

        ///// <summary>
        ///// This API will return all the customer accounts associated with the customer and their specific details.
        ///// </summary>
        ///// <param name="myData"></param>
        ///// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Data")]
        [SwaggerResponse(300059, "Invalid CustomerIdentifier")]
        [SwaggerResponse(300019, "Invalid CardMask")]
        [SwaggerResponse(300021, "Invalid CustomerNumber")]
        [SwaggerResponse(300022, "Invalid CustomerId")]
        [SwaggerResponse(300023, "Invalid AccountNumber")]
        [SwaggerResponse(300024, "Invalid CardNumber")]
        [SwaggerResponse(300025, "Invalid Lang Used")]
        [SwaggerResponse(500, "Internal Server Error")]
        //public async Task<IHttpActionResult> GetCardDetails(CardDetailsRequestData myData)
        private async Task<IHttpActionResult> CardInquiry(CardInquiryRequestData myData)
        {
            DataSet resp = null;
            string CardMask = string.Empty;
            string CardNumber = string.Empty;
            string InstitutionId = string.Empty;
            string GetBalance = "0";
            string Lang = string.Empty;
            var cardsUrl = ConfigurationManager.AppSettings["GetCardRequestUrl"];
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var AppId = ConfigurationManager.AppSettings["AppId"];
            var AppVersion = ConfigurationManager.AppSettings["AppVersion"];
            var endpoint = "card-Inquiry";

            var fullUrl = cardsUrl + endpoint;

            // Entry log
            CoreOmniApiBase.LogEvent($"METHOD: CardInquiry START \n FullUrl: {fullUrl}\n RequestPayload: {JsonConvert.SerializeObject(myData)}");

            // validate input request
            if (myData == null)
            {
                CoreOmniApiBase.LogEvent("METHOD: CardInquiry - Invalid request: request body is null");
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Req");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                // validate field by field and log which field is missing
                CardMask = myData.Cardmask;
                GetBalance = myData.GetBalance;
                Lang = myData.Lang;
                InstitutionId = myData.InstitutionId;

                if (string.IsNullOrEmpty(CardMask))
                {
                    CoreOmniApiBase.LogEvent("METHOD: CardInquiry - Validation failed: Cardmask is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300021", "Invalid CardMask");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(Lang))
                {
                    CoreOmniApiBase.LogEvent("METHOD: CardInquiry - Validation failed: Lang is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300025", "Invalid Lang");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(InstitutionId))
                {
                    CoreOmniApiBase.LogEvent("METHOD: CardInquiry - Validation failed: InstitutionId is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300025", "Invalid Lang");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Generate Access Token (log progress and mask token)
                string accessToken = null;
                try
                {
                    accessToken = await OmniCore.GetCrdbAccessToken();
                    string tokenPreview = accessToken != null && accessToken.Length > 8
                        ? $"{accessToken.Substring(0, 6)}...{accessToken.Substring(accessToken.Length - 2)} (len:{accessToken.Length})"
                        : (accessToken ?? "null");
                    CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Retrieved access token: {tokenPreview}");
                }
                catch (Exception exToken)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - GetCrdbAccessToken failed: {exToken.Message}\n{exToken.StackTrace}");
                    return Content(HttpStatusCode.InternalServerError, "Failed to obtain access token");
                }

                // Build request
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(fullUrl));
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Channel", channelId);
                request.Headers.Add("AppId", AppId);
                request.Headers.Add("AppVersion", AppVersion);
                request.Headers.Add("RequestId", Guid.NewGuid().ToString());
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var requestContentString = JsonConvert.SerializeObject(myData);
                var content = new StringContent(requestContentString, Encoding.UTF8, "application/json");
                request.Content = content;

                // Log request metadata (avoid logging full token again)
                CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Sending request. Url: {fullUrl}\nHeaders: ChannnelId={channelId}, AppId={AppId}, AppVersion={AppVersion}, RequestId={request.Headers.GetValues("RequestId").FirstOrDefault()}\nRequestBody: {requestContentString}");

                // Send request
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response;
                    try
                    {
                        response = await client.SendAsync(request);
                    }
                    catch (Exception httpEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - HTTP send failed: {httpEx.Message}\n{httpEx.StackTrace}");
                        return Content(HttpStatusCode.InternalServerError, "Error while calling card inquiry service");
                    }

                    string responseContent = null;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception readEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Failed reading response content: {readEx.Message}\n{readEx.StackTrace}");
                        responseContent = null;
                    }

                    CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Response received. StatusCode: {(int)response.StatusCode} ({response.ReasonPhrase})\nResponseBody: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Non-success status code returned: {(int)response.StatusCode}. Returning InternalServerError.");
                        return Content(HttpStatusCode.InternalServerError, "Internal Server Error");
                    }

                    // Parse response
                    CardInquiryResponseData cardInquiryResponse = null;
                    try
                    {
                        cardInquiryResponse = JsonConvert.DeserializeObject<CardInquiryResponseData>(responseContent);
                    }
                    catch (Exception deserEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nResponseContent: {responseContent}");
                        return Content(HttpStatusCode.InternalServerError, "Invalid response format from card service");
                    }

                    if (cardInquiryResponse?.ResponseHeader != null)
                    {
                        if (cardInquiryResponse.ResponseHeader.StatusCode == 1000)
                        {
                            var cardDetails = cardInquiryResponse.ResponseBody;
                            CoreOmniApiBase.LogEvent("METHOD: CardInquiry - Success. Returning card details.");
                            return Ok(cardDetails);
                        }
                        else
                        {
                            CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - Card service returned error: {cardInquiryResponse.ResponseHeader.StatusCode} : {cardInquiryResponse.ResponseHeader.StatusMessage}");
                            return Content(HttpStatusCode.BadRequest, $"{cardInquiryResponse.ResponseHeader.StatusCode} : {cardInquiryResponse.ResponseHeader.StatusMessage}");
                        }
                    }
                    else
                    {
                        CoreOmniApiBase.LogEvent("METHOD: CardInquiry - ResponseHeader is null in card service response");
                        return Content(HttpStatusCode.BadRequest, "Invalid Response from server");
                    }
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"METHOD: CardInquiry - UNHANDLED EXCEPTION: {ex.Message}\n{ex.StackTrace}\nRequestPayload: {JsonConvert.SerializeObject(myData)}");
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Data")]
        [SwaggerResponse(300023, "Invalid CardStatusCode")]
        [SwaggerResponse(300024, "Invalid CardNumber")]
        [SwaggerResponse(300025, "reason is required")]
        [SwaggerResponse(500, "Internal Server Error")]
        
        private async Task<IHttpActionResult> ChangeCardStatus(CardStatusChange.RequestData requestData)
        {
            string CardNumber = string.Empty;
            string CardStatusCode = string.Empty;
            string Reason = string.Empty;
            DataSet resp = null;
            var requestId = Guid.NewGuid().ToString();
            var start = DateTime.UtcNow;

            // Entry log
            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus START | RequestId: {requestId} | Timestamp: {start:O} | Payload: {JsonConvert.SerializeObject(requestData)}"));

            try
            {
                if (requestData == null)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Invalid request: request body is null");
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Data");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                CardStatusCode = requestData.CardStatusCode;
                CardNumber = requestData.CardNumber;
                Reason = requestData.Reason;

                // Validate input data and log specific failures
                if (string.IsNullOrEmpty(CardStatusCode))
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Validation failed: CardStatusCode is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Invalid Card Status Code");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(CardNumber))
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Validation failed: CardNumber is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Invalid CardNumber");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(Reason))
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Validation failed: Reason is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Reason is required");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Call lower-level service and log progress
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Calling OmniCore.ChangeCardStatusAsync"));

                string responseContent = null;
                try
                {
                    responseContent = await OmniCore.ChangeCardStatusAsync(requestData);
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Received raw response: {(responseContent ?? "null")}"));
                }
                catch (Exception exCall)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - OmniCore.ChangeCardStatusAsync failed: {exCall.Message}\n{exCall.StackTrace}");
                    resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (!string.IsNullOrEmpty(responseContent))
                {
                    CardStatusChange.ResponseData CardStatusResponse = null;
                    try
                    {
                        CardStatusResponse = JsonConvert.DeserializeObject<CardStatusChange.ResponseData>(responseContent);
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Deserialized response. Header: {JsonConvert.SerializeObject(CardStatusResponse?.ResponseHeader)}"));
                    }
                    catch (Exception exDeser)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Deserialization failed: {exDeser.Message}\n{exDeser.StackTrace}\nRawResponse: {responseContent}");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "Invalid Response from server");
                    }

                    if (CardStatusResponse?.ResponseHeader != null)
                    {
                        var statusCode = CardStatusResponse.ResponseHeader.StatusCode;
                        var statusMsg = CardStatusResponse.ResponseHeader.StatusMessage;
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - ResponseHeader StatusCode: {statusCode}, StatusMessage: {statusMsg}"));

                        if (statusCode == 1000)
                        {
                            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Success. Returning success message."));
                            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, statusMsg, statusMsg);
                        }
                        else
                        {
                            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Remote service error. StatusCode: {statusCode}, Message: {statusMsg}"));
                            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, statusMsg, statusMsg);
                        }
                    }
                    else
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - ResponseHeader is null in service response");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "Invalid Response from server");
                    }
                }
                else
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - Empty response from OmniCore.ChangeCardStatusAsync");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "An error occured while processing request");
                }
            }
            catch (Exception ex)
            {
                // Log the exception with stack trace and request id
                CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus | RequestId: {requestId} - EXCEPTION: {ex.Message}\n{ex.StackTrace}\nPayload: {JsonConvert.SerializeObject(requestData)}");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", ex.Message);
            }
            finally
            {
                var end = DateTime.UtcNow;
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ChangeCardStatus END | RequestId: {requestId} | Start: {start:O} | End: {end:O}"));
            }
        }


        [HttpPost]
    private async Task<IHttpActionResult> CardPinSet(CardPinSetRequestData pinSet)
        {
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var AppId = ConfigurationManager.AppSettings["AppId"];
            var AppVersion = ConfigurationManager.AppSettings["AppVersion"];
            var cardsUrl = ConfigurationManager.AppSettings["CRDBCardsUrl"];
            DataSet resp = null;
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Input validation + early returns (log specific failures)
                if (pinSet == null)
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet START | RequestId:{requestId} | ERROR: request body is null"));
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Data");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                string CardNumber = pinSet.CardNumber;
                string CardId = pinSet.CardId;
                string PinBlock = pinSet.PinBlock;

                if (string.IsNullOrEmpty(CardNumber))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Validation failed: CardNumber is null or empty"));
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Invalid Card Number");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(CardId))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Validation failed: CardId is null or empty"));
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Invalid CardId");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(PinBlock))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Validation failed: PinBlock is null or empty"));
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Invalid PinBlock");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Start log (mask sensitive info)
                var maskedCard = CardNumber.Length > 4 ? $"****{CardNumber.Substring(CardNumber.Length - 4)}" : CardNumber;
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet START | RequestId:{requestId} | Url:{cardsUrl} | Card:{maskedCard} | CardId:{CardId}"));

                // Get access token (masked) and log
                string accessToken;
                try
                {
                    accessToken = await OmniCore.GetCrdbAccessToken();
                    var tokenPreview = accessToken != null && accessToken.Length > 8
                        ? $"{accessToken.Substring(0, 6)}...{accessToken.Substring(accessToken.Length - 2)} (len:{accessToken.Length})"
                        : (accessToken ?? "null");
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Retrieved access token: {tokenPreview}"));
                }
                catch (Exception exToken)
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - GetCrdbAccessToken failed: {exToken.Message}\n{exToken.StackTrace}"));
                    resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Prepare request payload (do NOT log full PinBlock)
                var safePayloadForLog = new
                {
                    pinSet.CardNumber,
                    pinSet.CardId,
                    PinBlock = "****" // never log PINs
                };

                var requestContent = JsonConvert.SerializeObject(pinSet, Formatting.None);
                var content = new StringContent(requestContent, Encoding.UTF8, "application/json");

                // Use HttpClient and set headers (set reasonable timeout to avoid long waits)
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken ?? string.Empty);

                    // Use header name "Channel" (consistent with other methods)
                    client.DefaultRequestHeaders.Remove("Channel");
                    client.DefaultRequestHeaders.Add("Channel", channelId);
                    client.DefaultRequestHeaders.Remove("AppId");
                    client.DefaultRequestHeaders.Add("AppId", AppId);
                    client.DefaultRequestHeaders.Remove("AppVersion");
                    client.DefaultRequestHeaders.Add("AppVersion", AppVersion);
                    client.DefaultRequestHeaders.Remove("RequestId");
                    client.DefaultRequestHeaders.Add("RequestId", requestId);

                    // Log short request metadata (non-sensitive)
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Sending request. Channel:{channelId}, AppId:{AppId}, AppVersion:{AppVersion}, PayloadPreview:{JsonConvert.SerializeObject(safePayloadForLog)}"));

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.PostAsync(cardsUrl, content).ConfigureAwait(false);
                    }
                    catch (Exception httpEx)
                    {
                        stopwatch.Stop();
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - HTTP send failed: {httpEx.Message}\n{httpEx.StackTrace} | ElapsedMs:{stopwatch.ElapsedMilliseconds}"));
                        resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    string responseContent;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        responseContent = null;
                        stopwatch.Stop();
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Failed reading response content: {readEx.Message}\n{readEx.StackTrace} | ElapsedMs:{stopwatch.ElapsedMilliseconds}"));
                        resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    stopwatch.Stop();
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Response received. StatusCode:{(int)response.StatusCode} ({response.ReasonPhrase}) | ElapsedMs:{stopwatch.ElapsedMilliseconds} | ResponseBodyLength:{(responseContent?.Length ?? 0)}"));

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var pinSetResponse = JsonConvert.DeserializeObject<CardPinSetResponse>(responseContent);
                            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Deserialized response header: {JsonConvert.SerializeObject(pinSetResponse?.responseHeader)}"));

                            if (pinSetResponse?.responseHeader != null && pinSetResponse.responseHeader.StatusCode == 1000)
                            {
                                var responseMessage = pinSetResponse.responseHeader.StatusMessage;
                                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Success. StatusMessage:{responseMessage}"));
                                // preserve original return behavior
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", responseMessage);
                            }
                            else if (pinSetResponse?.responseHeader != null)
                            {
                                var responseMessage = pinSetResponse.responseHeader.StatusMessage;
                                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Remote service returned error code:{pinSetResponse.responseHeader.StatusCode}, message:{responseMessage}"));
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", responseMessage);
                            }
                            else
                            {
                                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Invalid response from server (missing header)"));
                                var responseMessage = "Error Processing the response from the server";
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", responseMessage);
                            }
                        }
                        catch (Exception deserEx)
                        {
                            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace} | RawResponseLength:{(responseContent?.Length ?? 0)}"));
                            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "Error Processing the response from the server");
                        }
                    }
                    else
                    {
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: CardPinSet | RequestId:{requestId} - Non-success status code: {(int)response.StatusCode} ({response.ReasonPhrase}) | ResponseBodyLength:{(responseContent?.Length ?? 0)}"));
                        var responseMessage = "Invalid Response from the server";
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", responseMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD : CardPinSet | RequestId:{requestId} \n EXCEPTION : {ex.Message} \n{ex.StackTrace}"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", ex.Message);
            }
        }
        //[HttpPost]
        //public async Task<IHttpActionResult> CardPinReset() { 
        //}

        
    private async Task<IHttpActionResult> SendPinSms(SendSmsPinRequestData sendSms)
        {
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var AppId = ConfigurationManager.AppSettings["AppId"];
            var AppVersion = ConfigurationManager.AppSettings["AppVersion"];
            var smsUrl = ConfigurationManager.AppSettings["PinSetSmsUrl"];
            DataSet resp = null;
            string CardNumber = string.Empty;
            DateTime ExpiryDate = DateTime.Now;
            string Phone = string.Empty;
            string accessToken = string.Empty;
            string PhoneRegex = @"^2557\d{8}$";
            var requestId = Guid.NewGuid().ToString();
            var start = DateTime.UtcNow;

            // Entry log
            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms START | RequestId: {requestId} | Url: {smsUrl} | Payload: {JsonConvert.SerializeObject(sendSms)}"));

            try
            {
                if (sendSms == null)
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Invalid request: body is null"));
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                CardNumber = sendSms.CardNumber;
                ExpiryDate = sendSms.ExpiryDate;
                Phone = sendSms.Phone;

                // Validate phone
                if (string.IsNullOrEmpty(Phone))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - PhoneNumber is null or empty"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "PhoneNumber cannot be Null.");
                }

                if (!Regex.IsMatch(Phone, PhoneRegex))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Invalid phone format: {Phone}"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "Invalid phone number format. Use 255712345678.");
                }

                // Validate expiry and card number
                if (ExpiryDate == default(DateTime))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - ExpiryDate missing or default"));
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Expiry Date is requird");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(CardNumber))
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - CardNumber is null or empty"));
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Card Number is required");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Get access token
                try
                {
                    accessToken = await OmniCore.GetCrdbAccessToken();
                    var tokenPreview = accessToken != null && accessToken.Length > 8
                        ? $"{accessToken.Substring(0, 6)}...{accessToken.Substring(accessToken.Length - 2)} (len:{accessToken.Length})"
                        : (accessToken ?? "null");
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Retrieved access token: {tokenPreview}"));
                }
                catch (Exception exToken)
                {
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - GetCrdbAccessToken failed: {exToken.Message}\n{exToken.StackTrace}"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Failed to obtain access token");
                }

                // Prepare request content (serialize DTO)
                var requestContentString = JsonConvert.SerializeObject(sendSms);
                var content = new StringContent(requestContentString, Encoding.UTF8, "application/json");

                // Masked preview for logs
                var maskedCard = CardNumber.Length > 4 ? $"****{CardNumber.Substring(CardNumber.Length - 4)}" : CardNumber;
                var maskedPhone = Phone.Length > 4 ? $"****{Phone.Substring(Phone.Length - 4)}" : Phone;
                var payloadPreview = new { CardNumber = maskedCard, ExpiryDate = ExpiryDate, Phone = maskedPhone };

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Sending request. Url: {smsUrl}\nHeaders: Channel={channelId}, AppId={AppId}, AppVersion={AppVersion}, RequestId={requestId}\nPayloadPreview: {JsonConvert.SerializeObject(payloadPreview)}"));

                // Send request
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken ?? string.Empty);

                    client.DefaultRequestHeaders.Remove("Channel");
                    client.DefaultRequestHeaders.Add("Channel", channelId);
                    client.DefaultRequestHeaders.Remove("AppId");
                    client.DefaultRequestHeaders.Add("AppId", AppId);
                    client.DefaultRequestHeaders.Remove("AppVersion");
                    client.DefaultRequestHeaders.Add("AppVersion", AppVersion);
                    client.DefaultRequestHeaders.Remove("RequestId");
                    client.DefaultRequestHeaders.Add("RequestId", requestId);

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.PostAsync(smsUrl, content).ConfigureAwait(false);
                    }
                    catch (Exception httpEx)
                    {
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - HTTP send failed: {httpEx.Message}\n{httpEx.StackTrace}"));
                        resp = CoreOmniApiBase.GenResponse("99", "300007", "Transaction not successful");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                    string responseContent = null;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Failed reading response content: {readEx.Message}\n{readEx.StackTrace}"));
                        responseContent = null;
                    }

                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Response received. StatusCode: {(int)response.StatusCode} ({response.ReasonPhrase}) | ResponseBodyLength: {(responseContent?.Length ?? 0)}"));

                    if (!response.IsSuccessStatusCode)
                    {
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Non-success status code: {(int)response.StatusCode}"));
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Invalid response");
                    }

                    try
                    {
                        var sendSmsResponse = JsonConvert.DeserializeObject<SendPinSmsResponse>(responseContent);
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Deserialized response header: {JsonConvert.SerializeObject(sendSmsResponse?.responseHeader)}"));

                        if (sendSmsResponse?.responseHeader != null)
                        {
                            if (sendSmsResponse.responseHeader.StatusCode == 1000)
                            {
                                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Success. StatusMessage: {sendSmsResponse.responseHeader.StatusMessage}"));
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "200", "Sms Send Sucessfully");
                            }
                            else
                            {
                                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Remote service error. Code: {sendSmsResponse.responseHeader.StatusCode}, Message: {sendSmsResponse.responseHeader.StatusMessage}"));
                                var responseMessage = sendSmsResponse.responseHeader.StatusMessage;
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", responseMessage);
                            }
                        }
                        else
                        {
                            Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Missing response header in remote response"));
                            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Error Processing response");
                        }
                    }
                    catch (Exception deserEx)
                    {
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponseLength: {(responseContent?.Length ?? 0)}"));
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Error Processing response");
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms | RequestId: {requestId} - EXCEPTION: {ex.Message}\n{ex.StackTrace}\nPayload: {JsonConvert.SerializeObject(sendSms)}"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", ex.Message);
            }
            finally
            {
                var end = DateTime.UtcNow;
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: SendPinSms END | RequestId: {requestId} | Start: {start:O} | End: {end:O}"));
            }
        }


        [HttpPost]
        private async Task<IHttpActionResult> ActivateCard(CardActivation.CardActivationRequestData requestData)
        {
            var channelId = ConfigurationManager.AppSettings["ChannelId"];
            var AppId = ConfigurationManager.AppSettings["AppId"];
            var AppVersion = ConfigurationManager.AppSettings["AppVersion"];
            var cardsUrl = ConfigurationManager.AppSettings["CRDBCardsUrl"];
            DataSet resp = null;
            string CardNumber = string.Empty;
            DateTime ExpiryDate = DateTime.Now;
            string Phone = string.Empty;
            string accessToken = string.Empty;
            string PhoneRegex = @"^2557\d{8}$";
            var requestId = Guid.NewGuid().ToString();
            var start = DateTime.UtcNow;

            // Entry log
            CoreOmniApiBase.LogEvent($"METHOD: ActivateCard START | RequestId: {requestId} | Url: {cardsUrl} | Payload: {JsonConvert.SerializeObject(requestData)}");

            try
            {
                if (requestData == null)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Invalid request: request body is null");
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                CardNumber = requestData.Cardnumber;
                ExpiryDate = requestData.ExpiryDate;
                Phone = requestData.Phonenumber;

                // Validate inputs and log specific failures
                if (!string.IsNullOrEmpty(Phone))
                {
                    if (!Regex.IsMatch(requestData.Phonenumber, PhoneRegex))
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Invalid phone format: {requestData.Phonenumber}");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "Invalid phone number format. Use 255712345678.");
                    }
                }
                else
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - PhoneNumber is null or empty");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", "PhoneNumber cannot be Null.");
                }

                if (ExpiryDate == default(DateTime))
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - ExpiryDate missing or default");
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Expiry Date is requird");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(CardNumber))
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - CardNumber is null or empty");
                    resp = CoreOmniApiBase.GenResponse("99", "300023", "Card Number is required");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                // Get access token and log masked preview
                try
                {
                    accessToken = await OmniCore.GetCrdbAccessToken();
                    string tokenPreview = accessToken != null && accessToken.Length > 8
                        ? $"{accessToken.Substring(0, 6)}...{accessToken.Substring(accessToken.Length - 2)} (len:{accessToken.Length})"
                        : (accessToken ?? "null");
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Retrieved access token: {tokenPreview}");
                }
                catch (Exception exToken)
                {
                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - GetCrdbAccessToken failed: {exToken.Message}\n{exToken.StackTrace}");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Failed to obtain access token");
                }

                // Build request
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(cardsUrl));
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Channel", channelId);
                request.Headers.Add("AppId", AppId);
                request.Headers.Add("AppVersion", AppVersion);
                request.Headers.Add("RequestId", requestId);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // Serialize the DTO (not the HttpRequestMessage) and mask sensitive fields in logs
                var requestContentString = JsonConvert.SerializeObject(requestData);
                request.Content = new StringContent(requestContentString, Encoding.UTF8, "application/json");

                var maskedCard = CardNumber.Length > 4 ? $"****{CardNumber.Substring(CardNumber.Length - 4)}" : CardNumber;
                var payloadPreview = new { CardNumber = maskedCard, ExpiryDate = requestData.ExpiryDate, Phonenumber = "****" };

                CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Sending request. Url: {cardsUrl}\nHeaders: Channel={channelId}, AppId={AppId}, AppVersion={AppVersion}, RequestId={requestId}\nPayloadPreview: {JsonConvert.SerializeObject(payloadPreview)}");

                // Send
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response;
                    try
                    {
                        response = await client.SendAsync(request);
                    }
                    catch (Exception httpEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - HTTP send failed: {httpEx.Message}\n{httpEx.StackTrace}");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Error while calling activation service");
                    }

                    string responseContent = null;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception readEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Failed reading response content: {readEx.Message}\n{readEx.StackTrace}");
                        responseContent = null;
                    }

                    CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Response received. StatusCode: {(int)response.StatusCode} ({response.ReasonPhrase})\nResponseBody: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Non-success status code: {(int)response.StatusCode}");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Invalid response");
                    }

                    // Parse response
                    try
                    {
                        var activationResponse = JsonConvert.DeserializeObject<CardActivation.CardActivationResponseData>(responseContent);

                        if (activationResponse?.crdbResponseHeader != null)
                        {
                            if (activationResponse.crdbResponseHeader.StatusCode == 1000)
                            {
                                CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Success. StatusMessage: {activationResponse.crdbResponseHeader.StatusMessage}");
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "200", "Card Activated Sucessfully");
                            }
                            else
                            {
                                CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Remote service error. Code: {activationResponse.crdbResponseHeader.StatusCode}, Message: {activationResponse.crdbResponseHeader.StatusMessage}");
                                var responseMessage = activationResponse.crdbResponseHeader.StatusMessage;
                                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", responseMessage);
                            }
                        }
                        else
                        {
                            CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Activation response header is null");
                            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Error Processing response");
                        }
                    }
                    catch (Exception deserEx)
                    {
                        CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - Deserialization failed: {deserEx.Message}\n{deserEx.StackTrace}\nRawResponse: {responseContent}");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "500", "Error Processing response");
                    }
                }
            }
            catch (Exception ex)
            {
                CoreOmniApiBase.LogEvent($"METHOD: ActivateCard | RequestId: {requestId} - EXCEPTION: {ex.Message}\n{ex.StackTrace}\nPayload: {JsonConvert.SerializeObject(requestData)}");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, "400", ex.Message);
            }
            finally
            {
                var end = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"METHOD: ActivateCard END | RequestId: {requestId} | Start: {start:O} | End: {end:O}");
            }
        }

        //[HttpGet]
        //public async Task<IHttpActionResult> GetAcessToken(string HeadOffice)
        //{
        //    try
        //    {
        //         var token = await OmniCore.GetCrdbAccessToken();

        //        return Content(HttpStatusCode.OK, token);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}


        //----------------------------------------End Cards-------------------------------------------------------

        /// <summary>
        /// This API will return the specific customer account  details.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid CustomerIdentifier")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult GetAccountDetails(CustQuery myData)
        {
            string strAccountID = string.Empty;
            string strOurBranchID = string.Empty;
            string strMobileNumber = string.Empty;
            string strClientID = string.Empty;
            bool ValidAccount = false;
            BRDataSet ds_AccountCustomer = new BRDataSet();
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            IList<AccountsDetailResponse> lstAccountOpeningResponse = null;
            DataSet resp = null;
            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                
                string mydata = string.Empty;
              

                mydata = JsonConvert.SerializeObject(myData).ToString();
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountDetails", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetAccountDetails \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                ValidAccount = OmniCore.GetAccountDetails(usrInfo, mydata, out ds_AccountCustomer);
                ds_AccountCustomer.Namespace = null;
                ValidAccount = ds_AccountCustomer.Tables.Count > 0 ? true : false;



                if (ValidAccount == true)
                {
                    strResponseCode = ds_AccountCustomer.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = ds_AccountCustomer.Tables[0].Rows[0][0].ToString();
                    if (ValidAccount == true)
                    {

                       // lstAccountOpeningResponse = ds_AccountCustomer.Tables[0].ToListof<AccountsDetailResponse>();

                        resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);

                    }

                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetAccountDetails \n RESPONSE:{JsonConvert.SerializeObject(strResponseMessage).ToString()}\n"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);



                }
                else
                {

                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exists");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



            }
            catch (Exception ex)
            {
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }


        /// <summary>
        /// This API will List all client Accounts.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid CustomerIdentifier")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult GetClientAccountDetails(ClientAccQuery myData)
        {     
            string strClientID = string.Empty;
            bool ValidAccount = false;
            BRDataSet ds_AccountCustomer = new BRDataSet();
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            IList<AccountsDetailResponse> lstAccountOpeningResponse = null;
            DataSet resp = null;
            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {

                string mydata = string.Empty;

                mydata = JsonConvert.SerializeObject(myData).ToString();
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetClientAccountDetails \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                ValidAccount = OmniCore.GetClientAccountDetails(usrInfo, mydata, out ds_AccountCustomer);
                ds_AccountCustomer.Namespace = null;
                ValidAccount = ds_AccountCustomer.Tables.Count > 0 ? true : false;

                if (ValidAccount == true)
                {
                    strResponseCode = ds_AccountCustomer.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = ds_AccountCustomer.Tables[0].Rows[0][0].ToString();
                    if (ValidAccount == true)
                    {

                        resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);

                    }

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);



                }
                else
                {

                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exists");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }




            }
            catch (Exception ex)
            {
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }



        /// <summary>
        /// This API will return customer details.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid Client National ID")]
        [SwaggerResponse(500, "Internal Server Error")]   
        private IHttpActionResult GetClientByUniqueNumber(ClientDetails myData)
        {
            string strClientID = string.Empty;
            string strIDNumber = string.Empty;
            string strPhoneNumber = string.Empty;
            //string strAccountID = string.Empty;   this will be on another method
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            IList<ClientDetailResponse> lstClientDetailResponse = null;
            DataSet resp = null;
            bool ValidAccount = false;
            BRDataSet ds_Client = new BRDataSet();

            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                strClientID = myData.ClientID;
                strIDNumber = myData.IDNumber;
                strPhoneNumber = myData.PhoneNumber;
                //strAccountID = myData.AccountID;  this will be on another method

                string mydata = string.Empty;
                //mydata = myData.ToString();
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetClientByUniqueNumber", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetClientByUniqueNumber \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                mydata = JsonConvert.SerializeObject(myData).ToString();



                ValidAccount = OmniCore.GetClientByUniqueNumber(usrInfo, mydata, out ds_Client);
                ds_Client.Namespace = null;

                 if (ValidAccount == true)
                {
                    strResponseCode = ds_Client.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = "SUCCESS"; //ds_Client.Tables[0].Rows[0][1].ToString();
                    if (ValidAccount == true)
                    { 
                        //lstClientDetailResponse = ds_Client.Tables[0].ToListof<ClientDetailResponse>();
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                    }
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetClientByUniqueNumber \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //                       CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Client.Tables[0]),
                    //                       CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Client.Tables[1]));

                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //                                    ds_Client.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""),
                    //                                    ds_Client.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""));
                }

                else
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Invalid AccountID");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetClientByUniqueNumber \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }


        /// <summary>
        /// This endpoint will return Group details.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid Group ID")]
        [SwaggerResponse(500, "Internal Server Error")]


        private IHttpActionResult GroupDetails(GroupDetails myData)
        {
            string strGroupID = string.Empty;
            string strOurBranchID = string.Empty;
            string strOperatorID = string.Empty;
            string strDirection = string.Empty;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;

            IList<GroupDetailsResponse> lstGroupDetailResponse = null;

            DataSet resp = null;
            bool ValidAccount = false;
            BRDataSet ds_Group = new BRDataSet();

            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                strGroupID = myData.GroupID;
                strOurBranchID = myData.OurBranchID;
                strOperatorID = myData.OperatorID;
                //strAccountID = myData.AccountID;  this will be on another method

                string mydata = string.Empty;
                //mydata = myData.ToString();
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetGroupDetails", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetGroupDetails \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));


                mydata = JsonConvert.SerializeObject(myData).ToString();



                ValidAccount = OmniCore.GetGroupDetails(usrInfo, mydata, out ds_Group);
                ds_Group.Namespace = null;
                
                if (ValidAccount == true)
                {
                    strResponseCode = ds_Group.Tables[0].Rows[0][0].ToString();
                    if (ValidAccount == true)
                    {
                        strResponseMessage = "SUCCESS";
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, "000", strResponseMessage);
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetGroupDetails \n RESPONSE:{strResponseCode}\n"));
                    }
                    else
                    {
                        strResponseMessage = "Failed";
                        resp = CoreOmniApiBase.GenResponse(strResponseMessage, strResponseCode, strResponseMessage);
                    }
                    //Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetGroupDetails \n RESPONSE:{strResponseCode}\n"));
                  
                    //Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetGroupDetails \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetGroupDetails", "RESPONSE", JsonConvert.SerializeObject(strResponseCode).ToString(), ""));

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                }

                else
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Invalid AccountID");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

            }
            catch (Exception ex)
            {
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }




        /// <summary>
        /// Use this API to open new  Account in BRNET.  
        /// </summary>
        /// <remarks>
        /// supply the clientID and a new Account is added based on the specified product.
        /// </remarks>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
       
        //[SwaggerResponse(300052, "Gender cannot be empty")]
        //[SwaggerResponse(300053, "Date Of Birth cannot be empty")]
        //[SwaggerResponse(300054, "LegacyAccountID cannot be empty")]
        //[SwaggerResponse(300055, "LegacyClientID cannot be empty")]

        [SwaggerResponse(300051, "Product cannot be empty")]
        [SwaggerResponse(300056, "Branch cannot be empty")]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Account not successfully Created")]

        [SwaggerResponse(500, "Internal Server Error")]

        private IHttpActionResult AccountOpening(AccountOpening myData) {
            string strResponse = string.Empty;
            string strCreatedBy = string.Empty;
            string strOpenedBy = string.Empty;

          
            DateTime DOB = new DateTime();

            BRDataSet dsAccountCustomer = new BRDataSet();
            DataSet dsAccountOpening = new DataSet();
            BRDataSet ds_Group = new BRDataSet();
            AccountOpeningResponse accountDetailsResponse = new AccountOpeningResponse(); ;
            string[] strAccountDetailsResponse = null;
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
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening","REQUEST", JsonConvert.SerializeObject(myData).ToString(),""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AccountOpening \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));
                string mydata1 = string.Empty;
                mydata1 = JsonConvert.SerializeObject(myData).ToString();

                if (string.IsNullOrEmpty(myData.ClientID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300054", "ClientID cannot be empty");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



                if (string.IsNullOrEmpty(myData.OurBranchID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300056", "branch cannot be empty");

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                }

                if (string.IsNullOrEmpty(myData.ProductID))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300051", "product cannot be empty");

                    //Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AccountOpening \n RESPONSE:{"product cannot be empty"}\n"));
                    //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "RESPONSE", "product cannot be empty", ""));


                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                }


                DataTable dtAccountCustomer = CoreOmniApiBase.dtFromObject(myData, "dt_AccountCustomersMobile");
          
                if (!dtAccountCustomer.Columns.Contains("NewRecord"))
                {
                    dtAccountCustomer.Columns.Add("NewRecord", typeof(int));
                }
                dtAccountCustomer.Rows[0]["NewRecord"] = 1;


                if (!dtAccountCustomer.Columns.Contains("CreatedBy"))
                {
                    dtAccountCustomer.Columns.Add("CreatedBy", typeof(string));
                }
                dtAccountCustomer.Rows[0]["CreatedBy"] = usrInfo.strUser; 


                dsAccountCustomer.Tables.Add(dtAccountCustomer);
                strResponse = OmniCore.AddEditiBankingAccountCustomerApi(usrInfo, mydata1, dsAccountCustomer);

                if (string.IsNullOrEmpty(strResponse))
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300059", "Account not successfully Created");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }


                if (strResponse.Length > 0)
                {

                    dsAccountOpening.Tables.Add("dtAccountOpening");
                    dsAccountOpening.Tables[0].Columns.Add("Response");
                    DataRow dr = dsAccountOpening.Tables[0].NewRow();
                    dr["Response"] = strResponse;
                    dsAccountOpening.Tables[0].Rows.Add(dr);

                    List<AccountOpeningResponse> outerData = JsonConvert.DeserializeObject<List<AccountOpeningResponse>>(strResponse);
                    foreach (var obj in outerData)
                    {
                        strResponseCode = obj.Status;
                        strResponse = obj.Response;
                        strResponseMessage = obj.Data;

                     }

                   if (strResponseCode == "000")
                    {
                        //resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponse, strResponseMessage);
                        resp = CoreOmniApiBase.GenResponse(strResponseMessage, "", "");


                    }
                    else
                    {
                       // resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponse, strResponseMessage);
                        resp = CoreOmniApiBase.GenResponse(strResponseMessage, "", "");


                    }
                }
                else
                {
                    strResponseCode = "99";
                    strResponseMessage = strResponse;

                    resp = CoreOmniApiBase.GenResponse(strResponseCode, "failed", strResponseMessage);
                }


                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AccountOpening \n RESPONSE:{JsonConvert.SerializeObject(strResponseMessage).ToString()}\n"));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "RESPONSE", JsonConvert.SerializeObject(strResponseMessage).ToString(), ""));

                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300059", ex.Message.ToString());
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AccountOpening \n RESPONSE:{JsonConvert.SerializeObject(ex.Message).ToString()}\n"));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "RESPONSE", JsonConvert.SerializeObject(ex.Message).ToString(), ""));

                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }


        /// <summary>
        /// Use this API to open new Client in BRNET.  
        /// 
        /// 
        /// </summary>
        /// <remarks>
        /// Use this endpoint to open new Client in BRNET. 
        /// Add clienttype to add diffrent types of clients  Coporate->C  Individual->I GroUP->G
        /// </remarks>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300051, "FCS cannot be empty")]
        [SwaggerResponse(300052, "Gender cannot be empty")]
        [SwaggerResponse(300053, "Date Of Birth cannot be empty")]
        [SwaggerResponse(300056, "Mobile cannot be empty")]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Client not successfully Created")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult ClientOpening(ClientOpening myData)
        {

            string photo = string.Empty;
            string IdFront = string.Empty;
            string IdBack = string.Empty;
            string signature = string.Empty;
            string strResponse = string.Empty;
            string strCreatedBy = string.Empty;
            string strOpenedBy = string.Empty;

            DateTime DOB = new DateTime();



            BRDataSet dsClientCustomer = new BRDataSet();
            DataSet dsClientOpening = new DataSet();
            ClientOpeningResponse clientDetailsResponse = new ClientOpeningResponse(); 
            string[] strClientDetailsResponse = null;
            DataSet resp = null;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            if (myData == null)
            {
                //resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                var resError = new { Error = "300057, Invalid Request Parameters" };
                string ErrorString = JsonConvert.SerializeObject(resError);
                resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "ClientOpening", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ClientOpening \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                if (string.IsNullOrEmpty(myData.Mobile))
                {
                    //resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile cannot be empty");
                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp); 
                    var resError = new { Error = "300056, Mobile cannot be empty" };
                    string ErrorString = JsonConvert.SerializeObject(resError);
                    resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }


                if (string.IsNullOrEmpty(myData.FCS_Number))
                {
                    //resp = CoreOmniApiBase.GenResponse("99", "300051", "FCS cannot be empty");
                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    var resError = new { Error = "300051, FCS cannot be empty" };
                    string ErrorString = JsonConvert.SerializeObject(resError);
                    resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                if (string.IsNullOrEmpty(myData.ClientType))
                {
                    //resp = CoreOmniApiBase.GenResponse("99", "300051", "ClientType cannot be empty");
                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                    var resError = new { Error = "300051, ClientType cannot be empty" };
                    string ErrorString = JsonConvert.SerializeObject(resError);
                    resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }


                if (string.IsNullOrEmpty(myData.GenderID))
                {
                    //resp = CoreOmniApiBase.GenResponse("99", "300052", "Gender cannot be empty");
                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp); 

                    var resError = new { Error = "300052, Gender cannot be empty" };
                    string ErrorString = JsonConvert.SerializeObject(resError);
                    resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

          
                DataTable dtClientsMobile = CoreOmniApiBase.dtFromObject(myData, "dt_ClientsMobile");  //  dt_AccountCustomersMobile

                dtClientsMobile.Columns.Remove("photo");
                dtClientsMobile.Columns.Remove("IdFront");
                dtClientsMobile.Columns.Remove("IdBack");
                dtClientsMobile.Columns.Remove("signature");

                photo = myData.Photo;
                IdFront = myData.IdFront;
                IdBack = myData.IdBack;
                signature = myData.Signature;


                dtClientsMobile.Rows[0]["NewRecord"] = 1;
                dsClientCustomer.Tables.Add(dtClientsMobile);  //  AddEditiBankingAccountCustomerApi    dtAccountCustomer
                strResponse = OmniCore.AddEditiBankingNewClientApi(usrInfo, myData.LegacyAccountID, myData.Mobile, dsClientCustomer, photo, IdFront, IdBack, signature, true);

                if (string.IsNullOrEmpty(strResponse))
                {
                    var resError = new { Error = "Account not successfully Created" };  
                    string ErrorString = JsonConvert.SerializeObject(resError);
                    resp = CoreOmniApiBase.GenResponse(ErrorString, "", "");

                    //  resp = CoreOmniApiBase.GenResponse("99", "300059", "Account not successfully Created");
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ClientOpening \n RESPONSE:{JsonConvert.SerializeObject(ErrorString).ToString()}\n"));
                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "ClientOpening", "RESPONSE", JsonConvert.SerializeObject(ErrorString).ToString(), ""));


                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }



                //strClientDetailsResponse = strResponse.Split('|');

                if (strResponse.Length > 0)
                {

                    dsClientOpening.Tables.Add("dtClientOpening");
                    dsClientOpening.Tables[0].Columns.Add("Response");
                    DataRow dr = dsClientOpening.Tables[0].NewRow();
                    dr["Response"] = strResponse;
                    dsClientOpening.Tables[0].Rows.Add(dr);

                    List<ClientOpeningResponse> outerData = JsonConvert.DeserializeObject<List<ClientOpeningResponse>>(strResponse);
                    foreach (var obj in outerData)
                    {
                        strResponseCode = obj.Status;
                        strResponse = obj.Response;
                        strResponseMessage = obj.Data;

                    }


       

                    resp = CoreOmniApiBase.GenResponse(strResponseMessage, "", "");


                    //if (strClientDetailsResponse[1] != "99" && strClientDetailsResponse[0] != "99")
                    //if (strClientDetailsResponse[1] == "000")
                    //{
                    //    strResponseCode = strClientDetailsResponse[1];
                    //    strResponseMessage = "succcess";
                        //strResponseCode = strAccountDetailsResponse[0] != null ? strAccountDetailsResponse[0] : null;
                        //strResponseMessage = strAccountDetailsResponse.Length > 1 ? strAccountDetailsResponse[1] : strResponse;

                        //strResponseCode = strClientDetailsResponse[1] != null ? strClientDetailsResponse[1] : null;
                        //strResponseMessage = strClientDetailsResponse[1] == "000" || strClientDetailsResponse[1] == "00" ? "SUCCESS" : null;

                        //clientDetailsResponse.OurBranchID = strClientDetailsResponse.Length > 2 ? strClientDetailsResponse[3] : null;
                        //clientDetailsResponse.ClientID = strClientDetailsResponse.Length > 4 ? strClientDetailsResponse[5] : null;
                        //clientDetailsResponse.AccountID = strClientDetailsResponse.Length > 6 ? strClientDetailsResponse[7] : null;
                        //clientDetailsResponse.Name = strClientDetailsResponse.Length > 8 ? strClientDetailsResponse[9] : null;

                        //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);

                        //resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponse, strResponseMessage);
                    //}

                    //else
                    //{
                    //    if (strClientDetailsResponse[1] == "99")
                    //    {
                    //        strResponseCode = strClientDetailsResponse[1];
                    //        strResponseMessage = strClientDetailsResponse[3];
                    //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    //    }
                    //    else
                    //    {
                    //        strResponseCode = strClientDetailsResponse[0];
                    //        strResponseMessage = strClientDetailsResponse[1];
                    //        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    //    }
                    //}
                }
                else
                {
                    strResponseCode = "99";
                    strResponseMessage = strResponse;
                    //resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    resp = CoreOmniApiBase.GenResponse(strResponseMessage, "", "");
                

                }

                //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);
                //dsAccountOpening.Tables.Add("dtAccountOpening");
                //dsAccountOpening.Tables[0].Columns.Add("Response");
                //DataRow dr = dsAccountOpening.Tables[0].NewRow();
                // dr["Response"] = strResponse;
                //dsAccountOpening.Tables[0].Rows.Add(dr);

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: ClientOpening \n RESPONSE:{JsonConvert.SerializeObject(strResponseMessage).ToString()}\n"));
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "ClientOpening", "RESPONSE", JsonConvert.SerializeObject(strResponseMessage).ToString(), ""));


                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            catch (Exception ex)
            {

                resp = CoreOmniApiBase.GenResponse("99", "300059", ex.Message.ToString());

                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }


        /// <summary>
        /// This API will return customer statement at a specified period.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300059, "Invalid AccountID")]
        [SwaggerResponse(300062, "No Available Transactions")]
        [SwaggerResponse(300063, "Invalid Date Format")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult MiniStatement(MinistatementQuery myData)
        {
            string strAccountID = string.Empty;
            string strOurBranchID = string.Empty;
            bool ValidAccount = false;
            string OurBranchID = string.Empty;
            string AccountID = string.Empty;
            string strMobileNumber = string.Empty;
            string strClientID = string.Empty;
            //string FromDate = string.Empty;
            //string Todate = string.Empty;
            DataSet resp = null;
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            //DateTime dtFromDate = DateTime.MinValue;
            //DateTime dtTodate = DateTime.MinValue;

            int TopCountTrx = 0;
            BRDataSet dsMiniStatement = new BRDataSet();
            BRDataSet ds_Ministatement = new BRDataSet();
            IList<AccountStatementResponse> lstAccountStatementResponse = null;
            if (myData == null)
            {
                resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }
            try
            {
               

                //strAccountID = myData.AccountID;
                strOurBranchID = myData.OurBranchID;

                OurBranchID = myData.OurBranchID;
                AccountID = myData.AccountID;
                //strMobileNumber = myData.MobileNumber;
                //strClientID = myData.ClientID;
                //FromDate = myData.FromDate;
                //Todate = myData.Todate;
                //TopCountTrx = myData.TopCountTrx;

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MiniStatement \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));
                /*
                DateTime.TryParse(FromDate, out dtFromDate);

                if (dtFromDate== DateTime.MinValue)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300063", "Invalid Date Format");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                DateTime.TryParse(Todate, out dtTodate);
                if (dtTodate == DateTime.MinValue)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300063", "Invalid Date Format");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                FromDate = dtFromDate.ToShortDateString();
                Todate = dtTodate.ToShortDateString();
                */
                string mydata = string.Empty;

                mydata = JsonConvert.SerializeObject(myData).ToString();


                DataTable dtMiniStatement = CoreOmniApiBase.dtFromObject(myData, "dt_MiniStatement");
                dsMiniStatement.Tables.Add(dtMiniStatement);

                ValidAccount = OmniCore.GetMiniStatement(usrInfo, mydata,/* OurBranchID, strClientID AccountID, /*strMobileNumber,*/ dsMiniStatement, out ds_Ministatement);//RecordCount
                ds_Ministatement.Namespace = null;

                ValidAccount = ds_Ministatement.Tables.Count > 0 ? true : false;
                if (ValidAccount == true)
                {
                    strResponseCode = ds_Ministatement.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = ds_Ministatement.Tables[0].Rows[0][0].ToString();
                    if (ValidAccount == true)
                    {

                        lstAccountStatementResponse = ds_Ministatement.Tables[0].ToListof<AccountStatementResponse>();
                        resp = CoreOmniApiBase.GenResponse(strResponseMessage,"", "");
                      //  resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(lstAccountStatementResponse), strResponseMessage);
                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                    }
                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "RESPONSE", JsonConvert.SerializeObject(strResponseCode).ToString(), ""));
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MiniStatement \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));
                    
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                    }

                    //if (ValidAccount == true)
                    //{


                    //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //                               CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]),
                    //                               CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]));

                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //                        ds_Ministatement.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""),
                    //                        ds_Ministatement.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""));

                    //}
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300062", "No Available Transactions");

                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "RESPONSE", "No Available Transactions", ""));
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MiniStatement \n RESPONSE:{"No Available Transactions"}\n"));

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }

                }
                catch (Exception ex)
                {
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
                }

            }


            /// <summary>
            /// This API will return all the Customer Account Balance.
            /// </summary>
            /// <param name="myData"></param>
            /// <returns></returns>
            [HttpPost]
            [SwaggerResponseRemoveDefaults]
            [SwaggerResponse(300057, "Invalid Request Parameters")]
            [SwaggerResponse(300059, "Invalid AccountID")]
            [SwaggerResponse(500, "Internal Server Error")]
            private IHttpActionResult GetAccountBalance(AcBalQuery myData)
            {
                string strAccountID = string.Empty;
                string strOurBranchID = string.Empty;
                bool ValidAccount = false;
                BRDataSet ds_AccountBalance = new BRDataSet();
                BRDataSet dsAccountBalance = new BRDataSet();
                DataSet resp = null;
                string strResponseCode = string.Empty;
                string strResponseMessage = string.Empty;
                IList<AccountBalanceResponse> lstAccountBalanceResponse = null;
                if (myData == null)
                {
                    resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }

                try
                {
                    strAccountID = myData.AccountID;
                    //strOurBranchID = myData.OurBranchID;
                   string mydata = string.Empty;

                  //  mydata= myData;
                    //object tr = mydata.ToList();
                    //string json = JsonConvert.SerializeObject(myData, new JsonSerializerSettings
                    //{
                    //    StringEscapeHandling = StringEscapeHandling.EscapeHtml // Adjust serialization settings
                    //});


                    mydata = JsonConvert.SerializeObject(myData).ToString();

                    UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountBalance", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetAccountBalance \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));


                    DataTable dtAccountBalance = CoreOmniApiBase.dtFromObject(myData, "dt_AccountBalance");
                    dsAccountBalance.Tables.Add(dtAccountBalance);

                    ValidAccount = OmniCore.GetAccountBalance(usrInfo, mydata, dsAccountBalance, out ds_AccountBalance);
                    ds_AccountBalance.Namespace = null;

                    if (ValidAccount == true)
                    {
                        strResponseCode = ds_AccountBalance.Tables[0].Rows[0][0].ToString();
                        strResponseMessage = "SUCCESS";
                        if (strResponseMessage == "SUCCESS")
                        {

                            lstAccountBalanceResponse = ds_AccountBalance.Tables[0].ToListof<AccountBalanceResponse>();

                        // lstAccountOpeningResponse = ds_AccountCustomer.Tables[0].ToListof<AccountsDetailResponse>();

                        Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountBalance", "RESPONSE", JsonConvert.SerializeObject(strResponseCode).ToString(), ""));
                        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetAccountBalance \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));


                        resp = CoreOmniApiBase.GenResponse(strResponseCode, "", "");
                        }
                        else
                        {
                            resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                        }

                    //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //                       CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountBalance.Tables[0]),
                    //                       CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_AccountBalance.Tables[0]));

                    Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetAccountBalance", "RESPONSE", JsonConvert.SerializeObject(strResponseCode).ToString(), ""));
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetAccountBalance \n RESPONSE:{JsonConvert.SerializeObject(strResponseCode).ToString()}\n"));

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                    }
                    else
                    {
                        resp = CoreOmniApiBase.GenResponse("99", "300059", "Account does not exists");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    }




                }
                catch (Exception ex)
                {
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
                }

            }



           }
            }



            ///// <summary>
            ///// Use this API to open new Client and Account in BRNET.  
            ///// 
            ///// 
            ///// </summary>
            ///// <remarks>
            ///// For a new Client leave clientID as null and a new Account will created and for an existing Client
            ///// supply the clientID and a new Account is added based on the specified product.
            ///// </remarks>
            ///// <param name="myData"></param>
            ///// <returns></returns>
            //[HttpPost]
            //[SwaggerResponseRemoveDefaults]
            //[SwaggerResponse(300051, "FCS cannot be empty")]
            //[SwaggerResponse(300052, "Gender cannot be empty")]
            //[SwaggerResponse(300053, "Date Of Birth cannot be empty")]
            //[SwaggerResponse(300054, "LegacyAccountID cannot be empty")]
            //[SwaggerResponse(300055, "LegacyClientID cannot be empty")]
            //[SwaggerResponse(300056, "Mobile cannot be empty")]
            //[SwaggerResponse(300057, "Invalid Request Parameters")]
            //[SwaggerResponse(300059, "Account not successfully Created")]

            //[SwaggerResponse(500, "Internal Server Error")]

            //public IHttpActionResult AccountOpening(AccountOpening myData)
            //{

            //    string photo = string.Empty;
            //    string IdFront = string.Empty;
            //    string IdBack = string.Empty;
            //    string signature = string.Empty;
            //    string strResponse = string.Empty;
            //    string strCreatedBy = string.Empty;
            //    string strOpenedBy = string.Empty;

            //    DateTime DOB = new DateTime();



            //    BRDataSet dsAccountCustomer = new BRDataSet();
            //    //BRDataSet dsAccountOpening = new BRDataSet();
            //    DataSet dsAccountOpening = new DataSet();
            //    AccountOpeningResponse accountDetailsResponse = new AccountOpeningResponse(); ;
            //    string[] strAccountDetailsResponse = null;
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
            //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
            //        Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "AccountOpening", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
            //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: AccountOpening \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

            //        //if (string.IsNullOrEmpty(myData.LegacyAccountID))
            //        //{
            //        //    resp = CoreOmniApiBase.GenResponse("99", "300054", "LegacyAccountID cannot be empty");

            //        //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        //}

            //        //if (string.IsNullOrEmpty(myData.LegacyClientID))
            //        //{
            //        //    resp = CoreOmniApiBase.GenResponse("99", "300055", "LegacyClientID cannot be empty");

            //        //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        //}


            //        if (string.IsNullOrEmpty(myData.Mobile))
            //        {
            //            resp = CoreOmniApiBase.GenResponse("99", "300056", "Mobile cannot be empty");

            //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        }


            //        if (string.IsNullOrEmpty(myData.FCS_Number))
            //        {
            //            resp = CoreOmniApiBase.GenResponse("99", "300051", "FCS cannot be empty");

            //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        }


            //        if (string.IsNullOrEmpty(myData.GenderID))
            //        {
            //            resp = CoreOmniApiBase.GenResponse("99", "300052", "Gender cannot be empty");

            //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        }

            //        /*
            //        if (string.IsNullOrEmpty(myData.DateOfBirth))
            //        {
            //            resp = CoreOmniApiBase.GenResponse("99", "300053", "Date Of Birth cannot be empty");

            //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

            //        }

            //        DateTime.TryParse(myData.DateOfBirth, out DOB);
            //        myData.DateOfBirth = DOB.ToString("dd MMM yyyy");
            //        */
                    //        DataTable dtAccountCustomer = CoreOmniApiBase.dtFromObject(myData, "dt_AccountCustomersMobile");

                    //        dtAccountCustomer.Columns.Remove("photo");
                    //        dtAccountCustomer.Columns.Remove("IdFront");
                    //        dtAccountCustomer.Columns.Remove("IdBack");
                    //        dtAccountCustomer.Columns.Remove("signature");

                    //        photo = myData.Photo;
                    //        IdFront = myData.IdFront;
                    //        IdBack = myData.IdBack;
                    //        signature = myData.Signature;


                    //        dtAccountCustomer.Rows[0]["NewRecord"] = 1;

                    //        dsAccountCustomer.Tables.Add(dtAccountCustomer);
                    //        strResponse = OmniCore.AddEditiBankingAccountCustomerApi(usrInfo, myData.LegacyAccountID, myData.Mobile, dsAccountCustomer, photo, IdFront, IdBack, signature, true);

                    //        if (string.IsNullOrEmpty(strResponse))
                    //        {

                    //            resp = CoreOmniApiBase.GenResponse("99", "300059", "Account not successfully Created");

                    //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //        }



                    //        strAccountDetailsResponse = strResponse.Split('|');

                    //        if (strAccountDetailsResponse.Length > 0)
                    //        {
                    //            if (strAccountDetailsResponse[1] != "99" && strAccountDetailsResponse[0] != "99")
                    //            {
                    //                //strResponseCode = strAccountDetailsResponse[0] != null ? strAccountDetailsResponse[0] : null;
                    //                //strResponseMessage = strAccountDetailsResponse.Length > 1 ? strAccountDetailsResponse[1] : strResponse;

                    //                strResponseCode = strAccountDetailsResponse[1] != null ? strAccountDetailsResponse[1] : null;
                    //                strResponseMessage = strAccountDetailsResponse[1] == "000" || strAccountDetailsResponse[1] == "00" ? "SUCCESS" : null;

                    //                accountDetailsResponse.OurBranchID = strAccountDetailsResponse.Length > 2 ? strAccountDetailsResponse[3] : null;
                    //                accountDetailsResponse.ClientID = strAccountDetailsResponse.Length > 4 ? strAccountDetailsResponse[5] : null;
                    //                accountDetailsResponse.AccountID = strAccountDetailsResponse.Length > 6 ? strAccountDetailsResponse[7] : null;
                    //                accountDetailsResponse.Name = strAccountDetailsResponse.Length > 8 ? strAccountDetailsResponse[9] : null;

                    //                //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);

                    //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);
                    //            }

                    //            else
                    //            {
                    //                if (strAccountDetailsResponse[1] == "99")
                    //                {
                    //                    strResponseCode = strAccountDetailsResponse[1];
                    //                    strResponseMessage = strAccountDetailsResponse[3];
                    //                    resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    //                }
                    //                else
                    //                {
                    //                    strResponseCode = strAccountDetailsResponse[0];
                    //                    strResponseMessage = strAccountDetailsResponse[1];
                    //                    resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            strResponseCode = "99";
                    //            strResponseMessage = strResponse;
                    //            resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage);
                    //        }

                    //        //resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(accountDetailsResponse), strResponseMessage);
                    //        //dsAccountOpening.Tables.Add("dtAccountOpening");
                    //        //dsAccountOpening.Tables[0].Columns.Add("Response");
                    //        //DataRow dr = dsAccountOpening.Tables[0].NewRow();
                    //        // dr["Response"] = strResponse;
                    //        //dsAccountOpening.Tables[0].Rows.Add(dr);

                    //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //        resp = CoreOmniApiBase.GenResponse("99", "300059", ex.Message.ToString());

                    //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);

                    //        //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
                    //    }
                    //}




                    ///// <summary>
                    ///// This API will return customer statement at a specified period.
                    ///// </summary>
                    ///// <param name="myData"></param>
                    ///// <returns></returns>
                    //[HttpPost]
                    //[SwaggerResponseRemoveDefaults]
                    //[SwaggerResponse(300057, "Invalid Request Parameters")]
                    //[SwaggerResponse(300059, "Invalid AccountID")]
                    //[SwaggerResponse(300062, "No Available Transactions")]
                    //[SwaggerResponse(300063, "Invalid Date Format")]
                    //[SwaggerResponse(500, "Internal Server Error")]
                    //public IHttpActionResult MiniStatement(CustQuery myData)
                    //{
                    //    string strAccountID = string.Empty;
                    //    string strOurBranchID = string.Empty;
                    //    bool ValidAccount = false;
                    //    string OurBranchID = string.Empty;
                    //    string AccountID = string.Empty;
                    //    string strMobileNumber = string.Empty;
                    //    string strCustomerIdentifier = string.Empty;
                    //    //string FromDate = string.Empty;
                    //    //string Todate = string.Empty;
                    //    DataSet resp = null;
                    //    string strResponseCode = string.Empty;
                    //    string strResponseMessage = string.Empty;
                    //    //DateTime dtFromDate = DateTime.MinValue;
                    //    //DateTime dtTodate = DateTime.MinValue;

                    //    int TopCountTrx = 0;
                    //    BRDataSet ds_Ministatement = new BRDataSet();
                    //    IList<AccountStatementResponse> lstAccountStatementResponse = null;
                    //    if (myData == null)
                    //    {
                    //        resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                    //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //    }
                    //    try
                    //    {
                    //        strAccountID = myData.AccountID;
                    //        strOurBranchID = myData.OurBranchID;

                    //        OurBranchID = myData.OurBranchID;
                    //        AccountID = myData.AccountID;
                    //        strMobileNumber = myData.MobileNumber;
                    //        strCustomerIdentifier = myData.CustomerIdentifier;
                    //        //FromDate = myData.FromDate;
                    //        //Todate = myData.Todate;
                    //        //TopCountTrx = myData.TopCountTrx;

                    //        UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                    //        //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "MiniStatement", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), ""));
                    //        Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: MiniStatement \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));
                    //        /*
                    //        DateTime.TryParse(FromDate, out dtFromDate);

                    //        if (dtFromDate== DateTime.MinValue)
                    //        {
                    //            resp = CoreOmniApiBase.GenResponse("99", "300063", "Invalid Date Format");
                    //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //        }

                    //        DateTime.TryParse(Todate, out dtTodate);
                    //        if (dtTodate == DateTime.MinValue)
                    //        {
                    //            resp = CoreOmniApiBase.GenResponse("99", "300063", "Invalid Date Format");
                    //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //        }

                    //        FromDate = dtFromDate.ToShortDateString();
                    //        Todate = dtTodate.ToShortDateString();
                    //        */
                    //        ValidAccount = OmniCore.GetMiniStatement(usrInfo, OurBranchID, strCustomerIdentifier, AccountID, strMobileNumber, out ds_Ministatement);//RecordCount
                    //        ds_Ministatement.Namespace = null;

                    //        ValidAccount = ds_Ministatement.Tables.Count > 0 ? true : false;
                    //        if (ValidAccount == true)
                    //        {
                    //            strResponseCode = ds_Ministatement.Tables[0].Rows[0][0].ToString();
                    //            strResponseMessage = ds_Ministatement.Tables[0].Rows[0][1].ToString();
                    //            if (ds_Ministatement.Tables[1].Rows.Count > 1)
                    //            {

                    //                lstAccountStatementResponse = ds_Ministatement.Tables[1].ToListof<AccountStatementResponse>();

                    //                resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(lstAccountStatementResponse), strResponseMessage);
                    //            }
                    //            else
                    //            {
                    //                resp = CoreOmniApiBase.GenResponse(strResponseCode, strResponseMessage, strResponseMessage);
                    //            }

                    //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);



                    //        }

                    //        //if (ValidAccount == true)
                    //        //{


                    //        //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //        //                               CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]),
                    //        //                               CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_Ministatement.Tables[0]));

                    //        //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute,
                    //        //                        ds_Ministatement.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""),
                    //        //                        ds_Ministatement.GetXml().ToString().Replace(" xmlns=\"BREntities.BRDataSet\"", ""));

                    //        //}
                    //        else
                    //        {
                    //            resp = CoreOmniApiBase.GenResponse("99", "300062", "No Available Transactions");
                    //            return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                    //        }

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
                    //    }

                    //}



             
