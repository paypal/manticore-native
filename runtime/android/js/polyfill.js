const g = global;
const m = g.manticore;
// Setup the JavaScriptCore runtime to look like what Manticore requires (bind native functions)
m.log('info', 'Loading android polyfill');

require('core-js/es6/symbol');
require('core-js/es6/set');
require('core-js/fn/string/includes');
require('core-js/fn/object/is');
require('core-js/fn/object/assign');
require('core-js/fn/array/of');
require('core-js/fn/array/from');
require('core-js/fn/array/find');
require('core-js/fn/array/find-index');
require('core-js/fn/symbol/iterator');

require('../../common/console');
require('../../common/promise');
require('../../common/timer');
require('../../common/fetch');

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

m.log('info', 'Loaded android polyfill');
