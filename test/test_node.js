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

  suite.test('Fetch should work', infra.wrapTest(async(t) => {
    const sdkTest = new SDK.SDKTest();
    sdkTest.goFetch((err, rz) => {
      t.ok(!err, 'Fetch should not error');
      t.ok(rz, 'Fetch should get result');
      t.equal(rz.args.foo, 'bar', 'Should echo query string arg');
      t.end();
    });
  }));

  suite.test('Promise fetch should work', async(t) => {
    const sdkTest = new SDK.SDKTest();
    const rz = await sdkTest.goFetchP();
    t.ok(rz, 'Promise fetch should get result');
    t.equal(rz.args.baz, 'bop', 'Promise fetch should echo query string arg');
    t.end();
  });
});
