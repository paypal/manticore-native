const g = global;
g.Promise = require('yaku');
g.regeneratorRuntime = require('babel-regenerator-runtime');

g.manticore.asCallback = function asCallback(p, c) {
  p.then((r) => c(null, r)).catch((e) => c(e, null));
};
