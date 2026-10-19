using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Jwt.Filters
{
    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        private object ResponseMessage;

        public AddChallengeOnUnauthorizedResult(string reasonPhrase, HttpRequestMessage request, object responseMessage)
        {
            ReasonPhrase = reasonPhrase;
            Request = request;
            ResponseMessage = responseMessage;
        }

        public string ReasonPhrase { get; private set; }

        public HttpRequestMessage Request { get; private set; }

        public AuthenticationHeaderValue Challenge { get; set; }

        public IHttpActionResult InnerResult { get; set; }

        //public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        //{
        //    HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

        //    if (response.StatusCode == HttpStatusCode.Unauthorized)
        //    {
        //        // Only add one challenge per authentication scheme.
        //        if (response.Headers.WwwAuthenticate.All(h => h.Scheme != Challenge.Scheme))
        //        {
        //            response.Headers.WwwAuthenticate.Add(Challenge);
        //        }
        //    }

        //    return response;
        //}

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }
        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            System.Net.Http.Formatting.MediaTypeFormatter jsonFormatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
            response.Content = new System.Net.Http.ObjectContent<object>(ResponseMessage, jsonFormatter);
            response.RequestMessage = Request;
            response.ReasonPhrase = ReasonPhrase;
            return response;
        }

    }
}