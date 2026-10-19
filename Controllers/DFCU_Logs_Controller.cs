using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Jwt.Filters;
using BRBase;
using WebApi.Jwt.Models;
using System.Configuration;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using BR.ApplicationBlocks.Data;
using BR.DBClient;
using System.Collections.Specialized;
using OmniApi.CoreLibraries;
using WebApi.Jwt;
using Swashbuckle.Swagger.Annotations;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniApi.Models;

namespace CoreOmniApi.Controllers
{
    [JwtAuthentication]
    private class DFCU_LogsController : ApiController
    {

        /// <summary>
        /// This API will return all the products and their specific details.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult LogDFCURequestsResponses(DFCU_Request_Response myData)
        {   try
            {
             
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, myData.MethodName, myData.Action, myData.strAddEditAPIAuditLogXML , myData.AuditID));
                
                    return CreatedAtRoute("True", "Successfully Logged", "Successfully Logged");

            }
            catch (Exception ex)
            {
                return CreatedAtRoute("Failed", CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

     }
}

