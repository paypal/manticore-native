/**
 * @class
 * @property {string} stringProperty A string
 * @property {int} intProperty An int
 */
export default class Hello {

    /**
     * Create an instance of Hello.
     * @constructor
     * @param {string} stringProperty A string
     * @param {int} intProperty An int
     */
    constructor(stringProperty, intProperty) {
        this.stringProperty = stringProperty;
        this.intProperty = intProperty;
    }

    /**
     * @method
     * @returns {string}
     */
    toString() {
	return "{\n\t\"" + this.stringProperty + "\",\n\t" + this.intProperty + "\n}";
    }

    /**
     *
     */
    testCallback(callback) {
	  callback();
    }

    set stringProperty(value) {
	this._stringProperty = value;
    }

    get stringProperty() {
	return this._stringProperty;
    }

    set intProperty(value) {
	this._intProperty = value;
    }

    get intProperty() {
	return this._intProperty;
    }
}
