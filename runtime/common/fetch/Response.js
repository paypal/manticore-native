import Body from './Body';
import Headers from './Headers';

export default class Response extends Body {
  constructor(native, request) {
    const rz = native || {};

    super(rz, {});
    this.url = request.url;
    this.status = rz.status;
    this.headers = new Headers(rz.headers);
    this.ok = this.status >= 200 && this.status < 300;
  }
}
