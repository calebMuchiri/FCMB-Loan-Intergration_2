using BR.Configuration;
using BRBase;
using BRCoreDALLibrary;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using System.Xml;
using WebApi.Jwt;
using WebApi.Jwt.Filters;
using WebApi.Jwt.Models;



namespace OmniApi.Controllers
{

    public class TokenController : ApiController
    {
        string usrKey = BRConfigurationManager.BRAppSettings["usrKey"];
                  


        /// <summary>
        /// This gives you time bound access token to call the allowed APIs
        /// </summary>
        /// <param name="usrData"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(300058, "Invalid Credentials")]
        [SwaggerResponse(500, "Internal Server Error")]
        public IHttpActionResult Generate(UserInfoData usrData)
        {
            string consumerKey = string.Empty;          
            string consumerSecret = string.Empty;
            Int64 tokenExpiry = Convert.ToInt64(BRConfigurationManager.BRAppSettings["exp"]);
            string Token = string.Empty;

            if (usrData == null)
            {
                DataSet resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                if (usrData != null)
                {
                    consumerKey = CoreOmniApiBase.DeCrypt(usrData.ConsumerKey, usrKey);

                    consumerSecret = CoreOmniApiBase.DeCrypt(usrData.ConsumerSecret, consumerKey);//

                    if (CheckUser(consumerKey, consumerSecret, usrData))
                    {
                        DataSet dsResponse = new DataSet();
                        dsResponse.Tables.Add("dtResponse");
                        dsResponse.Tables[0].Columns.Add("accessToken");
                        dsResponse.Tables[0].Columns.Add("tokenExpiry");

                        Token = JwtManager.GenerateToken(consumerKey, tokenExpiry);

                        DataRow dr = dsResponse.Tables[0].NewRow();
                        dr["accessToken"] = Token;
                        dr["tokenExpiry"] = tokenExpiry;

                        dsResponse.Tables[0].Rows.Add(dr);


                        return new JSONActionResult(CoreOmniApiBase.DtToJSON(dsResponse.Tables[0]));
                        
                    }
                    else
                    {
                        DataSet resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Credentials");
                        return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp,resp);
                    }

                }
                else
                {
                    DataSet resp = CoreOmniApiBase.GenResponse("99", "300058", "Invalid Credentials");
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp,resp);
                }
            }
            catch(HttpResponseException ex)
            {
                
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, new HttpResponseException(HttpStatusCode.Unauthorized), new HttpResponseException(HttpStatusCode.Unauthorized));
            }
        }


        private bool CheckUser(string username, string password, UserInfoData usrData)
        {
            DataTable usrInfo = new DataTable();
            
            DataTable dtUsrData = CoreOmniApiBase.dtFromObject(usrData, "usrData");
            dtUsrData.Rows[0]["ConsumerKey"] = username;
            dtUsrData.Rows[0]["ConsumerSecret"] = password;

            CoreOmniApiBase.gOperatorID = username;

            bool ValidUser = CoreOmniApiBase.ValidateUser(dtUsrData);
            return ValidUser;
        }


        [Route(""), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public HttpResponseMessage RedirectToSwaggerUi()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Found);
            httpResponseMessage.Headers.Location = new Uri("/ui/index", UriKind.Relative);
            return httpResponseMessage;
        }
    }
}
