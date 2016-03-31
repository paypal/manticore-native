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
import com.eclipsesource.v8.V8Array;
import com.eclipsesource.v8.V8Function;
import com.eclipsesource.v8.V8Object;
import com.eclipsesource.v8.V8RuntimeException;
import com.eclipsesource.v8.V8Value;
import okhttp3.Call;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Callback;
import okhttp3.Response;

class NativeServices
{
  ManticoreEngine engine;
  ScheduledExecutorService deferredFnExecutor = Executors.newScheduledThreadPool(1);
  OkHttpClient httpClient;

  public NativeServices(ManticoreEngine engine)
  {
    try {
      httpClient = new OkHttpClient();
    } catch (AssertionError ae) {
      Log.e("manticore", "Could not start default OkHttpClient", ae);
    }
    this.engine = engine;

    engine.manticoreJsObject.registerJavaMethod(this, "log", "_log", new Class<?>[]{String.class, Object.class});
    engine.manticoreJsObject.registerJavaMethod(this, "fetch", "_fetch", new Class<?>[]{V8Object.class, V8Function.class});
    engine.manticoreJsObject.registerJavaMethod(this, "setTimeout", "_setTimeout", new Class<?>[]{V8Function.class, Integer.class});

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

  public void injectHttpClient(OkHttpClient client) {
    this.httpClient = client;
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

  public void fetch(final V8Object request, V8Function jsCallback)
  {
    final V8Function callback = (V8Function) jsCallback.twin();
    final String url = request.getString("url");
    final Request.Builder httpRequestBuilder = new Request.Builder()
        .url(url);

    String contentType = "application/x-www-form-urlencoded";
    if (request.contains("headers"))
    {
      V8Object headers = request.getObject("headers").executeObjectFunction("raw", engine.EmptyArray);
      for (String kv : headers.getKeys())
      {
        if ("Content-Type".equalsIgnoreCase(kv))
        {
          contentType = headers.getString(kv);
          continue;
        }
        if (headers.getType(kv) == V8Value.V8_ARRAY)
        {
          V8Array allValues = headers.getArray(kv);
          for (int hv = 0; hv < allValues.length(); hv++)
          {
            httpRequestBuilder.addHeader(kv, allValues.get(hv).toString());
          }
        }
        else
        {
          httpRequestBuilder.addHeader(kv, headers.get(kv).toString());
        }
      }
    }

    RequestBody body = null;
    Object jsBody = request.executeFunction("nativeBody", null);
    if (jsBody != null &&
        (jsBody instanceof V8Value) &&
        !((V8Value) jsBody).isUndefined())
    {
      String stringBody = jsBody.toString();
      if (stringBody != null && stringBody.length() > 0)
      {
        if (request.contains("isBase64") && request.getType("isBase64") == V8Value.BOOLEAN && request.getBoolean("isBase64"))
        {
          body = RequestBody.create(MediaType.parse(contentType), Base64.decode(stringBody, Base64.NO_WRAP));
        }
        else
        {
          body = RequestBody.create(MediaType.parse(contentType), stringBody);
        }
      }
    }

    if (request.contains("method"))
    {
      httpRequestBuilder.method(request.getString("method"), body);
    }

    httpClient.newCall(httpRequestBuilder.build()).enqueue(new Callback()
    {
      @Override
      public void onFailure(Call call, final IOException e)
      {
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
      public void onResponse(Call call, final Response response) throws IOException
      {
        engine.getExecutor().runNoWait(new Runnable()
        {
          @Override
          public void run()
          {
            try
            {
              FetchResponse fetchResponse = new FetchResponse(engine, response);
              callback.call(engine.getManticoreJsObject(), engine.createJsArray()
                  .pushUndefined()
                  .push(fetchResponse.getJSInterface()));
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
