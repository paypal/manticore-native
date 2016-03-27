const g = global;

// Setup the JavaScriptCore runtime to look like what Manticore requires (bind native functions)
g.manticore.log('info', 'Loading objc polyfill');

require('core-js/es6');
require('../../common/console');

if (!g.setTimeout) {
  g.setTimeout = g.manticore.setTimeout;
}

g.Promise = require('yaku');
g.regeneratorRuntime = require('babel-regenerator-runtime');

require('../../common/fetch');
g.manticore.log('info', 'Loaded objc polyfill');
