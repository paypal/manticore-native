// A simple utility script to browserify documents as we want
import fs from 'fs';
import browserify from 'browserify';
import path from 'path';
import mkdirp from 'mkdirp';
import glob from 'glob';

let files = process.argv.slice(3);
const output = process.argv[2];
const options = {};

if (require('os').platform() === 'win32') {
  let targ = [];
  for (const file of files) {
    targ = targ.concat(glob.sync(file));
  }
  files = targ;
}

mkdirp.sync(path.dirname(output));

const babelRc = JSON.parse(fs.readFileSync(path.join(__dirname, '.babelrc')));
let b = browserify(files, options).transform('babelify', babelRc);

if (process.env.UGLIFYIFY) {
  b = b.transform({
    global: true,
  }, 'uglifyify');
}

b.bundle().pipe(fs.createWriteStream(output));
