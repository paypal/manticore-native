function logger(level, args) {
  global.manticore.log(level, Array.prototype.slice.call(args).join(' '));
}

global.console = {
  log() {
    logger('debug', arguments);
  },
  warn() {
    logger('warn', arguments);
  },
  error() {
    logger('error', arguments);
  }
};
