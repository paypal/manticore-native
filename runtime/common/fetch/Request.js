import Body from './Body';
import Headers from './Headers';

export default class Request extends Body {
  /**
   * Request class
   *
   * @param   Mixed   input  Url or Request instance
   * @param   Object  init   Custom options
   * @return  Void
   */
  constructor(input, init) {
    let url;
    let _input = input;
    const _init = init || {};

    if (!(input instanceof Request)) {
      url = input;
      _input = {};
    } else {
      url = input.url;
    }
    // Bypass URL validation. Don't screw it up, caller.

    super(_init.body || _input.body, {
      timeout: _init.timeout || _input.timeout || 0,
      size: _init.size || _input.size || 0,
    });

    this.method = _init.method || _input.method || 'GET';
    this.headers = new Headers(_init.headers || _input.headers || {});
    this.url = url;
  }

  clone() {
    return new Request(this);
  }
}
