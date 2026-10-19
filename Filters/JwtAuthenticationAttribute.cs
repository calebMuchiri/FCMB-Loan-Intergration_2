using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace WebApi.Jwt.Filters
{
    
    public class JwtAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }
        public bool AllowMultiple = false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            System.Net.Http.Formatting.MediaTypeFormatter jsonFormatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
            

            //Lets check whether the request has the required Authorization header. If not set the Error
            var authHeader = context.Request.Headers.Authorization;
            var token = authHeader.Parameter;
            var principal = await AuthenticateJwtToken(token);
            dynamic ci = null;
            if (principal != null)
            {
                ci = principal.Identity as ClaimsIdentity;
            }

            //am using the default "Bearer" scheme, lets validate it
            if (authHeader == null || authHeader.Scheme != "Bearer")
            {                
                context.ErrorResult = context.ErrorResult = new AddChallengeOnUnauthorizedResult("unauthorized", context.Request,
                    new { Response = 401.01, Message = "Invalid Scheme" });
            }

            //if the Token has not been supplied...scream
            if (string.IsNullOrEmpty(token))
            {
                context.ErrorResult = context.ErrorResult = new AddChallengeOnUnauthorizedResult("unauthorized", context.Request,
                    new { Response = 402, Message = "Token Missing" });
            }

            if (principal == null)
            {
                context.ErrorResult = context.ErrorResult = new AddChallengeOnUnauthorizedResult("unauthorized", context.Request,
                new { Response = 403, Message = "Invalid token" });
            }

             ///"IsAuthenticated" is false if the token has expired lets set the damn error
            else if (!ci.IsAuthenticated)
            {
                context.ErrorResult = new AddChallengeOnUnauthorizedResult("unauthorized", context.Request,
                    new { Response = 405, Message = "Your token has expired" });
            }
        }



        private static bool ValidateToken(string token, out string username)
        {
            username = null;
            //token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImRhdmlzIiwibmJmIjoxNTAzNTc5OTQzLCJleHAiOjE1MDM1ODExNDMsImlhdCI6MTUwMzU3OTk0M30.1VqSj-dbb-5Ef7aZmOvgVZk6k2IN1dLDrjRobvh_h8E";
            try
            {
                var simplePrinciple = JwtManager.GetPrincipal(token);
                var identity = simplePrinciple.Identity as ClaimsIdentity;

                if (identity == null)
                    return false;

                if (!identity.IsAuthenticated)
                    return false;

                var usernameClaim = identity.FindFirst(ClaimTypes.Name);
                username = usernameClaim.Value;

                if (string.IsNullOrEmpty(username))
                    return false;

                // More validate to check whether username exists in system

                CoreOmniApiBase.gSessionID = token;
                CoreOmniApiBase.gOperatorID = username;


            return true;
            }
            catch(Exception ex)
            {
             return false;
            }
            
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;

            if (ValidateToken(token, out username))
            {
                // based on username to get more information from database in order to build local identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                    // Add more claims if needed: Roles, ...
                };

                var identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);
            }

            return Task.FromResult<IPrincipal>(null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Bearer", parameter);
        }

        bool IFilter.AllowMultiple
        {
            get { return false; }
        }
    }
}
