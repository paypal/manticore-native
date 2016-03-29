import { EventEmitter } from 'events';
import { SDKTestDefault, SDKTestDefaultSubclass } from './SDKTestDefault';

/**
 * This single file basically fakes the real SDK and exercises the various ways the native engines
 * interact with Javascript.
 * @class
 * @property {int} itsOne Starts out as 1.
 * @property {int} cantTouchThis This is @readonly
 * @property {string} stringProperty This is a plain old string property
 * @property {string} accessorString This property has accessor functions in JS
 * @property {SDKTestDefault} complexType Yep.
 * @property {SDKTest.Statuses} myStatus fake enum for status.
 */
export class SDKTest extends EventEmitter {
  /**
   * Make the class with a particular stringProperty setting
   * @param {string} stringProperty initial setting
   */
  constructor(stringProperty) {
    super();
    this.itsOne = 1;
    this.complexType = new SDKTestDefault();
    this.stringProperty = stringProperty;
  }

  get accessorString() {
    return this._accessorString;
  }

  set accessorString(value) {
    this._accessorString = value;
  }

  /**
   * Echo the argument via the callback
   * @param {string} arg
   * @param {SDKTest~echo} callback
   */
  echo(arg, callback) {
    callback(null, arg);
  }

  /**
   * Echo the argument via return value from a callback
   * @param {string} arg
   * @param {SDKTest~echoReturn} callback
   */
  echoReturn(arg, callback) {
    return callback(arg);
  }

  /**
   * Echo the argument via the callback after setTimeout(10)
   * @param {string} arg
   * @param {SDKTest~echo} callback
   */
  echoWithSetTimeout(arg, callback) {
    setTimeout(() => {
      callback(null, arg);
    }, 10);
  }

  /**
   * Fire an event
   */
  triggerFakeAfterTimeout() {
    setTimeout(() => {
      this.emit('fakeEvent', new SDKTestDefault());
    }, 10);
  }

  /**
   * Return a complex object.
   * @returns {SDKTestDefault} Stuff
   */
  returnAnObject() {
    return new SDKTestDefault();
  }

  /**
   * Return a derivative of SDKTestDefault
   * @returns {SDKTestDefault} Stuff
   */
  returnADerivedObject() {
    return SDKTestDefaultSubclass.getDerived();
  }

  /**
   * Return one SDKTestDefault and one derived
   * @returns {[SDKTestDefault]} array of size 2
   */
  returnBaseAndDerived() {
    return [new SDKTestDefault(), SDKTestDefaultSubclass.getDerived()];
  }

  /**
   * Pre decrement within an indexer --j.
   * Create array c= [a,b], set j=1, set c[--j] = c[j]+add
   * push j to c and return c
   * expected result: c[0] is set to c[0]+add, so, [a+add,b,0]
   * @param {int} a
   * @param {int} b
   * @param {int} add
   * @returns {[int]} Stuff
   */
  preDecrement(a, b, add) {
    const c = [a, b];
    let j = 1;
    c[--j] = c[j] + add;
    c.push(j);
    return c;
  }

  /**
   * Post decrement within an indexer j--.
   * Create array c= [a,b], set j=1, set c[j--] = c[j]+add
   * push j to c and return c
   * expected result: c[1] is set to c[0]+add, so, [a,a+add, 0]
   * @param {int} a
   * @param {int} b
   * @param {int} add
   * @returns {[int]} Stuff
   */
  postDecrement(a, b, add) {
    const c = [a, b];
    let j = 1;
    c[j--] = c[j] + add;
    c.push(j);
    return c;
  }

  /**
   * Return a JS dictionary
   * @returns {object} Stuff
   */
  returnAMixedType() {
    return {
      anInt: 4,
      aFloat: 1.1,
      aString: 'testing',
      anObject: new SDKTestDefault(),
      aBool: true,
      aNull: null,
      aMixed: {},
    };
  }

  /**
   * Take a JS dictionary and return it
   * @param {object} stuff
   * @returns {object} Stuff
   */
  takeAMixedType(stuff) {
    return stuff;
  }

  /**
   * Throw an exception
   */
  throwOne() {
    this._throw();
  }

  _throw() {
    throw new Error('throwOne should be in the stack.');
  }

  /**
   * Fetch some JSON from httpbin.org
   * @param {SDKTest~fetched} callback Called on completion
   */
  async goFetch(callback) {
    try {
      const result = await fetch('https://httpbin.org/get');
      console.log(`fetch completed ${Object.getOwnPropertyNames(result)}`);
      callback(null, result);
    } catch (x) {
      console.error('error', `fetch failed ${x.message}`);
      callback(x, null);
    }
  }

  /**
   * Returns a new instance of this class
   * @returns {SDKTest} instance
   */
  static staticMethod() {
    return new SDKTest();
  }
}

/**
 * Callback for fetch method
 * @callback SDKTest~fetched
 * @param {error} error What went wrong
 * @param {object} response What the server said
 */

/**
 * Callback for echo method
 * @callback SDKTest~echo
 * @param {error} error What went wrong
 * @param {string} arg What you said
 */

/**
 * Callback for echo method with return
 * @callback SDKTest~echoReturn
 * @returns {string} arg What you said
 */

/**
 * Simple event
 * @event SDKTest#fakeEvent
 * @param {SDKTestDefault} item An item.
 */

/**
 * Valid SDK statuses
 * @enum {int}
 */
SDKTest.Statuses = {
  /**
   * comments matter apparently
   */
  ON_FIRE: 0,

  /**
   * you must write one
   */
  WET: 1,

  /**
   * just, shhhh.
   */
  UNINMAGINABLE: 2,
};
