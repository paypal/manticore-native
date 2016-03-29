import * as infra from './infra';
import path from 'path';
import glob from 'glob';
import { generate } from '../gen/index';

const filePattern = path.join(__dirname, 'js-source/*.js');
const inputFiles = glob.sync(filePattern).sort();

// wrapper around the main function to be tested, filling in common arguments
function makeGeneratorWrapper(templateDir, outputDir) {
  return async (overrideOpts) => {
    // sensible defaults that can be overridden
    const defaultOpts = { q: true };
    for (const key in overrideOpts) {
      if ({}.hasOwnProperty.call(overrideOpts, key)) {
        defaultOpts[key] = overrideOpts[key];
      }
    }

    return await generate(templateDir,
      require(path.join(templateDir, 'config.json')),
      outputDir,
      defaultOpts,
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
    t.end();
  }));
});
