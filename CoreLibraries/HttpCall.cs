using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace OmniApi.CoreLibraries
{
    /// <summary>
    /// Reusable HTTP client wrapper for external API calls with built-in token generation and error handling.
    /// Uses a static HttpClient instance to avoid socket exhaustion while providing comprehensive logging.
    /// </summary>
    public class HttpCall
    {
        // Reuse a single HttpClient instance per app to avoid socket exhaustion.
        private static readonly HttpClient _httpClient;

        static HttpCall()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            // Set TLS 1.2 for all requests
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<HttpCallResponse> GetDataAsync(
      string url,
      string clientId,
      string password,
      string subscriptionKey = null,
      Action<HttpRequestMessage> additionalHeaders = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            CoreOmniApiBase.LogEvent(
                "[HttpCall] GET START | RequestId:" + requestId +
                " | URL:" + url +
                " | Timestamp:" + startTime.ToString("O"));

            try
            {
                var utcNow = DateTime.UtcNow;
                var utcTimestamp = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                var xToken = GetXTokenHeader(utcNow, clientId, password);

                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // Accept header (match Postman)
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                    // User-Agent (important for WAF)
                    request.Headers.UserAgent.ParseAdd("PostmanRuntime/7.32.0");

                    // Cache
                    request.Headers.CacheControl = new CacheControlHeaderValue
                    {
                        NoCache = true
                    };

                    // Required FCMB headers (CASE-SENSITIVE)
                    request.Headers.Add("UTCTimestamp", utcTimestamp);
                    request.Headers.Add("x-token", xToken);
                    request.Headers.Add("client_Id", clientId);

                    if (!string.IsNullOrWhiteSpace(subscriptionKey))
                    {
                        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    }

                    if (additionalHeaders != null)
                    {
                        additionalHeaders(request);
                    }

                    // Log outgoing headers
                    foreach (var header in request.Headers)
                    {
                        CoreOmniApiBase.LogEvent(
                            "[HttpCall] Header OUT | " + header.Key + ": " +
                            string.Join(",", header.Value));
                    }

                    HttpResponseMessage response;
                    try
                    {
                        response = await _httpClient.SendAsync(request)
                                                    .ConfigureAwait(false);
                    }
                    catch (Exception sendEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent(
                            "[HttpCall] RequestId:" + requestId +
                            " - SEND FAILED after " + stopwatch.ElapsedMilliseconds +
                            "ms | " + sendEx);

                        return new HttpCallResponse
                        {
                            Success = false,
                            ErrorMessage = sendEx.Message,
                            RequestId = requestId
                        };
                    }

                    string responseBody;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync()
                                                             .ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent(
                            "[HttpCall] RequestId:" + requestId +
                            " - READ FAILED after " + stopwatch.ElapsedMilliseconds +
                            "ms | " + readEx);

                        return new HttpCallResponse
                        {
                            Success = false,
                            StatusCode = (int)response.StatusCode,
                            ErrorMessage = readEx.Message,
                            RequestId = requestId
                        };
                    }

                    stopwatch.Stop();

                    CoreOmniApiBase.LogEvent(
                        "[HttpCall] RequestId:" + requestId +
                        " - RESPONSE in " + stopwatch.ElapsedMilliseconds +
                        "ms | Status=" + (int)response.StatusCode +
                        " (" + response.ReasonPhrase + ")" +
                        " | Length=" + (responseBody != null ? responseBody.Length : 0));

                    if (!response.IsSuccessStatusCode)
                    {
                        var preview = responseBody != null && responseBody.Length > 200
                            ? responseBody.Substring(0, 200) + "..."
                            : responseBody ?? "<empty>";

                        CoreOmniApiBase.LogEvent(
                            "[HttpCall] RequestId:" + requestId +
                            " - NON-SUCCESS | Body Preview: " + preview);

                        return new HttpCallResponse
                        {
                            Success = false,
                            StatusCode = (int)response.StatusCode,
                            Content = responseBody,
                            ErrorMessage = "HTTP " + (int)response.StatusCode +
                                           " (" + response.ReasonPhrase + ")",
                            RequestId = requestId
                        };
                    }

                    CoreOmniApiBase.LogEvent(
                        "[HttpCall] RequestId:" + requestId + " - SUCCESS");

                    return new HttpCallResponse
                    {
                        Success = true,
                        StatusCode = (int)response.StatusCode,
                        Content = responseBody,
                        RequestId = requestId
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                CoreOmniApiBase.LogEvent(
                    "[HttpCall] RequestId:" + requestId +
                    " - UNHANDLED EXCEPTION after " +
                    stopwatch.ElapsedMilliseconds + "ms | " + ex);

                return new HttpCallResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    RequestId = requestId
                };
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent(
                    "[HttpCall] GET END | RequestId:" + requestId +
                    " | Start:" + startTime.ToString("O") +
                    " | End:" + endTime.ToString("O"));
            }
        }



        /// <summary>
        /// Executes a POST request with x-token authentication and comprehensive logging.
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="jsonBody">JSON request body</param>
        /// <param name="clientId">Client ID for authentication</param>
        /// <param name="password">Password for token generation</param>
        /// <param name="subscriptionKey">Optional subscription key</param>
        /// <param name="additionalHeaders">Optional additional headers</param>
        /// <returns>Response content or error message</returns>
        public async Task<HttpCallResponse> PostDataAsync(
            string url, 
            string jsonBody, 
            string clientId, 
            string password, 
            string subscriptionKey = null,
            Action<HttpRequestMessage> additionalHeaders = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            CoreOmniApiBase.LogEvent($"[HttpCall] POST START | RequestId:{requestId} | URL:{url} | Timestamp:{startTime:O}");

            try
            {
                var utcdate = DateTime.UtcNow;
                var xtoken = GetXTokenHeader(utcdate, clientId, password);
                var utcTime = utcdate.ToString("yyyy-MM-ddTHH:mm:ss.fff");

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    // Set headers
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Add("UTCTimestamp", utcTime);
                    request.Headers.Add("x-token", xtoken);

                    if (!string.IsNullOrEmpty(clientId))
                        request.Headers.Add("client_id", clientId);

                    if (!string.IsNullOrEmpty(subscriptionKey))
                        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    // Apply any additional headers
                    additionalHeaders?.Invoke(request);

                    // Set content
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - Sending POST | BodyLength:{jsonBody?.Length ?? 0}");

                    HttpResponseMessage response;
                    try
                    {
                        response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                    }
                    catch (Exception httpEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - HTTP send failed after {stopwatch.ElapsedMilliseconds}ms. Error: {httpEx.Message}\n{httpEx.StackTrace}");
                        return new HttpCallResponse
                        {
                            Success = false,
                            ErrorMessage = $"HTTP request failed: {httpEx.Message}",
                            RequestId = requestId
                        };
                    }

                    string responseBody;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception readEx)
                    {
                        stopwatch.Stop();
                        CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - Failed reading response after {stopwatch.ElapsedMilliseconds}ms. Error: {readEx.Message}\n{readEx.StackTrace}");
                        return new HttpCallResponse
                        {
                            Success = false,
                            ErrorMessage = $"Failed to read response: {readEx.Message}",
                            StatusCode = (int)response.StatusCode,
                            RequestId = requestId
                        };
                    }

                    stopwatch.Stop();
                    CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - Response received in {stopwatch.ElapsedMilliseconds}ms. StatusCode={(int)response.StatusCode} ({response.ReasonPhrase}). ResponseLength={responseBody?.Length ?? 0}");

                    if (!response.IsSuccessStatusCode)
                    {
                        var preview = responseBody != null && responseBody.Length > 200 
                            ? responseBody.Substring(0, 200) + "..." 
                            : responseBody ?? "<empty>";
                        CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - Non-success status. Content preview: {preview}");
                        
                        return new HttpCallResponse
                        {
                            Success = false,
                            Content = responseBody,
                            StatusCode = (int)response.StatusCode,
                            ErrorMessage = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})",
                            RequestId = requestId
                        };
                    }

                    CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - SUCCESS");
                    return new HttpCallResponse
                    {
                        Success = true,
                        Content = responseBody,
                        StatusCode = (int)response.StatusCode,
                        RequestId = requestId
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorMsg = $"Unexpected error: {ex.Message}";
                CoreOmniApiBase.LogEvent($"[HttpCall] RequestId:{requestId} - UNHANDLED EXCEPTION after {stopwatch.ElapsedMilliseconds}ms: {errorMsg}\n{ex.StackTrace}");
                return new HttpCallResponse
                {
                    Success = false,
                    ErrorMessage = errorMsg,
                    RequestId = requestId
                };
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                CoreOmniApiBase.LogEvent($"[HttpCall] POST END | RequestId:{requestId} | Start:{startTime:O} | End:{endTime:O}");
            }
        }

        /// <summary>
        /// Generates x-token header using SHA512 hash of timestamp + clientID + password.
        /// </summary>
        private string GetXTokenHeader(DateTime utcdate, string clientId, string password)
        {
            var date = utcdate.ToString("yyyy-MM-ddHHmmss");
            var data = date + clientId + password;
            return SHA512(data);
        }

        /// <summary>
        /// Computes SHA512 hash of input string.
        /// </summary>
        private string SHA512(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("x2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        /// <summary>
        /// Masks sensitive data for logging (shows first 3 and last 3 characters).
        /// </summary>
        private string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "null";
            
            if (data.Length <= 6)
                return "*****";
            
            return data.Substring(0, 3) + "..." + data.Substring(data.Length - 3);
        }
    }

    /// <summary>
    /// Response object from HTTP calls containing success status, content, and error information.
    /// </summary>
    public class HttpCallResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string RequestId { get; set; }
    }
}
