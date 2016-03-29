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

import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

import android.util.Log;

/**
 * J2V8 demands that the same thread access the engine all the time. Since we may have re-entrant calls
 * (e.g. to JS, back to Java, back to JS), the dispatcher serializes external calls while allowing reentrancy
 * on properly dipsatched jobs.
 */
public class JsExecutor
{
  private int jobCounter;
  private Thread executorThread;
  private ExecutorService singleMinded = Executors.newSingleThreadExecutor();

  public JsExecutor() {
    Future<Thread> future = singleMinded.submit(new Callable<Thread>()
    {
      @Override
      public Thread call() throws Exception
      {
        return Thread.currentThread();
      }
    });
    try {
      executorThread = future.get();
    } catch (InterruptedException ix) {
      throw new RuntimeException("Failed to create single threaded executor for the Javascript engine.", ix);
    } catch (ExecutionException ex) {
      throw new RuntimeException("Failed to create single threaded executor for the Javascript engine.", ex);
    }
  }

  public void debug(String format, Object... args) {
    // If you suspect deadlocks of some sort (hard to do), this can be handy to track them down
    // Every time I've suspected deadlocks it's been something much simpler.
    //Log.d("ManticoreExecutor", String.format(format, args));
  }

  public void run(final Runnable r)
  {
    final int jobId = jobCounter++;
    if (Thread.currentThread() == executorThread) {
      debug("#%d start void immediate", jobId);
      try
      {
        r.run();
        debug("#%d end void immediate", jobId);
      }
      catch (Exception e)
      {
        debug("#%d end void immediate %s", jobId, e.toString());
        throw e;
      }
      return;
    }
    try
    {
      debug("#%d queue void wait", jobId);
      Exception executionException = singleMinded.submit(new Callable<Exception>()
      {
        @Override
        public Exception call() throws Exception
        {
          try {
            debug("#%d start void wait", jobId);
            r.run();
            debug("#%d done void wait", jobId);
          } catch (Exception x) {
            return x;
          }
          return null;
        }
      }).get();
      debug("#%d completed void wait %s", jobId, executionException != null ? "exception" : "ok");
      if (executionException != null) {
        throw new RuntimeException(executionException);
      }
    }
    catch (InterruptedException e)
    {
      debug("#%d exception queuing %s", jobId, e.toString());
      Log.e("RetailSDK", "Internal Error", e);
      // TODO not sure...
    }
    catch (ExecutionException e)
    {
      debug("#%d exception queuing %s", jobId, e.toString());
      Log.e("RetailSDK", "Internal Error", e);
      // TODO not sure...
    }
  }

  public <T> T run(final Callable<T> r) {
    final int jobId = jobCounter++;
    T retVal;
    if (Thread.currentThread() == executorThread) {
      try
      {
        debug("#%d start T immediate", jobId);
        retVal = r.call();
        debug("#%d end T immediate", jobId);
        return retVal;
      } catch (Exception e) {
        debug("#%d exception T immediate", jobId);
        throw new RuntimeException(e);
      }
    }
    try
    {
      debug("#%d queue T wait", jobId);
      Future<T> task = singleMinded.submit(new Callable<T>()
      {
        @Override
        public T call() throws Exception
        {
          debug("#%d start void wait", jobId);
          T retVal = r.call();
          debug("#%d done void wait", jobId);
          return retVal;
        }
      });
      retVal = task.get();
      debug("#%d completed T immediate", jobId);
      return retVal;
    }
    catch (InterruptedException e)
    {
      debug("#%d exception queuing %s", jobId, e.toString());
      Log.e("RetailSDK", "Internal Error", e);
      // TODO not sure...
      throw new RuntimeException(e);
    }
    catch (ExecutionException e)
    {
      debug("#%d exception queuing %s", jobId, e.toString());
      Log.e("RetailSDK", "Internal Error", e);
      throw new RuntimeException(e);
      // TODO not sure...
    }
  }


  public void runNoWait(final Runnable r)
  {
    final int jobId = jobCounter++;
    if (Thread.currentThread() == executorThread) {
      debug("#%d immediate void noWait", jobId);
      try
      {
        r.run();
      } catch (Exception e) {
        debug("#%d exception void noWait %s", jobId, e.toString());
      }
      return;
    }
    debug("#%d queue void noWait", jobId);
    singleMinded.submit(new Runnable()
    {
      @Override
      public void run()
      {
        debug("#%d start void noWait", jobId);
        try
        {
          r.run();
          debug("#%d done void noWait", jobId);
        }
        catch (Exception e)
        {
          debug("#%d done void noWait %s", jobId, e.toString());
          throw e;
        }
      }
    });
    debug("#%d queued void noWait", jobId);
  }}
