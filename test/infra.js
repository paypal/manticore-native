import tape from 'tape';
import glob from 'glob';
import fs from 'fs';
import path from 'path';

global.Promise = require('bluebird');
global.Promise.longStackTraces(true);

export function test(name, fn) {
  return tape(name, fn);
}

export function wrapTest(fn) {
  return (t) => {
    const result = fn(t);
    if (result instanceof Promise) {
      result.catch((err) => {
        t.end(err);
      });
    }
  };
}

// assert that a list of files exist
export function assertFiles(t, dir, fileList) {
  fileList.forEach((file) => {
    const full = path.join(dir, file);
    t.ok(fs.existsSync(full), `${full} should exist`);
  });
}

// assert that a set of build files match their counterparts in the expected-outputs folder
export function assertContents(t, dir) {
  const expectedPath = dir.replace('test/output', 'test/expected-outputs');
  const expectedFiles = glob.sync(`${expectedPath}/*`);
  t.ok(expectedFiles.length > 0, `'expected output' files should found in ${expectedPath}`);

  expectedFiles.forEach((expectedFile) => {
    // make sure otherFile (the one that should have been built) exists
    const otherFile = path.join(dir, path.basename(expectedFile));
    t.ok(fs.existsSync(otherFile), `${otherFile} should exist`);

    // diff the 2 files
    t.equal(fs.readFileSync(expectedFile).toString(), fs.readFileSync(otherFile).toString(),
      `${otherFile} should match ${expectedFile}`);
  });
}

export function wipe(dir) {
  glob.sync(`${dir}/*`).forEach(fs.unlinkSync);
}
