function logger(level, args) {
  global.manticore.log(level, args.join(' '));
}

global.console = {
  log(...args) {
    logger('debug', args);
  },
  warn(...args) {
    logger('warn', args);
  },
  error(...args) {
    logger('error', args);
  },
};
