const g = global;
const m = g.manticore;
// Setup the JavaScriptCore runtime to look like what Manticore requires (bind native functions)
m.log('info', 'Loading Jint polyfill');

require('core-js/es6');
require('../../common/console');

if (!g.setTimeout) {
  g.setTimeout = m.setTimeout;
}

g.Promise = require('yaku');
g.regeneratorRuntime = require('babel-regenerator-runtime');

m._ = {
  array() {
    return [];
  },
  fn(fnlike, count) {
    return function () {
      const a = arguments;
      switch (count) {
        case 0:
          return fnlike();
        case 1:
          return fnlike(a[0]);
        case 2:
          return fnlike(a[0], a[1]);
        case 3:
          return fnlike(a[0], a[1], a[2]);
        default:
          throw new Error('Do not make callbacks with so many arguments');
      }
    };
  },
  construct: function construct(C, a) {
    if (!C) return {};
    function F() { return C.apply(this, a); }
    F.prototype = C.prototype;
    return new F();
  },
};

require('../../common/fetch');
m.log('info', 'Loaded Jint polyfill');
