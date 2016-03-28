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
            engine.ManticoreJsObject.log = new Action<String, String, String>((level, component, message) => log(level, component, message));
            engine.ManticoreJsObject.export = new Action<dynamic>((obj) => export(engine, obj));
            engine.ManticoreJsObject.setTimeout = new Action<dynamic, int>((fn, msec) =>
            {
                setTimeout(fn, msec);
            });
            engine.ManticoreJsObject.http = new Action<dynamic, dynamic>((opts, cb) => this.http(opts, cb));
        }

        public void log(String level, String component, String message)
        {
            Console.Out.WriteLine("{0} ({1}): {2} {3}", level, Thread.CurrentThread.ManagedThreadId, component, message);
        }

        public void http(dynamic options, dynamic callback)
        {
            var client = new HttpClient();

            HttpMethod requestMethod = HttpMethod.Get;
            if (!(options.method is Undefined))
            {
                var method = ((object)options.method).ToString();
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

            var request = new HttpRequestMessage(requestMethod, new Uri(options.url));
            if (!(options.body is Undefined))
            {
                var body = ((object)options.body).ToString();
                if ((!(options.base64Body is Undefined)) && options.base64Body == true)
                {
                    request.Content = new ByteArrayContent(Convert.FromBase64String(body));
                }
                else
                {
                    request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
                }
            }

            if (!(options.headers is Undefined))
            {
                DynamicObject dopts = (DynamicObject)options.headers;
                foreach (var p in dopts.GetDynamicMemberNames())
                {
                    if ("content-type".Equals(p, StringComparison.OrdinalIgnoreCase))
                    {
                        if (request.Content != null)
                        {
                            request.Content.Headers.ContentType = new MediaTypeHeaderValue(options.headers[p]);
                        }
                    }
                    else
                    {
                        request.Headers.Add(p, options.headers[p]);
                    }
                }
            }

            String format = null;
            if (!(options.format is Undefined))
            {
                format = ((object)options.format).ToString();
            }
            sendRequest(format, client, request, callback);
        }

        private async void sendRequest(String format, HttpClient client, HttpRequestMessage request, dynamic callback)
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

                headers[kv.Key] = kv.Value.FirstOrDefault();
            }

            foreach (var kv in response.Content.Headers)
            {
                if (headers == null)
                {
                    headers = engine.ManticoreJsObject._.construct();
                }

                headers[kv.Key] = kv.Value.FirstOrDefault();
            }
            responseInfo.headers = headers;
            responseInfo.statusCode = (int)response.StatusCode;
            try
            {
                DefaultConverter<JsBackedObject>.ParseResponseBody(engine, responseInfo, format, await response.Content.ReadAsByteArrayAsync());
                callback(null, responseInfo);
            }
            catch (ScriptEngineException se)
            {
                dynamic exp = new JsErrorBuilder(se).Build();
                callback(exp, responseInfo);
            }
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

        static void export(ManticoreEngine engine, dynamic obj)
        {
            if (obj == null || obj is Undefined)
            {
                return;
            }
            var exports = obj as DynamicObject;
            if (exports != null)
            {
                foreach (var name in exports.GetDynamicMemberNames())
                {
                    engine.exportedItems[name] = obj[name];
                }
            }
        }
    }
}
