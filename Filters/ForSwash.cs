using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using WebApi.Jwt.Filters;

namespace OmniApi.Filters
{
    public class ForSwash : IOperationFilter
    {

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var filterPipeline = apiDescription.ActionDescriptor.GetFilterPipeline();
            var isAuthorized = filterPipeline
                                   .Select(filterInfo => filterInfo.Instance)
                                   .Any(filter => filter is JwtAuthenticationAttribute);

            var allowAnonymous = apiDescription.ActionDescriptor
                .GetCustomAttributes<AllowAnonymousAttribute>()
                .Any();

            if (isAuthorized && !allowAnonymous)
            {
                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    description = "Bearer token (e.g. Bearer tokenhere)",
                    required = true,
                    type = "string"
                });
            }
        }
    

    //public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
    //{
    //    if (operation.parameters != null)
    //    {
    //        operation.parameters.Add(new Parameter
    //        {
    //            name = "Authorization",
    //            @in = "header",
    //            description = "access token",
    //            required = false,
    //            type = "string"
    //        });
    //    }
    //}

    //public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
    //{

    //    var filterPipeline = apiDescription.ActionDescriptor.FilterDescriptors;
    //    var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
    //    var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

    //    if (isAuthorized && !allowAnonymous)
    //    {
    //        if (operation.Parameters == null)
    //            operation.Parameters = new List<IParameter>();

    //        operation.Parameters.Add(new NonBodyParameter
    //        {
    //            Name = "Authorization",
    //            In = "header",
    //            Description = "access token",
    //            Required = true,
    //            Type = "string"
    //        });
    //    }
    //}

}
}