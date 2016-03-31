import Body from './Body';
import Headers from './Headers';

export default class Response extends Body {
  constructor(body, opts) {
    const options = opts || {};

    super(body, options);
    this.url = options.url;
    this.status = options.status;
    this.headers = new Headers(options.headers);
    this.ok = this.status >= 200 && this.status < 300;
  }
}
