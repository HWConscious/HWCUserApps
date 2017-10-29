using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HWC_UserApp.UserApp.Models
{
    /// <summary>
    /// REST client
    /// </summary>
    public class RestClient
    {
        #region Data members

        private readonly HttpVerb _httpMethod;
        private readonly Uri _endPoint;
        private readonly Dictionary<string, string> _headers;
        private readonly string _contentType;
        private string _content { get; set; }
        private int? _timeoutInMs { get; set; }

        /// <summary>
        /// REST request methods.
        /// </summary>
        public enum HttpVerb
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        #endregion

        #region Initialize

        /// <summary>
        /// REST client constructor
        /// </summary>
        /// <param name="httpMethod">Method to be used for REST call</param>
        /// <param name="endPoint">REST endpoint</param>
        /// <param name="headers">HTTP request headers</param>
        /// <param name="contentType">Request content type. Default: "application/json"</param>
        /// <param name="content">Request content</param>
        /// <param name="timeoutInMs">Request timeout value in millisecond</param>
        public RestClient(HttpVerb httpMethod, string endPoint, Dictionary<string, string> headers = null, string contentType = null, string content = null, int? timeoutInMs = null)
        {
            _httpMethod = httpMethod;
            _endPoint = new Uri(endPoint, UriKind.Absolute);
            _headers = headers;
            _contentType = contentType ?? "application/json";
            _content = content ?? string.Empty;
            _timeoutInMs = timeoutInMs;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Makes RESTful HTTP request
        /// </summary>
        /// <returns>Response of the request (could be JSON, XML or HTML etc. serialized into string)</returns>
        public async Task<string> MakeRequestAsync()
        {
            HttpClient httpClient = null;

            // Create a HttpClient instance for http request
            try
            {
                httpClient = new HttpClient();
                httpClient.BaseAddress = _endPoint;
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_contentType));
                if (_headers?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> header in _headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create http request handler. " + ex.Message);
            }

            if (httpClient != null)
            {
                return await ProcessHttpRequestAsync(httpClient);
            }

            return null;
        }

        /// <summary>
        /// Update request content
        /// </summary>
        /// <param name="content">Request content</param>
        /// <returns></returns>
        public bool UpdateContent(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                _content = content;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update request timeout
        /// </summary>
        /// <param name="timeoutInMs">Timeout value in millisecond</param>
        /// <returns></returns>
        public bool UpdateTimeout(int? timeoutInMs)
        {
            if (timeoutInMs != null && timeoutInMs < 0)
            {
                return false;
            }
            _timeoutInMs = timeoutInMs;
            return true;
        }

        /// <summary>
        /// Get Endpoint of the REST client
        /// </summary>
        /// <returns></returns>
        public string GetEndpoint()
        {
            return _endPoint.AbsolutePath;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Processes http request
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns>Response of the request</returns>
        private async Task<string> ProcessHttpRequestAsync(HttpClient httpClient)
        {
            string responseValue = null;

            if (httpClient != null)
            {
                StringContent requestContent = null;
                if (_httpMethod != HttpVerb.GET)
                {
                    // Format the request content
                    try
                    {
                        requestContent = new StringContent(_content, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error in request content. " + ex.Message);
                    }
                }
                
                // Make the http request and process the response
                HttpResponseMessage httpResponse = null;
                try
                {
                    // Set a timer to abort the http requset on timeout (if any)
                    CancellationTokenSource abortHttpRequestTaskCancellationTokenSource = null;
                    if (_timeoutInMs != null && _timeoutInMs > -1)
                    {
                        abortHttpRequestTaskCancellationTokenSource = new CancellationTokenSource();
                        AbortHttpRequestOnTimeoutAsync(httpClient, abortHttpRequestTaskCancellationTokenSource.Token);
                    }
                    
                    // Make the http request
                    switch (_httpMethod)
                    {
                        case HttpVerb.GET:
                        case HttpVerb.DELETE:
                            httpResponse = await httpClient.GetAsync(httpClient.BaseAddress);
                            break;
                        case HttpVerb.POST:
                        case HttpVerb.PUT:
                            httpResponse = await httpClient.PostAsync(httpClient.BaseAddress, requestContent);
                            break;
                    }

                    // Cancel the task for aborting http request now as it made the full cycle before the timeout timer got hit
                    abortHttpRequestTaskCancellationTokenSource?.Cancel();

                    // Process the response
                    if (httpResponse != null)
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            // Read the response content into string
                            responseValue = await httpResponse.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            // Throw http status code
                            throw new Exception(((int)httpResponse.StatusCode).ToString());
                        }
                    }
                    else
                    {
                        throw new Exception("An empty HTTP response was received");
                    }
                }
                // Handle different exceptions
                catch (HttpRequestException ex)
                {
                    throw new Exception("Error in RESTful HTTP request. " + ex.Message);
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("HTTP request was timed out, hence aborted.");
                }
                catch (ArgumentNullException ex)
                {
                    throw new Exception("The HTTP client was null. " + ex.Message);
                }

                httpClient?.Dispose();
            }

            return responseValue;
        }

        /// <summary>
        /// Set a timer to abort the http requset on timeout (if any)
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="cancellationToken"></param>
        private async void AbortHttpRequestOnTimeoutAsync(HttpClient httpClient, CancellationToken cancellationToken)
        {
            if (httpClient != null)
            {
                await Task.Run(async () =>
                {
                    await Task.Delay(_timeoutInMs ?? 0); // Set timeout timer
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        httpClient.CancelPendingRequests(); // Abort the http request
                    }
                }, cancellationToken); // Set the timeout Task with cancellation token
            }
        }

        #endregion
    }
}
