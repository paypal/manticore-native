using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Parser;
using System;
using System.Diagnostics;
using System.IO;

#if DOTNET_4
using System.Net;
#else
using Windows.Web.Http.Filters;
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#endif
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Jint;
using Jint.Native.Error;

namespace Manticore
{
    class NativeServices
    {
        int ThreadId
        {
            get
            {
#if DOTNET_4
                return System.Threading.Thread.CurrentThread.ManagedThreadId;
#else
                return Environment.CurrentManagedThreadId;
#endif
            }
        }

        public void Register(ManticoreEngine engine)
        {
            engine.ManticoreJsObject.FastAddProperty("_log",
                engine.AsJsFunction((thisObject, args) =>
                {
                    JsValue extra = JsValue.Null;
                    if (args.Length > 2)
                    {
                        extra = args[2];
                    }
                    Log(args[0].AsString(), args[1].AsString(), extra);
                    return JsValue.Undefined;
                }), true, false, false);
            engine.ManticoreJsObject.FastAddProperty("_setTimeout",
                engine.AsJsFunction((thisObject, args) =>
                {
                    SetTimeout(engine, args[0].As<FunctionInstance>(), (int) args[1].AsNumber());
                    return JsValue.Undefined;
                }), true, false, false);
            engine.ManticoreJsObject.FastAddProperty("_fetch",
                engine.AsJsFunction((thisObject, args) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        Fetch(engine, args[0], args[1]);
                    });
                    return JsValue.Undefined;
                }), true, false, false);
        }

        void Log(String level, String message, JsValue extraData)
        {
            Debug.WriteLine("{0} ({1}): {2}", level, ThreadId, message);
        }

