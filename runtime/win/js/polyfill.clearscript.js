const g = global;
const m = g.manticore;

// Setup the JavaScriptCore runtime to look like what Manticore requires (bind native functions)
m._log('info', 'Loading ClearScript polyfill');

g.exports = g.exports || {};

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

m._ = {
  array() {
    return [];
  },

  // native functions (which these callbacks are) can't cope with variable argument length
  //   on this platform.
  //   we cannot use 'fnlike.apply(null, a);'
  //   we MUST match in argument count.
  fn(fnlike, count) {
    return function csFn(...a) {
      switch (count) {
        case 0:
          return fnlike();
        case 1:
          return fnlike(a[0]);
        case 2:
          return fnlike(a[0], a[1]);
        case 3:
          return fnlike(a[0], a[1], a[2]);
        case 4:
          return fnlike(a[0], a[1], a[2], a[3]);
        case 5:
          return fnlike(a[0], a[1], a[2], a[3], a[4]);
        case 6:
          return fnlike(a[0], a[1], a[2], a[3], a[4], a[5]);
        case 7:
          return fnlike(a[0], a[1], a[2], a[3], a[4], a[5], a[6]);
        default:
          throw new Error('Consider using an object instead of so many arguments in this callback');
      }
    };
  },
  construct: function construct(className, args) {
    if (!className) return {};
    const cons = g.exports[className];

    function F() {
      return cons.apply(this, args);
    }

    F.prototype = cons.prototype;
    return new F();
  },
  getClass: function getClass(className) {
    return g.exports[className];
  },
};

m._log('info', 'Loaded ClearScript polyfill');
