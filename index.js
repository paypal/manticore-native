//
// A simple shim for all languages that isolates your JS code's interface to manticore services
//
const closure = {
  exports: {},
};
const g = global;

/**
 * The native interface is where services provided by the hosting process will be injected.
 * This is usually just "global.manticore," but in Node.js we don't really want to pollute
 * global like that.
 */
export const native = (() => {
  if (g.manticore) {
    return g.manticore;
  }
  closure.manticore = {};
  return closure.manticore;
})();

/**
 * Export a set of objects to the native layer, as well as
 * adding them to your module exports (for JS platforms)
 * @param module Your module (e.g. put "module" here)
 * @param exportsObj The object containing items to export
 */
export function nativeExport(module, exportsObj) {
  // const exportsObj = (exports === undefined ? module.exports : exports); // won't work; don't try
  if (exportsObj === null || typeof exportsObj !== 'object') {
    throw new Error(`nativeExport expected exports to be an object, got ${typeof exportsObj}`);
  }
  const nativeExports = g.exports || closure.exports;
  for (const k of Object.getOwnPropertyNames(exportsObj)) {
    nativeExports[k] = module.exports[k] = exportsObj[k];
  }
}

let _fetch = g.fetch;

// Trick browserify to avoid pulling node-fetch into all native bundles
// since the native bundles are in charge of providing their own fetch function
if (!_fetch) {
  const fakeBrowserify = require;
  _fetch = fakeBrowserify('node-fetch');
}

/**
 * The HTTP "fetch" function, exposed via manticore so that you can write your JS code
 * to work in both node and other supported manticore environments. Your code should
 * call manticore.fetch, and let higher modules (e.g. the app/tool) decide what
 * implementation that should use.
 * TODO this is not a faithful implementation of fetch yet. It's close enough for simple
 * tasks, but we should flesh this out a lot more in the polyfills for the various
 * runtimes (including node).
 */
export function fetch(...args) {
  return _fetch.apply(null, args);
}

/**
 * This allows "import manticore from 'manticore';" to do something useful
 */
export default native;
