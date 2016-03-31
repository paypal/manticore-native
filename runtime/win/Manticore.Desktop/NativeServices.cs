using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manticore
{
    class NativeServices
    {
        ManticoreEngine engine;

        public void Register(ManticoreEngine engine)
        {
            this.engine = engine;
            engine.ManticoreJsObject._log = new Action<String, String>((level, message) => log(level, message));
            engine.ManticoreJsObject._setTimeout = new Action<dynamic, int>((fn, msec) =>
            {
                setTimeout(fn, msec);
            });
            engine.ManticoreJsObject._fetch = new Action<dynamic, dynamic>((opts, cb) => this.fetch(opts, cb));
        }

        public void log(String level, String message)
        {
            Console.Out.WriteLine("{0} ({1}): {2}", level, Thread.CurrentThread.ManagedThreadId, message);
        }

        public void fetch(dynamic request, dynamic callback)
        {
            var client = new HttpClient();

            HttpMethod requestMethod = HttpMethod.Get;
            if (!(request.method is Undefined))
            {
                var method = ((object)request.method).ToString();
                if (method != null)
                {
                    switch (method.ToLower())
                    {
                        case "delete":
                            requestMethod = HttpMethod.Delete;
                            break;
                        case "head":
                            requestMethod = HttpMethod.Head;
                            break;
                        case "options":
                            requestMethod = HttpMethod.Options;
                            break;
                        case "patch":
                            requestMethod = new HttpMethod("PATCH");
                            break;
                        case "post":
                            requestMethod = HttpMethod.Post;
                            break;
                        case "put":
                            requestMethod = HttpMethod.Put;
                            break;
                    }
                }
            }

            var httpRequest = new HttpRequestMessage(requestMethod, new Uri(request.url));
            var rawBody = request.nativeBody();
            if (!engine.IsNullOrUndefined(rawBody))
            {
                var body = ((object)rawBody).ToString();
                if ((!(request.isBase64 is Undefined) && request.isBase64 == true))
                {  
                    request.Content = new ByteArrayContent(Convert.FromBase64String(body));
                }
                else
                {
                    request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
                }
            }

            dynamic rqHeaders = request.headers.raw();
            DynamicObject dopts = (DynamicObject)rqHeaders;
            foreach (var p in dopts.GetDynamicMemberNames())
            {
                if ("content-type".Equals(p, StringComparison.OrdinalIgnoreCase))
                {
                    if (request.Content != null)
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(rqHeaders[p]);
                    }
                }
                else
                {
                    request.Headers.Add(p, rqHeaders[p]);
                }
            }
            
            sendRequest(client, httpRequest, callback);
        }

        private async void sendRequest(HttpClient client, HttpRequestMessage request, dynamic callback)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request);
            }
            catch (HttpRequestException x)
            {
                dynamic exp = new JsErrorBuilder(x).SetErrorCode((int)ErrorCodes.NetworkOffline).Build();
                dynamic rz = engine.ManticoreJsObject._.construct();
                callback(exp, rz);
                return;
            }

            dynamic responseInfo = engine.ManticoreJsObject._.construct();
            dynamic headers = null;

            foreach (var kv in response.Headers)
            {
                if (headers == null)
                {
                    headers = engine.ManticoreJsObject._.construct();
                }

                headers[kv.Key] = engine.Converter.ToJsArray(kv.Value.ToList(), (v) => v.ToString());
            }

            foreach (var kv in response.Content.Headers)
            {
                if (headers == null)
                {
                    headers = engine.ManticoreJsObject._.construct();
                }

                headers[kv.Key] = engine.Converter.ToJsArray(kv.Value.ToList(), (v) => v.ToString());
            }
            responseInfo.headers = headers;
            responseInfo.status = (int)response.StatusCode;
            // TODO figure out how to sneak this wait into the readers
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            responseInfo.json = new Func<dynamic>(() =>
            {
                try
                {
                    return engine.v8.Script.JSON.parse(Encoding.UTF8.GetString(responseBytes));
                }
                catch (ScriptEngineException se)
                {
                    return new JsErrorBuilder(se).Build();
                }
            });
            responseInfo.text = new Func<dynamic>(() => Encoding.UTF8.GetString(responseBytes));
            responseInfo.body = new Func<dynamic>(() => Convert.ToBase64String(responseBytes));
            callback(null, responseInfo);
        }

        static async void setTimeout(dynamic callback, int timeout)
        {
            if (timeout > 0)
            {
                await Task.Delay(timeout);
            }
            else
            {
                await Task.Yield();
            }

            callback();
        }
    }
}
