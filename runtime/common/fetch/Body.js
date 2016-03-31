/**
 * Our implementation is a bit funny... Since the native layer is callback based, we basically
 * will stack up waiting for a signal from native to release all the promises.
 */
export default class Body {
  constructor(body, opts) {
    const options = opts || {};

    this._body = body;
    this.bodyUsed = false;
    this.size = options.size || 0;
    this.timeout = options.timeout || 0;
    this.isBase64 = !!options.isBase64;
  }

  async json() {
    return this._body.json();
  }

  async text() {
    return this._body.text();
  }

  async body() {
    return this._body.body();
  }
}
