import tape from 'tape';
import glob from 'glob';
import fs from 'fs-extra';
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
  const subpath = path.relative(path.join(__dirname, '../output'), dir);
  const expectedPath = path.join(__dirname, 'expected-outputs', subpath);
  const expectedFiles = glob.sync(`${expectedPath}/*`);
  const shouldAccept = !!process.env.ACCEPT_MANTICORE_OUTPUTS;
  if (shouldAccept) {
    fs.mkdirpSync(expectedPath);
    glob.sync(`${dir}/*`).forEach((f) => {
      fs.copySync(
        f,
        path.join(expectedPath, path.basename(f))
      );
      t.ok(true, `Accepting ${path.basename(f)} into ${expectedPath}`);
    });
  } else {
    t.ok(expectedFiles.length > 0, `'expected output' files should found in ${expectedPath}`);

    for (const expectedFile of expectedFiles) {
      // make sure otherFile (the one that should have been built) exists
      const otherFile = path.join(dir, path.basename(expectedFile));
      t.ok(fs.existsSync(otherFile), `${otherFile} should exist`);

      // diff the 2 files
      t.equal(fs.readFileSync(expectedFile).toString(), fs.readFileSync(otherFile).toString(),
        `${otherFile} should match ${expectedFile}`);
    }
  }
}

export function wipe(dir) {
  glob.sync(`${dir}/*`).forEach(fs.unlinkSync);
}

// convert an array of things with names to obj[name] = thing
function loadAnything(what, input, transformer) {
  const ret = {};
  if (input === undefined) return ret;

  const _transformer = transformer || ((i) => i);

  for (const myThing of input) {
    // console.log("making " + what + " " + myThing.name);
    ret[myThing.name] = _transformer(myThing);
  }
  return ret;
}

// convert array of variables to hash
// function args, fields, it's all good
function loadProperties(input) {
  return loadAnything('property', input);
}

class InspectorEnum {
  constructor(typeInformationModel) {
    this.name = typeInformationModel.name;
    this.filename = typeInformationModel.filename;
    this.description = typeInformationModel.description;
    this.location = typeInformationModel.location;
    this.values = loadProperties(typeInformationModel.values);
    this.type = typeInformationModel.type;
  }
}

// methods, events, it's all good
class InspectorFunction {
  constructor(typeInformationModel) {
    this.name = typeInformationModel.name;
    this.description = typeInformationModel.description;
    this.containingType = typeInformationModel.containingType;
    this.type = typeInformationModel.type;
    this.args = loadProperties(typeInformationModel.args);
  }
}

// convert array of methods to hash
function loadMethods(input) {
  return loadAnything('function', input, (myFunc) => new InspectorFunction(myFunc));
}

class InspectorClass {
  constructor(typeInformationModel) {
    this.name = typeInformationModel.name;
    this.filename = typeInformationModel.filename;
    this.description = typeInformationModel.description;
    this.methods = loadMethods(typeInformationModel.methods);
    this.staticMethods = loadMethods(typeInformationModel.staticMethods);
    this.fields = loadProperties(typeInformationModel.fields);
    // this.innerTypes = new Set(typeInformationModel.innerTypes);
    this.packedName = typeInformationModel.packedName;
    this.dottedName = typeInformationModel.dottedName;
    this.events = loadMethods(typeInformationModel.events);
    this.callbacks = loadMethods(typeInformationModel.callbacks);
    if (typeInformationModel.instanceConstructor) {
      this.instanceConstructor = new InspectorFunction(typeInformationModel.instanceConstructor);
    }
  }
}

// convert array of classes to hash
function loadClasses(input) {
  const ret = {};
  for (const myClass in input) {
    if ({}.hasOwnProperty.call(input, myClass)) {
      ret[myClass] = new InspectorClass(input[myClass]);
    }
  }
  return ret;
}

function loadEnums(input) {
  return loadAnything('enum', input, (myEnum) => new InspectorEnum(myEnum));
}

export class Inspector {
  // the argument should be a javascript object in the form outputted by the debug language
  // TODO: should probably copy the object first
  constructor(typeInformationModel) {
    this.rootTypes = loadClasses(typeInformationModel.rootTypes);
    this.enums = loadEnums(typeInformationModel.enums);
    this.allCallbacks = loadMethods(typeInformationModel.allCallbacks);
    this.referencedTypes = new Set(typeInformationModel.referencedTypes);
    this.typePrefix = typeInformationModel.typePrefix;
    this.baseClass = typeInformationModel.baseClass;
    this.namespace = typeInformationModel.namespace;
  }
}

// create a model inspector from the given directory (uses model.json)
export function getModelInspector(base) {
  const where = path.join(base, 'model.json');
  const model = require(where);
  return new Inspector(model);
}
