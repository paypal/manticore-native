const g = global;
const m = g.manticore;
// Setup the JavaScriptCore runtime to look like what Manticore requires (bind native functions)
m.log('info', 'Loading android polyfill');

require('core-js/es6');
require('../../common/console');

if (!g.setTimeout) {
  g.setTimeout = m.setTimeout;
}

g.Promise = require('yaku');
g.regeneratorRuntime = require('babel-regenerator-runtime');

m.construct = function _construct(C, args) {
  function F() {
    return C.apply(this, args);
  }

  F.prototype = C.prototype;
  return new F();
};

m.newDate = function _newDate(t) {
  return new Date(t);
};

require('../../common/fetch');
m.log('info', 'Loaded android polyfill');
