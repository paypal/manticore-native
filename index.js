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
 * Export a class to the native host by adding it here
 */
export const exports = g.exports || closure.exports;

/**
 * This allows "import manticore from 'manticore';" to do something useful
 */
export default native;
