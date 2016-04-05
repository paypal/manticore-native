package com.paypal.manticore;

/**
 * Exposing these elements of the loading cycle allows you to inject your own code
 * at appropriate points. Typically this is used to expose new native functionality
 * to the javascript. Note that if you load script, your own will/didLoadScript event
 * will get called, so make sure to not infinitely do so.
 */
public interface IManticoreObserver
{
  public void willLoadPolyfill(ManticoreEngine engine);
  public void didLoadPolyfill(ManticoreEngine engine);
  public void willLoadScript(ManticoreEngine engine, String script, String name);
  public void didLoadScript(ManticoreEngine engine, String script, String name);
}
