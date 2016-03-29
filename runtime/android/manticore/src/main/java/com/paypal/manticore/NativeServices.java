/*-------------------------------------------------------------------------------------------------------------------*\
 |  Copyright (C) 2015 PayPal                                                                                          |
 |                                                                                                                     |
 |  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance     |
 |  with the License.                                                                                                  |
 |                                                                                                                     |
 |  You may obtain a copy of the License at                                                                            |
 |                                                                                                                     |
 |       http://www.apache.org/licenses/LICENSE-2.0                                                                    |
 |                                                                                                                     |
 |  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed   |
 |  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for  |
 |  the specific language governing permissions and limitations under the License.                                     |
 \*-------------------------------------------------------------------------------------------------------------------*/
package com.paypal.manticore;

import java.io.IOException;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.net.ProtocolException;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

import android.util.Base64;
import android.util.Log;
import com.eclipsesource.v8.V8;
import com.eclipsesource.v8.V8Function;
import com.eclipsesource.v8.V8Object;
import com.eclipsesource.v8.V8RuntimeException;
import com.eclipsesource.v8.V8Value;
import com.squareup.okhttp.Callback;
import com.squareup.okhttp.MediaType;
import com.squareup.okhttp.OkHttpClient;
import com.squareup.okhttp.Request;
import com.squareup.okhttp.RequestBody;
import com.squareup.okhttp.Response;

class NativeServices
{
  ManticoreEngine engine;
  ScheduledExecutorService deferredFnExecutor = Executors.newScheduledThreadPool(1);
  OkHttpClient httpClient = new OkHttpClient();

  public NativeServices(ManticoreEngine engine)
  {
    this.engine = engine;

    engine.manticoreJsObject.registerJavaMethod(this, "log", "log", new Class<?>[]{String.class, Object.class});
    engine.manticoreJsObject.registerJavaMethod(this, "http", "http", new Class<?>[]{V8Object.class, V8Function.class});
    engine.manticoreJsObject.registerJavaMethod(this, "setTimeout", "setTimeout", new Class<?>[]{V8Function.class, Integer.class});

    // If you need to develop against invalid certificates...
    /*
    try
    {
      // Create a trust manager that does not validate certificate chains
      final javax.net.ssl.TrustManager[] trustAllCerts = new javax.net.ssl.TrustManager[]{
          new javax.net.ssl.X509TrustManager()
          {
            @Override
            public void checkClientTrusted(java.security.cert.X509Certificate[] chain, String authType)
            {
            }


            @Override
            public void checkServerTrusted(java.security.cert.X509Certificate[] chain, String authType)
            {
            }


            @Override
            public java.security.cert.X509Certificate[] getAcceptedIssuers()
            {
              return null;
            }
          }
      };
      // Install the all-trusting trust manager
      final javax.net.ssl.SSLContext sslContext = javax.net.ssl.SSLContext.getInstance("SSL");
      sslContext.init(null, trustAllCerts, new java.security.SecureRandom());
      // Create an ssl socket factory with our all-trusting manager
      final javax.net.ssl.SSLSocketFactory sslSocketFactory = sslContext.getSocketFactory();

      httpClient.setSslSocketFactory(sslSocketFactory);
      httpClient.setHostnameVerifier(new javax.net.ssl.HostnameVerifier()
      {
        @Override
        public boolean verify(String hostname, javax.net.ssl.SSLSession session)
        {
          return true;
        }
      });
    }
    catch (Exception e)
    {

    }
    */
  }

  public void log(String level,  Object messageValue)
  {
    String message;
    if (messageValue == V8.getUndefined()) {
      message = "undefined";
    } else {
      message = messageValue.toString();
    }
    if ("ERROR".equalsIgnoreCase(level))
    {
      Log.e("manticore", message);
    }
    else if ("WARN".equalsIgnoreCase(level)) {
      Log.w("manticore", message);
    }
    else
    {
      Log.i("manticore", message);
    }
  }

