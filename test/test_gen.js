import * as infra from './infra';
import fs from 'fs';
import path from 'path';
import glob from 'glob';
import { generate } from '../gen/index';

const filePattern = path.join(__dirname, 'js-source/*.js');
const inputFiles = glob.sync(filePattern).sort();

// wrapper around the main function to be tested, filling in common arguments
function makeGeneratorWrapper(templateDir, outputDir) {
  return async (overrideOpts) => (await generate(templateDir,
      require(path.join(templateDir, 'config.json')),
      outputDir,
      Object.assign({}, { q: true }, overrideOpts),
      inputFiles));
}

infra.test('Codegen', (suite) => {
  suite.test('should export generate function', (t) => {
    t.end();
  });

  suite.test('Should generate expected debug output', infra.wrapTest(async (t) => {
    const templateDir = path.join(__dirname, 'debug-templates');
    const outputDir = path.join(__dirname, '../output/debug');
    const genWrapper = makeGeneratorWrapper(templateDir, outputDir);

    infra.wipe(outputDir);
    await genWrapper({});
    infra.assertFiles(t, outputDir, [
      'model.json',
      'enumsFilename.enums',
      'headerFilename.header',
      'implFilename.impl',
      'importAllFilename.importAll',
      'myTypePrefixSDKTest.class',
      'myTypePrefixSDKTest.classHeader',
      'myTypePrefixSDKTestDefault.class',
      'myTypePrefixSDKTestDefault.classHeader',
      'myTypePrefixSDKTestStatuses.enum',
      'typedefsFilename.typedefs',
    ]);

    const enumModelContent = fs.readFileSync(path.join(outputDir, 'enumsFilename.enums'));
    t.equal(enumModelContent.toString(), 'msg=Generated from: undefined',
      'Enum ouptut should match');

    const mi = infra.getModelInspector(outputDir);
    t.ok(mi.rootTypes, 'Should have root types');
    t.ok(mi.enums, 'Should have enums');
    t.ok(mi.allCallbacks, 'Should have callbacks');
    t.ok(mi.referencedTypes, 'Should have referencedTypes');
    t.ok(mi.typePrefix, 'Should have type prefix');
    t.ok(mi.baseClass, 'Should have baseClass');
    t.ok(mi.namespace, 'Should have namespace');

    t.ok(mi.rootTypes.SDKTest, 'SDKTest type should exist');
    const sdkTestFilename = path.relative(process.cwd(), inputFiles[0]);
    t.equal(sdkTestFilename, mi.rootTypes.SDKTest.filename, 'SDKTest filename should match');
    t.ok(mi.rootTypes.SDKTest.methods.echo, 'echo function should exist');
    t.ok(mi.rootTypes.SDKTest.methods.echo.args.arg, 'echo function arg name');
    t.ok(mi.rootTypes.SDKTest.methods.echo.args.callback, 'echo function callback arg');
    t.ok(mi.rootTypes.SDKTest.fields.itsOne, 'SDKTest fields');
    t.equal('int', mi.rootTypes.SDKTest.fields.itsOne.type, 'SDKTest field type');
    t.equal('SDKTestDefault', mi.rootTypes.SDKTest.fields.complexType.type, 'complex type return');

    // and so on
    const defFilename = path.relative(process.cwd(), inputFiles[1]);
    t.equal(defFilename, mi.rootTypes.SDKTestDefault.filename, 'SDKTestDefault filename');

    t.ok(mi.enums.Statuses.values.ON_FIRE, 'enum value should exist');
    t.equal('SDKTest.Statuses', mi.rootTypes.SDKTest.fields.myStatus.type, 'enum field type');

    infra.wipe(outputDir);
    await genWrapper({ s: true });
    const full = path.join(outputDir, 'model.json');
    t.ok(!fs.existsSync(full), `${full} should not exist when suppressed`);
    const enumContent = fs.readFileSync(path.join(outputDir, 'enumsFilename.enums'));
    t.equal(enumContent.toString(), 'msg=', 'Enum output should match');
    t.end();
  }));

  function queueTest(languageName, relativePath, files) {
    suite.test(`Should generate ${languageName}`, infra.wrapTest(async (t) => {
      const templateDir = path.join(__dirname, relativePath);
      const outputDir = path.join(__dirname, `../output/${languageName}`);
      const genWrapper = makeGeneratorWrapper(templateDir, outputDir);

      infra.wipe(outputDir);
      await genWrapper({});
      infra.assertFiles(t, outputDir, files);

      infra.assertContents(t, outputDir);
      t.end();
    }));
  }

  queueTest('j2v8', '../runtime/android/templates', [
    'model.json',
    'SDKTest.java',
    'SDKTestDefault.java',
    'SDKTestDefaultSubclass.java',
    'SDKTestStatuses.java',
  ]);

  queueTest('objc', '../runtime/objc/templates', [
    'model.json',
    'PayPalManticoreImports.h',
    'PayPalManticoreTypes.h',
    'PPManticoreSDKTest.h',
    'PPManticoreSDKTest.m',
    'PPManticoreSDKTestDefault.h',
    'PPManticoreSDKTestDefault.m',
    'PPManticoreSDKTestDefaultSubclass.h',
    'PPManticoreSDKTestDefaultSubclass.m',
  ]);

  queueTest('csharp-clearscript', '../runtime/win/templates/clearscript', [
    'model.json',
    'SDKTest.cs',
    'SDKTestDefault.cs',
    'SDKTestDefaultSubclass.cs',
    'SDKTestStatuses.cs',
  ]);

  queueTest('csharp-jint', '../runtime/win/templates/jint', [
    'model.json',
    'SDKTest.cs',
    'SDKTestDefault.cs',
    'SDKTestDefaultSubclass.cs',
    'SDKTestStatuses.cs',
  ]);

  queueTest('xamarin-objc', '../runtime/xamarin/templates/objc', [
    'ApiDefinition.cs',
    'StructsAndEnums.cs',
  ]);
});
