/**
 * Our implementation is a bit funny... Since the native layer is callback based, we basically
 * will stack up waiting for a signal from native to release all the promises.
 */
export default class Body {
  constructor(body, opts) {
    const options = opts || {};

    this._fulfillers = [];
    this.body = body;
    this.bodyUsed = false;
    this.size = options.size || 0;
    this.timeout = options.timeout || 0;
  }

  async json() {
    return JSON.parse(await this._decode());
  }

  async text() {
    return await this._decode();
  }

  // TODO this in theory allows multiple calls to json/text/etc. Not sure that's actually to spec.
  async _decode() {
    if (this.bodyUsed) {
      return this._raw;
    }
    let accept;
    let reject;
    const promise = new Promise((_accept, _reject) => {
      accept = _accept;
      reject = _reject;
    });
    const fulfill = (error, body) => {
      if (error) {
        reject(error);
      } else {
        accept(body);
      }
    };
    this._fulfillers.push(fulfill);
    return promise;
  }

  _completed(error, base64) {
    this._raw = base64;
    for (const fn of this._fulfillers) {
      fn(error, base64);
    }
  }
}