  public void http(final V8Object options, V8Function jsCallback)
  {
    final V8Function callback = (V8Function) jsCallback.twin();
    final String url = options.getString("url");
    final Request.Builder httpRequestBuilder = new Request.Builder()
        .url(url);

    String contentType = "application/x-www-form-urlencoded";
    if (options.contains("headers"))
    {
      V8Object headers = options.getObject("headers");
      for (String kv : headers.getKeys())
      {
        String key = kv.toString();
        String value = headers.getString(kv);
        if ("Content-Type".equalsIgnoreCase(key))
        {
          contentType = value;
        }
        httpRequestBuilder.addHeader(key, value);
      }
    }

    String method = "GET";
    RequestBody body = null;
    if (options.contains("body"))
    {
      body = RequestBody.create(MediaType.parse(contentType), options.getString("body"));
    }
    if (options.contains("method"))
    {
      httpRequestBuilder.method(options.getString("method"), body);
    }

    String tmpformat = null;
    if (options.contains("format"))
    {
      tmpformat = options.getString("format");
    }

    final String format = tmpformat;

    final boolean isDebug = options.contains("debug");
    if (isDebug) {
      // Set a breakpoint here and modify the JS call to intercept it.
      Log.d("NativeServices", "debuggable request");
    }

    httpClient.newCall(httpRequestBuilder.build()).enqueue(new Callback()
    {
      @Override
      public void onFailure(final Request request, final IOException e)
      {
        if (isDebug) {
          // Set a breakpoint here and modify the JS call to intercept it.
          Log.d("NativeServices", "debuggable request");
        }

        engine.getExecutor().runNoWait(new Runnable()
        {
          @Override
          public void run()
          {
            V8Object vError = engine.asJsError(e);
            callback.call(engine.getManticoreJsObject(), engine.createJsArray().push(vError));
          }
        });
      }


      @Override
      public void onResponse(final Response response) throws IOException
      {
        if (isDebug) {
          // Set a breakpoint here and modify the JS call to intercept it.
          Log.d("NativeServices", "debuggable request");
        }

        engine.getExecutor().runNoWait(new Runnable()
        {
          @Override
          public void run()
          {
            V8Object returnValue = engine.createJsObject();
            returnValue.add("statusCode", response.code());
            V8Object headers = engine.createJsObject();
            for (String hdr : response.headers().names())
            {
              headers.add(hdr, response.header(hdr).toString());
            }
            returnValue.add("headers", headers);
            try
            {
              if (response.body().contentLength() != 0)
              {
                if ("json".equalsIgnoreCase(format))
                {
                  try
                  {
                    String body = response.body().string();
                    if (body == null || body.length() == 0)
                    {
                      returnValue.add("body", (V8Object) null);
                    }
                    else
                    {
                      try
                      {
                        V8Object jsonBody = engine.v8.getObject("JSON").executeObjectFunction("parse", engine.createJsArray().push(body));
                        returnValue.add("body", jsonBody);
                      }
                      catch (Exception x)
                      {
                        // Tough call here, but for now I think returning null is better,
                        // though we may want to also store the body
                        returnValue.add("body", (V8Object) null);
                      }
                    }
                  }
                  catch (V8RuntimeException pe)
                  {
                    callback.call(engine.getManticoreJsObject(), engine.createJsArray().push(engine.asJsError(pe)));
                    return;
                  }
                  catch (ProtocolException ex)
                  {
                    returnValue.add("body", (V8Object) null);
                  }
                }
                else if (!"binary".equalsIgnoreCase(format))
                {
                  if (response.body().contentLength() != 0)
                  {
                    returnValue.add("body", response.body().string());
                  }
                }
                else
                {
                  if (response.body().contentLength() != 0)
                  {
                    byte[] responseBody = response.body().bytes();
                    returnValue.add("body", Base64.encodeToString(responseBody, Base64.NO_WRAP));
                  }
                }
              }
              callback.call(engine.getManticoreJsObject(), engine.createJsArray().pushUndefined().push(returnValue));
            }
            catch (Exception x)
            {
              StringWriter sw = new StringWriter();
              PrintWriter pw = new PrintWriter(sw);
              x.printStackTrace(pw);
              // Don't call the callback again.
              log("ERROR", x.toString() + "\n" + sw.toString());
            }
          }
        });
      }
    });
  }

  public void setTimeout(V8Function func, Integer timeoutMsec) {
    final V8Function myFunction = (V8Function) func.twin();
    deferredFnExecutor.schedule(new Runnable()
    {
      @Override
      public void run()
      {
        engine.getExecutor().runNoWait(new Runnable()
        {
          @Override
          public void run()
          {
            // TODO swallow exceptions?
            myFunction.call(myFunction, null);
            myFunction.release();
          }
        });
      }
    }, timeoutMsec, TimeUnit.MILLISECONDS);
  }
}
