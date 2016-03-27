// Decimals are handled by converting to a string. It's your job
// to convert from that to whatever format you want (e.g. bignumber.js)
class FakeDecimal {
  constructor(val) {
    this.val = val;
  }

  toString() {
    return this.val;
  }
}

/**
 * @class
 * @property {int} test It's 1
 * @property {bool} itsTrue It's true
 * @property {bool} itsFalse It's false
 * @property {int} blankInt Starts blank
 * @property {int} intOne Starts 1
 * @property {decimal} blankDecimal Starts blank
 * @property {decimal} decimalHundredOhOne Starts 100.01
 * @property {string} nullString It's a null string.
 * @property {Date} now It's now
 * @property {[string]} stringArray An array of a, b, c
 */
export class SDKTestDefault {
  constructor() {
    this.test = 1;
    this.itsTrue = true;
    this.itsFalse = false;
    this.intOne = 1;
    this.fakeStuff = 123456;
    this.stringArray = ['a', 'b', 'c'];
    this.decimalHundredOhOne = new FakeDecimal('100.01');
    this.now = new Date();
  }

  /**
   * Test closure
   * @returns {bool} true
   */
  isItTrue() {
    return this.fakeStuff === 123456;
  }

  toString() {
    return `This is an SDK Default object: ${this.itsTrue}`;
  }
}

/**
 * @class
 * @extends SDKTestDefault
 */
export class SDKTestDefaultSubclass extends SDKTestDefault {
  constructor() {
    super();
    this._native = 'SDKTestDefaultSubclass';
  }

  /**
   * Test subclass
   * @returns {bool} true
   */
  isItDerived() {
    return true;
  }

  /**
   * Test derived classes
   * @returns {SDKTestDefault}
   */
  static getDerived() {
    return new SDKTestDefaultSubclass();
  }
}
