console.log('Loading sample JS');

import { SDKTest } from './SDKTest';
import { SDKTestDefault } from './SDKTestDefault';
import { exports } from '../../index';

Object.assign(exports, { SDKTest, SDKTestDefault });

console.log('Completed sample JS');
