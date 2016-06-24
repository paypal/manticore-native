#!/usr/bin/env node

// https://github.com/babel/babel-eslint/issues/163#issuecomment-213620435
const eslintWorkaround = true; // eslint-disable-line no-unused-vars

import fs from 'fs-extra';
import { Parser } from './lib/reader';
import { renderTypes } from './lib/writer';
import path from 'path';
import glob from 'glob';
import _ from 'lodash';
import readline from 'readline';
import { determineCommand } from '../cli.js';

require('babel-polyfill');
global.Promise = require('bluebird');
global.Promise.longStackTraces();

function help(argv) {
  const cmd = determineCommand(argv);
  console.log(`
 Usage:
   ${cmd} [options] <template_config_dir> <outputDirectory> <inputFile1 [inputFile2 [...]] >

 Example:
   ${cmd} --test ./templates native-src/generated/somewhere/ src/input*.js

 Supported options:
   -q                     Don't log stuff, just do your job
   -s                     Suppress generated code comments that imply the use of Javascript
   --print                Print the files before writing them
   --test                 Don't write files, just say which ones you would write
   --glob                 Execute file glob (defaults to true for win32)
   --base=<path>          Use path as a "base configuration" with templates and config.json
`);
}

// return an object containing the options that were specified on the command line
// (we provide a name to the positional arguments)
// onError() { what to do }
function parseCliOptions(args) {
  const opts = {
    boolean: ['q', 'print', 'test', 'glob', 's'],
  };
  const program = require('minimist')(args, opts);
  if (program._.length < 3) {
    throw new Error('Invalid Arguments');
  }
  program.templates = program._[0];
  program.outputDirectory = program._[1];
  program.inputFiles = program._.slice(2);
  return program;
}

/**
 * validate the options themselves (i.e. not whether the command line was in the correct form)
 *
 * @param templateDir the directory with dust templates and configs
 * @param outputDirectory the directory where output should go
 * @param options an object of command line options (t, p, and q currently)
 * @param files an array of input files
 */
function validateOptions(templateDir, outDir, opts, files) {
  if (!fs.existsSync(templateDir)) {
    throw new Error(`${templateDir} does not exist`);
  }
  if (files.length === 0) {
    throw new Error('No input files were provided');
  }
}

/**
 * main entry point
 *
 * @param templateDirectory the directory with dust templates and configs
 * @param config The final (merged) configuration information
 * @param outputDirectory the directory where output should go
 * @param options an object of command line options (t, p, and q currently)
 * @param files an array of input files
 */
export async function generate(templateDirectory, config, outputDirectory, options, files) {
  validateOptions(templateDirectory, outputDirectory, options, files);

  // conditional log
  const log = (...args) => {
    if (!options.q) {
      console.log.apply(null, args);
    }
  };

  log('Beginning generation in', templateDirectory, 'using\n ', files.join('\n  '));

  // This container will be passed to the parser, and all information read will be deposited in it
  const typeInformation = {};
  let globbedFiles = files;

  if (require('os').platform() === 'win32' || options.glob === true) {
    let targ = [];
    for (const file of files) {
      targ = targ.concat(glob.sync(file));
    }
    globbedFiles = targ;
  }

  // read all the type information from the specified input files --
  // multiple classes per file is possible
  const results = await Promise.all(globbedFiles.map((filename) => {
    const parser = new Parser(path.relative(process.cwd(), filename), typeInformation);
    parser.readTypes();
    return parser;
  }));
  for (const parser of results) {
    parser.readTypeDetails();
  }

  // typeInformation is now complete; run the code generator for the target language
  const outputs =
    await renderTypes(typeInformation, templateDirectory, outputDirectory, options, config);

  let rl;
  if (options.print) {
    rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
    });
  }
  for (const fout of outputs) {
    if (options.print) {
      console.log(fout.content);
      const answer = await (new Promise((accept) => {
        rl.question(`[${fout.filename}] Press ENTER to continue...`, accept);
      }));
      if (answer.toLowerCase() === 'q') {
        rl.close();
        process.exit(0);
      }
    }

    // whether to say, or actually do
    if (options.test) {
      console.log('Would write', fout.content.length, 'bytes to', fout.filename);
    } else {
      log('Writing', fout.filename);

      fs.mkdirsSync(path.dirname(fout.filename));
      fs.writeFileSync(fout.filename, fout.content, 'utf8');
    }
  }
  if (rl) {
    rl.close();
  }
  return outputs;
}

/**
 * entry point for command line arguments; just parse and generate
 *
 * @param argv
 */
export function run(argv) {
  let opts;
  try {
    const args = argv.slice(2);
    opts = parseCliOptions(args);
  } catch (e) {
    help(argv);
    process.exit(-1); // OK to do this, we are called from command line
  }

  let config = {};
  if (opts.base) {
    config = require(path.resolve(path.join(opts.base, 'config.json')));
  }
  const overrides = require(path.resolve(path.join(opts.templates, 'config.json')));
  config = _.merge({}, config, overrides);
  return generate(
    path.resolve(opts.templates),
    config,
    opts.outputDirectory,
    opts,
    opts.inputFiles);
}

if (require.main === module) {
  run(process.argv)
    .catch((e) => {
      console.error('Generation failed', e.message, e.stack);
      process.exit(-1);
    });
}
