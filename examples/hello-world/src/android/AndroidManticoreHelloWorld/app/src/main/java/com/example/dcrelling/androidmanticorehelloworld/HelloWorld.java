package com.example.dcrelling.androidmanticorehelloworld;

import java.lang.reflect.Method;

import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;
import com.paypal.manticore.Hello;
import com.paypal.manticore.JsBackedObject;


public class HelloWorld extends AppCompatActivity
{

  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    super.onCreate(savedInstanceState);
    setContentView(R.layout.activity_hello_world);
    Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
    setSupportActionBar(toolbar);

    Button btn1 = (Button) findViewById(R.id.button1);
    Button btn2 = (Button) findViewById(R.id.button2);

    btn1.setOnClickListener(new View.OnClickListener()
    {
      @Override
      public void onClick(View v)
      {
        Hello hello = new Hello("hey", 3);
        Log.d("HelloWorld", "function tests:");
        Log.d("HelloWorld", "created obj = " + hello.toString());
        Toast.makeText(getApplicationContext(), hello.toString(), Toast.LENGTH_LONG).show();
      }
    });

    btn2.setOnClickListener(new View.OnClickListener()
    {
      @Override
      public void onClick(View v)
      {
        Hello hi = new Hello("hi", 5);

        Method method = null;
        MyObj obj = new MyObj();
        try
        {
          method = obj.getClass().getMethod("showCallbackWorked", new Class[]{});
        }
        catch (NoSuchMethodException e)
        {
          e.printStackTrace();
        }
        hi.testCallback(); //FIXME
      }
    });
  }

  public class MyObj extends JsBackedObject
  {
    public void showCallbackWorked()
    {
      HelloWorld.this.runOnUiThread(new Runnable()
      {
        @Override
        public void run()
        {
          Toast.makeText(HelloWorld.this.getApplicationContext(), "It works!", Toast.LENGTH_LONG).show();
        }
      });
//      Log.d("Callback", "It worked!!");
    }
  }




  @Override
  public boolean onCreateOptionsMenu(Menu menu)
  {
    // Inflate the menu; this adds items to the action bar if it is present.
    getMenuInflater().inflate(R.menu.menu_hello_world, menu);
    return true;
  }


  @Override
  public boolean onOptionsItemSelected(MenuItem item)
  {
    // Handle action bar item clicks here. The action bar will
    // automatically handle clicks on the Home/Up button, so long
    // as you specify a parent activity in AndroidManifest.xml.
    int id = item.getItemId();

    //noinspection SimplifiableIfStatement
    if (id == R.id.action_settings)
    {
      return true;
    }

    return super.onOptionsItemSelected(item);
  }
}
