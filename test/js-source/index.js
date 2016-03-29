console.log('Loading sample JS');

import { SDKTest } from './SDKTest';
import { SDKTestDefault, SDKTestDefaultSubclass } from './SDKTestDefault';
import { nativeExport } from '../../index';

nativeExport(module, { SDKTest, SDKTestDefault, SDKTestDefaultSubclass });

console.log('Completed sample JS');
