import * as infra from './infra';
import fs from 'fs';
import path from 'path';
import glob from 'glob';
import { generate } from '../gen/index';

const filePattern = path.join(__dirname, 'js-source/*.js');
const inputFiles = glob.sync(filePattern).sort();

// wrapper around the main function to be tested, filling in common arguments
function makeGeneratorWrapper(templateDir, outputDir) {
  return async (overrideOpts) => {
    return await generate(templateDir,
      require(path.join(templateDir, 'config.json')),
      outputDir,
      Object.assign({}, { q: true }, overrideOpts),
      inputFiles);
  };
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

    infra.wipe(outputDir);
    await genWrapper({ s: true });
    const full = path.join(outputDir, 'model.json');
    t.ok(!fs.existsSync(full), `${full} should not exist when suppressed`);
    const enumContent = fs.readFileSync(path.join(outputDir, 'enumsFilename.enums'));
    t.equal(enumContent.toString(), 'msg=', 'Enum ouptut should match');
    t.end();
  }));
});
