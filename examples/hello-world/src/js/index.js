var manticore = require('manticore-native');

import Hello from './Hello';

module.exports = {
  Hello,
};

manticore.nativeExport(module, { Hello });