#if DOTNET_4
        private void SetTimeout(ManticoreEngine engine, FunctionInstance callback, int timeout)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            // TODO not sure there may be a better way to emulate NextTick
            timer.Interval = Math.Max(1, timeout);
            timer.AutoReset = false;
            timer.Elapsed += (obj, args) =>
            {
                engine.Js(() =>
                {
                    try
                    {
                        callback.Call(JsValue.Null, new JsValue[] { });
                    }
                    catch (Exception x)
                    {
                        // TODO perhaps throw this error INTO JS?
                        Log("error", x.ToString(), JsValue.Null);
                    }
                });
            };
            timer.Start();
        }

        public void Fetch(ManticoreEngine engine, JsValue optionsValue, JsValue callback)
        {
            var options = optionsValue.As<ObjectInstance>();
            JsValue rawBody = options.Get("body").As<FunctionInstance>().Call(options, ManticoreEngine.EmptyArgs);
            var hasBody = rawBody.IsString() && rawBody.AsString().Length > 0;
            var request = (HttpWebRequest)WebRequest.Create(options.Get("url").AsString());

            request.Method = "GET";
            if (options.HasProperty("method"))
            {
                request.Method = options.Get("method").AsString();
            }

            if (options.HasProperty("headers"))
            {
                var headers = options.Get("headers").AsObject();
                var raw = headers.Get("raw").As<FunctionInstance>().Call(headers, ManticoreEngine.EmptyArgs);
                if (raw.IsObject())
                {
                    foreach (var p in raw.AsObject().GetOwnProperties())
                    {
                        if (p.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            if (hasBody)
                            {
                                request.ContentType = p.Value.Value.Value.AsString();
                            }
                        }
                        else if (p.Key.Equals("If-Modified-Since", StringComparison.OrdinalIgnoreCase))
                        {
                            request.IfModifiedSince = DateTime.Parse(p.Value.Value.Value.AsString());
                        }
                        else
                        {
                            request.Headers.Add(p.Key, p.Value.Value.Value.AsString());
                        }
                    }
                }
            }

            if (hasBody)
            {
                var body = rawBody.AsString();
                byte[] bodyBytes;
                if (options.HasProperty("isBase64") && options.Get("isBase64").AsBoolean())
                {
                    bodyBytes = Convert.FromBase64String(body);
                }
                else
                {
                    bodyBytes = Encoding.UTF8.GetBytes(body);
                }
                request.ContentLength = bodyBytes.Length;

                var errorInstance = JsValue.Null;
                try
                {
                    var rqStream = request.GetRequestStream();
                    rqStream.Write(bodyBytes, 0, bodyBytes.Length);
                    rqStream.Close();
                }
                catch (Exception x)
                {
                    // TODO fire this log into JS? manticore.onError()?
                    Log("error", x.ToString(), JsValue.Null);
                    var errorBuilder = new JsErrorBuilder(engine, x);
                    if (x is WebException)
                    {
                        errorBuilder.SetErrorCode((int) ErrorCodes.NetworkOffline);
                    }
                    errorInstance = errorBuilder.Build();
                }

                if (errorInstance != JsValue.Null)
                {
                    engine.Js(() =>
                    {
                        callback.As<FunctionInstance>().Call(engine.ManticoreJsObject, new[] { errorInstance, JsValue.Null });
                    });
                    return;
                }
            }

            sendRequest(engine, request, callback);
        }

        private void sendRequest(ManticoreEngine engine, HttpWebRequest request, JsValue callback)
        {
            request.BeginGetResponse((asyncResult) =>
            {
                HttpWebResponse response = null;
                var errorInstance = JsValue.Null;
                try
                {
                    response = (HttpWebResponse)request.EndGetResponse(asyncResult);
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError) //Response was received from server but indicated protocol level error
                    {
                        response = (HttpWebResponse)e.Response;
                    }
                    else
                    {
                        errorInstance = new JsErrorBuilder(engine, e).SetErrorCode((int)ErrorCodes.NetworkOffline).Build();
                    }
                }

                if (response == null)
                {
                    engine.Js(() =>
                    {
                        callback.As<FunctionInstance>().Call(engine.ManticoreJsObject, new[] { errorInstance, JsValue.Null });
                    });
                    return;
                }

                var responseInfo = new ObjectInstance(engine.jsEngine);
                if (response.Headers.Count > 0)
                {
                    var headerCollection = new ObjectInstance(engine.jsEngine);
                    foreach (var kv in response.Headers.AllKeys)
                    {
                        headerCollection.FastAddProperty(kv, new JsValue(response.Headers[kv]), false, true, false);
                    }
                    responseInfo.FastAddProperty("headers", headerCollection, false, true, false);
                }

                responseInfo.FastAddProperty("status", new JsValue((int)response.StatusCode), false, true, false);
                // TODO find a way to sneak this wait into the gap between returning and asking for the results
                // json/body/text signatures probably need to change to take a callback.
                var memStream = new MemoryStream();
                response.GetResponseStream().CopyTo(memStream);
                var binaryResult = memStream.ToArray();
                responseInfo.FastAddProperty("json", engine.AsJsFunction((thisObject, args) => {
                        return engine.jsEngine.Json.Parse(JsValue.Null, new JsValue[] { Encoding.UTF8.GetString(binaryResult) });
                }), false, false, false);
                responseInfo.FastAddProperty("body", engine.AsJsFunction((thisObject, args) => {
                    return engine.jsEngine.Json.Parse(JsValue.Null, new JsValue[] { Convert.ToBase64String(binaryResult) });
                }), false, false, false);
                responseInfo.FastAddProperty("text", engine.AsJsFunction((thisObject, args) => {
                    return new JsValue(Encoding.UTF8.GetString(binaryResult));
                }), false, false, false);
                engine.Js(() =>
                {
                    callback.As<FunctionInstance>().Call(engine.ManticoreJsObject, new[] { errorInstance, responseInfo });
                });
            }, null);
        }

