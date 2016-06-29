#!/usr/bin/env node

// https://github.com/babel/babel-eslint/issues/163#issuecomment-213620435
const eslintWorkaround = true; // eslint-disable-line no-unused-vars

// A simple utility script to browserify documents as we want
import fs from 'fs';
import browserify from 'browserify';
import path from 'path';
import mkdirp from 'mkdirp';
import glob from 'glob';
import { determineCommand } from '../cli.js';

// return an object containing the options that were specified on the command line
// (we provide a name to the positional arguments)
// onError() { what to do }
function parseCliOptions(args) {
  const opts = {
  };
  const program = require('minimist')(args, opts);
  if (program._.length < 2) {
    throw new Error('Invalid Arguments');
  }
  program.outputFile = program._[0];
  program.inputFiles = program._.slice(1);
  return program;
}

function help(argv) {
  const cmd = determineCommand(argv);
  console.log(`
 Usage:
   ${cmd} [options] <outputFile> <inputFile1 [inputFile2 [...]] >

 Example:
   ${cmd} generated/manticore-modules.js src/input*.js

 Supported options:
    TODO: probably stuff that you would pass to browserify
`);
}

export function mantify(inputFiles, outputFile, browserifyOptions) {
  let files = inputFiles;

  if (require('os').platform() === 'win32') {
    let targ = [];
    for (const file of files) {
      targ = targ.concat(glob.sync(file));
    }
    files = targ;
  }

  // TODO: make this pull from path.join(__dirname, '..', '..', '.babelrc');
  const babelRc = {
    presets: ['es2015'],
    sourceMaps: true,
    plugins: ['syntax-async-functions', 'transform-regenerator'],
  };

  let b = browserify(files, browserifyOptions).transform('babelify', babelRc);

  if (process.env.UGLIFYIFY) {
    b = b.transform({
      global: true,
    }, 'uglifyify');
  }

  mkdirp.sync(path.dirname(outputFile));
  b.bundle().pipe(fs.createWriteStream(outputFile));
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

  mantify(opts.inputFiles, opts.outputFile, {});
}
