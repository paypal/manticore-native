/**
 * This serves as both the request and response bodies, and this is only used in native
 * platforms (node-fetch is used on node, and Chrome uses fetch for reals).
 */
export default class Body {
  constructor(body) {
    this._body = body;
  }

  /**
   * Called by manticore to get the request body as a string
   * @returns either base64 or regular string depending on isBase64
   */
  nativeBody() {
    return this._body;
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
