import * as infra from './infra';
import * as SDK from './js-source/index';

infra.test('Node.js runtime', (suite) => {
  suite.test('Should export the right objects', (t) => {
    t.ok(SDK.SDKTest, 'Should export SDKTest');
    t.ok(SDK.SDKTestDefault, 'Should export SDKTestDefault');
    t.ok(SDK.SDKTestDefaultSubclass, 'Should export SDKTestDefaultSubclass');
    t.end();
  });

  suite.test('Defaults should work', (t) => {
    const sdkTestDefault = new SDK.SDKTestDefault();
    t.ok(sdkTestDefault, 'Should create SDKTest instance');
    t.end();
  });

  suite.test('Fetch should work', async (t) => {
    const sdkTest = new SDK.SDKTest();
    try {
      sdkTest.goFetch((err, rz) => {
        t.ok(!err, 'Fetch should not error');
        t.ok(rz, 'Fetch should get result');
        t.end();
      });
    } catch (fetchError) {
      t.end(fetchError);
    }
  });
});