#else
        private async void SetTimeout(ManticoreEngine engine, FunctionInstance callback, int timeout)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(timeout));
            engine.Js(() =>
            {
                try
                {
                    callback.Call(JsValue.Null, new JsValue[] { });
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x);
                }
            });
        }

        public void Fetch(ManticoreEngine engine, JsValue optionsValue, JsValue callback)
        {
            var options = optionsValue.As<ObjectInstance>();
            var httpBaseFilter = new HttpBaseProtocolFilter
            {
                AllowUI = false
            };
            var client = new HttpClient(httpBaseFilter);

            HttpMethod requestMethod = HttpMethod.Get;
            if (options.HasProperty("method"))
            {
                var method = options.Get("method").AsString();
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
                            requestMethod = HttpMethod.Patch;
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

            var urlString = options.Get("url").AsString();
            var rawBody = options.Get("nativeBody").As<ScriptFunctionInstance>().Call(options, ManticoreEngine.EmptyArgs);
            var hasBody = rawBody.IsString() && rawBody.AsString().Length > 0;
            var request = new HttpRequestMessage(requestMethod, new Uri(urlString));

            if (options.HasProperty("headers"))
            {
                var headers = options.Get("headers").AsObject();
                var raw = headers.Get("raw").As<FunctionInstance>().Call(headers, ManticoreEngine.EmptyArgs);
                if (raw.IsObject())
                {
                    foreach (var p in raw.AsObject().GetOwnProperties())
                    {
                        if (p.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            if (hasBody)
                            {
                                request.Content.Headers.Add(p.Key, p.Value.Value.Value.AsString());
                            }
                        }
                        else
                        {
                            request.Headers.Add(p.Key, p.Value.Value.Value.AsString());
                        }
                    }
                }
            }

            if (hasBody)
            {
                if (options.HasProperty("isBase64") && options.Get("isBase64").AsBoolean())
                {
                    request.Content = new HttpBufferContent(Convert.FromBase64String(rawBody.AsString()).AsBuffer());
                }
                else
                {
                    request.Content = new HttpBufferContent(Encoding.UTF8.GetBytes(rawBody.AsString()).AsBuffer());
                }
            }

            int timeout = 60000;
            if (options.HasProperty("timeout") && options.Get("timeout").IsNumber())
            {
                timeout = (int)options.Get("timeout").AsNumber();
            }
            sendRequest(engine, timeout, client, request, callback);
        }

        private async void sendRequest(ManticoreEngine engine, int timeout, HttpClient client, HttpRequestMessage request, JsValue callback)
        {
            var responseInfo = engine.CreateJsObject();
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource(timeout);
            try
            {
                response = await client.SendRequestAsync(request, HttpCompletionOption.ResponseContentRead).AsTask(cts.Token);
            }
            catch (Exception x)
            {
                var ei = new JsErrorBuilder(engine, x).Build();
                responseInfo.FastAddProperty("error", ei, false, true, false);
            }

            if (response != null && response.Headers.Count > 0)
            {
                var headerCollection = engine.CreateJsObject();
                foreach (var kv in response.Headers)
                {
                    try
                    {
                        headerCollection.FastAddProperty(kv.Key, new JsValue(kv.Value), false, true, false);
                    }
                    catch (ArgumentException)
                    {
                        // Swallow duplicate headers for now.
                    }
                }
                responseInfo.FastAddProperty("headers", headerCollection, false, true, false);
            }

            byte[] binaryResult = null;
            if (response != null)
            {
                responseInfo.FastAddProperty("status", new JsValue((int)response.StatusCode), false, true, false);
                // TODO find a way to sneak this wait into the gap between returning and asking for the results
                // json/body/text signatures probably need to change to take a callback.
                binaryResult = (await response.Content.ReadAsBufferAsync()).ToArray();
            }
            engine.Js(() =>
            {
                responseInfo.FastAddProperty("json", engine.AsJsFunction((thisObject, args) => {
                    return engine.jsEngine.Json.Parse(JsValue.Null,
                        new JsValue[] { Encoding.UTF8.GetString(binaryResult, 0, binaryResult.Length) });
                }), false, false, false);
                responseInfo.FastAddProperty("body", engine.AsJsFunction((thisObject, args) => {
                    return engine.jsEngine.Json.Parse(JsValue.Null, new JsValue[] { Convert.ToBase64String(binaryResult) });
                }), false, false, false);
                responseInfo.FastAddProperty("text", engine.AsJsFunction((thisObject, args) => {
                    return new JsValue(Encoding.UTF8.GetString(binaryResult, 0, binaryResult.Length));
                }), false, false, false);
                callback.As<FunctionInstance>().Call(engine.ManticoreJsObject, new JsValue[] { JsValue.Null, responseInfo });
            });
        }
#endif
    }
}
