import Body from './Body';
import Headers from './Headers';

export default class Response extends Body {
  constructor(native, request) {
    super(native, {});
    this.url = request.url;
    this.status = native.status;
    this.headers = new Headers(native.headers);
    this.ok = this.status >= 200 && this.status < 300;
  }
}
