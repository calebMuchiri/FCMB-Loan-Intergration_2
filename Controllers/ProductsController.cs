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

namespace CoreOmniApi.Controllers
{
    [JwtAuthentication]
    public class ProductsController : ApiController
    {

        /// <summary>
        /// This API will return all the products and their specific details.
        /// </summary>
        /// <param name="myData"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(300057, "Invalid Request Parameters")]
        //[SwaggerResponse(300061, "Invalid Product")]
        [SwaggerResponse(500, "Internal Server Error")]
        private IHttpActionResult GetProductList(Products myData)
        {
            string ProductID = string.Empty;
            string CurrencyID = string.Empty;
            string ProductType = string.Empty;

            string strAccountID = string.Empty;
            bool ValidAccount = false;
            BRDataSet dsProduct = new BRDataSet();
            BRDataSet dsProductList = new BRDataSet();
            string strResponseCode = string.Empty;
            string strResponseMessage = string.Empty;
            IList<ProductsResponse> lstProductsResponse = null;
            DataSet resp = null;
            //if (myData==null)
            //{
            //    DataSet resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
            //    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);           bb      
            //}

            try
            {
                //ProductID = myData.ProductID;
                //CurrencyID = myData.CurrencyID;
                //ProductType = myData.ProductType;

                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();

                //Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetProductList", "REQUEST", string.Empty, ""));

                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetProductList", "REQUEST", JsonConvert.SerializeObject(myData).ToString(), "")); 

                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetProductList \n REQUEST:{JsonConvert.SerializeObject(myData).ToString()}\n"));

                DataTable dtProduct = CoreOmniApiBase.dtFromObject(myData, "dt_Product");
                dsProduct.Tables.Add(dtProduct);

                ValidAccount = OmniCore.GetProductList(usrInfo, dsProduct, out dsProductList);
                dsProductList.Namespace = null;


                dsProductList.Namespace = null;

                ValidAccount = dsProductList.Tables.Count > 0 ? true : false;
                if (ValidAccount == true)
                {
                    strResponseCode = dsProductList.Tables[0].Rows[0][0].ToString();
                    strResponseMessage = dsProductList.Tables[0].Rows[0][1].ToString();
                    if (dsProductList.Tables[1].Rows.Count > 0)
                    {

                        lstProductsResponse = dsProductList.Tables[1].ToListof<ProductsResponse>();

                        resp = CoreOmniApiBase.GenResponse(strResponseCode, JsonConvert.SerializeObject(lstProductsResponse), strResponseMessage);
                    }

                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);


                }

             
                else
                {
                     resp = CoreOmniApiBase.GenResponse("99", "300061", "Products not defined");
                    Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetProductList \n RESPONSE:{JsonConvert.SerializeObject(myData).ToString()}\n"));
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
                }
    
            }
            catch (Exception ex)
            {
                Task.Run(() => CoreOmniApiBase.LogEvent($"METHOD: GetProductList \n RESPONSE:{JsonConvert.SerializeObject(myData).ToString()}\n"));
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }

        }

     }
}

