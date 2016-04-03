export default class Headers {
  constructor(headers) {
    this._headers = {};
    for (const prop of Object.getOwnPropertyNames(headers)) {
      const typ = typeof headers[prop];
      const v = headers[prop];
      if (typ === 'string') {
        this.set(prop, headers[prop]);
      } else if (typ === 'number' && !isNaN(v)) {
        this.set(prop, headers[prop].toString());
      } else if (v instanceof Array) {
        for (const item of v) {
          this.append(prop, item.toString());
        }
      }
    }
  }

  get(name) {
    const list = this._headers[name.toLowerCase()];
    return list ? list[0] : null;
  }

  getAll(name) {
    if (!this.has(name)) {
      return [];
    }
    return this._headers[name.toLowerCase()];
  }

  forEach(callback, thisArg) {
    for (const name of Object.getOwnPropertyNames(this._headers)) {
      for (const value of this._headers[name]) {
        callback.call(thisArg, value, name, this);
      }
    }
  }

  set(name, value) {
    this._headers[name.toLowerCase()] = [value];
  }

  append(name, value) {
    if (!this.has(name)) {
      this.set(name, value);
      return;
    }
    this._headers[name.toLowerCase()].push(value);
  }

  has(name) {
    return this._headers.hasOwnProperty(name.toLowerCase());
  }

  delete(name) {
    delete this._headers[name.toLowerCase()];
  }

  raw() {
    return this._headers;
  }
}
