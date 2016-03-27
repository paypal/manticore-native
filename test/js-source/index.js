console.log('Loading sample JS');

import { SDKTest } from './SDKTest';
import { SDKTestDefault } from './SDKTestDefault';

Object.assign(global.exports, { SDKTest, SDKTestDefault });

console.log('Completed sample JS');
