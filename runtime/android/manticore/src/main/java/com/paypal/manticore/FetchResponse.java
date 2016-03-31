package com.paypal.manticore;

import java.io.IOException;
import java.net.ProtocolException;

import android.util.Base64;
import com.eclipsesource.v8.V8Object;
import com.eclipsesource.v8.V8RuntimeException;
import com.eclipsesource.v8.V8Value;
import okhttp3.Response;

/**
 * Created by mmetral on 3/31/16.
 */
public class FetchResponse
{
  ManticoreEngine _engine;
  Response _response;
  V8Object _jsThis;

  FetchResponse(ManticoreEngine engine, Response response) {
    this._engine = engine;
    this._response = response;
    this._jsThis = _engine.createJsObject();
    this._jsThis.add("status", response.code());
    V8Object headers = engine.createJsObject();
    for (String hdr : response.headers().names())
    {
      headers.add(hdr, response.header(hdr).toString());
    }
    this._jsThis.add("headers", headers);
    this._jsThis.registerJavaMethod(this, "json", "json", null);
    this._jsThis.registerJavaMethod(this, "text", "text", null);
    this._jsThis.registerJavaMethod(this, "body", "body", null);
  }

  public V8Object getJSInterface() {
    return _jsThis;
  }

  public String body() {
    try
    {
      if (_response.body().contentLength() == 0) {
        return null;
      }

      return Base64.encodeToString(_response.body().bytes(), Base64.NO_WRAP);
    }
    catch (IOException io) {
      return null;
    }
    catch (V8RuntimeException pe)
    {
      return null;
    }
  }

  public String text() {
    try
    {
      if (_response.body().contentLength() == 0) {
        return null;
      }

      return _response.body().string();
    }
    catch (IOException io) {
      return null;
    }
    catch (V8RuntimeException pe)
    {
      return null;
    }
  }

  public V8Object json() {
    try
    {
      String body = this.text();
      if (body == null || body.length() == 0)
      {
        return null;
      }
      else
      {
        try
        {
          V8Object jsonBody = _engine.v8.getObject("JSON").executeObjectFunction("parse", _engine.createJsArray().push(body));
          return jsonBody;
        }
        catch (Exception x)
        {
          // Tough call here, but for now I think returning null is better,
          // though we may want to also store the body. Ok fine, I was just being lazy.
          return null;
        }
      }
    }
    catch (V8RuntimeException pe)
    {
      return null;
    }
  }
}
