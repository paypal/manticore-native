package com.example.dcrelling.androidmanticorehelloworld;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import android.app.Application;
import android.content.Context;
import com.paypal.manticore.JsBackedObject;


public class MyApp extends Application
{

  private boolean _initialized = false;
  @Override
  public void onCreate()
  {
    super.onCreate();
    initialize(getBaseContext());
  }

  public void initialize(Context context)
  {
    if (_initialized)
    {
      throw new RuntimeException("MerchantCore library already initialized");
    }

    _initialized = true;

    StringBuilder fullJs = new StringBuilder();
    try
    {
      InputStream resourceStream = context.getResources().openRawResource(R.raw.manticore_modules);
      InputStreamReader inputReader = new InputStreamReader(resourceStream);
      BufferedReader jsReader = new BufferedReader(inputReader);

      String line;
      while ((line = jsReader.readLine()) != null)
      {
        fullJs.append(line).append('\n');
      }
      jsReader.close();
    }
    catch (IOException ex)
    {
      throw new RuntimeException(ex.getMessage());
    }

    final String finalJs = fullJs.toString();

    JsBackedObject.createManticoreEngine(context, finalJs, "");
  }
}
