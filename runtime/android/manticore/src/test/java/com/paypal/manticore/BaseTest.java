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

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import android.test.mock.MockContext;
import android.test.mock.MockResources;
import android.util.Log;
import okhttp3.OkHttpClient;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.mock;

/**
 * Because of the fact that J2V8 can't be active in multiple class loaders, and gradle/android junit doesn't
 * support passing a forkEvery parameter (that I can find), we have to use a single class for all the tests.
 * To make it easier to undo when this problem is fixed, I've kept the individual test classes and just
 * "indexed" them here.
 */
@PrepareForTest({Log.class})
@RunWith(PowerMockRunner.class)
public class BaseTest
{
  static MockContext _mockContext;
  static MockResources _mockResources;
  static String _testJs;


  @Before
  public void mockAndroids() throws Exception
  {
    // Mock the Android logging framework because it doesn't exist during JUnit and we like logs.
    PowerMockito.mockStatic(Log.class);
    PowerMockito.when(Log.d(anyString(), anyString())).thenAnswer(new Answer()
    {
      public Object answer(InvocationOnMock invocation)
      {
        Object[] args = invocation.getArguments();
        System.out.println(args[0].toString() + " " + args[1].toString());
        return 0;
      }
    });
    PowerMockito.when(Log.e(anyString(), anyString())).thenAnswer(new Answer()
    {
      public Object answer(InvocationOnMock invocation)
      {
        Object[] args = invocation.getArguments();
        System.out.println(args[0].toString() + " " + args[1].toString());
        return 0;
      }
    });
  }


  @BeforeClass
  public static void mockJS() throws Exception
  {
    // Fake the javascript loading mechanism to go against the local disk since the
    // resource loader isn't available.
    if (_mockContext == null)
    {
      _mockContext = mock(MockContext.class);
      _mockResources = mock(MockResources.class);

      PowerMockito.when(_mockContext.getResources()).thenReturn(_mockResources);

      String basePath = ManticoreEngine.class.getResource("ManticoreEngine.class").getPath();
      File basePathDir = new File(basePath);
      while (!basePathDir.getName().equalsIgnoreCase("intermediates"))
      {
        basePathDir = basePathDir.getParentFile();
      }
      final File jsPath = new File(basePathDir, "bundles/debug/res/raw/polyfill_pack.js");
      PowerMockito.when(_mockResources.openRawResource(R.raw.polyfill_pack))
          .thenAnswer(new Answer()
          {
            public Object answer(InvocationOnMock invocation) throws FileNotFoundException
            {
              return new FileInputStream(jsPath);
            }
          });

      _testJs = fromStream(BaseTest.class.getClassLoader().getResourceAsStream("index.pack.js"));
    }
  }


  static String fromStream(InputStream in) throws IOException
  {
    InputStreamReader input = new InputStreamReader(in /*, "UTF-8"*/);
    final int CHARS_PER_PAGE = 5000; //counting spaces
    final char[] buffer = new char[CHARS_PER_PAGE];
    StringBuilder output = new StringBuilder(CHARS_PER_PAGE);
    try
    {
      for (int read = input.read(buffer, 0, buffer.length);
           read != -1;
           read = input.read(buffer, 0, buffer.length))
      {
        output.append(buffer, 0, read);
      }
    }
    catch (IOException ignore)
    {
    }
    return output.toString();
  }


  @Test
  public void makeEngineTest()
  {
    new EngineTests().makeEngineTest(_mockContext);
  }


  @Test
  public void loadJsTest()
  {
    new EngineTests().loadJsTest(_mockContext, _testJs);
  }


  @Test
  public void propertyTest()
  {
    new EngineTests().propertyTest(_mockContext, _testJs);
  }


  @Test
  public void functionCallTest()
  {
    new EngineTests().functionCallTest(_mockContext, _testJs);
  }


  @Test
  public void callbackTest() throws Throwable
  {
    new EngineTests().callbackTest(_mockContext, _testJs);
  }


  @Test
  public void eventTest() throws Throwable
  {
    new EngineTests().eventTest(_mockContext, _testJs);
  }


  @Test
  public void collectionTest()
  {
    new EngineTests().collectionTest(_mockContext, _testJs);
  }


  @Test
  public void staticMethodTest()
  {
    new EngineTests().staticMethodTest(_mockContext, _testJs);
  }


  @Test
  public void objectTest()
  {
    new EngineTests().objectTest(_mockContext, _testJs);
  }


  @Test
  public void dateTest()
  {
    new EngineTests().dateTest(_mockContext, _testJs);
  }


  @Test
  public void inheritanceTest()
  {
    new EngineTests().inheritanceTest(_mockContext, _testJs);
  }


  @Test
  public void fetchTest() throws Throwable
  {
    new EngineTests().fetchTest(_mockContext, _testJs);
  }


  @Test
  public void fetchPTest() throws Throwable
  {
    new EngineTests().fetchPTest(_mockContext, _testJs);
  }
}
