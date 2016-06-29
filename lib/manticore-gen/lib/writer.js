import fs from 'fs';
import path from 'path';
import _ from 'lodash';
import dust from 'dustjs-linkedin';
import Promise from 'bluebird';

require('./dustHelpers');
dust.config.whitespace = true;

/**
 * @param templateName The name of the template to render.
 * @param globals Model object indicating global context
 * @param model The top-level model object containing type information.
 * @param jobs List of jobs to add a job function to for each piece of data in fileDatas.
 * @param filenameFunc the filename to render into.
 */
function addRenderOneFileJob(templateName, globals, model, jobs, filename) {
  const globalContext = dust.context(globals);
  const globalModel = globalContext.push(model);

  jobs.push(new Promise((accept, reject) => {
    dust.render(templateName, globalModel, (err, content) => {
      if (err) {
        return reject(err);
      }
      return accept({
        content,
        filename,
      });
    });
  }));
}

/**
 * @param templateName The name of the template to render.
 * @param globals Model object indicating global context
 * @param model The top-level model object containing type information.
 * @param fileDatas List of data objects. One file will be rendered for each object in this list.
 * @param language
 * @param languageSettings
 * @param jobs List of jobs to add a job function to for each piece of data in fileDatas.
 * @param filenameFunc A function that takes a data object from fileDatas and
 * returns the filename to render into.
 */
function addRenderMultipleFilesJob(templateName, globals, model, fileDatas,
                                   languageSettings, jobs, filenameFunc) {
  for (const fileData of fileDatas) {
    const fileModel = _.defaults({}, fileData, {
      typePrefix: model.typePrefix,
      baseClass: model.baseClass,
      namespace: model.namespace,
      languageSettings,
      enums: model.enums,
      classes: model.classes,
    });

    addRenderOneFileJob(templateName, globals, fileModel, jobs, filenameFunc(fileData));
  }
}

/**
 * @param directory The directory containing dust templates that should be
 *  loaded into the dust engine
 * @param dusts Container for all of the dust templates that will be loaded
 *  into the dust engine
 */
function loadDustTemplates(directory, dusts) {
  fs.readdirSync(directory).forEach((t) => {
    if (t.match(/\.dust$/)) {
      const label = path.basename(t, '.dust');
      // THESE GO INTO THE DUST MODULE and OUR TEMPLATE NAMES COLLIDE.
      // so you can't (safely) generate multiple languages in parallel.
      const compiled = dust.compile(fs.readFileSync(path.join(directory, t), 'utf8'), label);

      dust.loadSource(compiled);
      dusts[label] = 1;
    }
  });
}

/**
 * Sanity check -- if the model is empty, something upstream probably failed
 * @param model object that represents the model
 */
function modelLooksReasonable(model) {
  const whatToCheck = ['rootTypes',
    'allTypes',
    'classes',
    'enums',
    'allCallbacks',
    'referencedTypes'];

  for (const key of whatToCheck) {
    // TODO: array vs object tests?
    if (model[key]) return true;
  }
  return false;
}

export async function renderTypes(model, templateDir, outputSpec, options, languageSettings) {
  const dusts = {}; // dusts[base of filename] = boolean

  function fail(msg) {
    throw new Error(msg);
  }

  function checkBasename(templateName) {
    if (!(templateName in languageSettings.basenames)) {
      fail(`No basename found in 'configuration while running template '${templateName}'`);
    }
  }

  function checkExtension(templateName) {
    if (!(templateName in languageSettings.extensions)) {
      fail(`No extension found in configuration while running template '${templateName}'`);
    }
  }

  if (undefined === languageSettings) {
    fail('No config.json was found');
  }

  if (!modelLooksReasonable(model)) {
    fail('No type information was received');
  }

  dust.loadSource(dust.compile('{@contextDump/}', 'model'));
  if (options.base) {
    loadDustTemplates(options.templates, dusts);
  }

  loadDustTemplates(templateDir, dusts);
  dusts.model = 1;

  // Run any renames
  if (languageSettings.renames) {
    for (const t of model.allTypes) {
      if (languageSettings.renames[t.packedName]) {
        t.packedName = languageSettings.renames[t.packedName];
      }
    }
  }

  // Add important items to a global dust context in addition to the passed in model
  const globals = {};
  globals.languageSettings = languageSettings;
  globals.suppressJs = options.s;
  globals.typePrefix = languageSettings.typePrefix;
  globals.baseClass = languageSettings.baseClass;
  globals.namespace = languageSettings.namespace;
  Object.assign(model, globals);

  // Depending on what templates exist, run them over the matching items in the model.
  // We'll work from a list of jobs.
  const jobs = [];

  // templates we know about
  const singleFiles = ['header', 'impl', 'enums', 'typedefs', 'importAll'];
  const multiFiles = {
    classHeader: model.classes,
    class: model.classes,
    enum: model.enums,
  };

  // don't mention JSON if suppressed
  if (!(options.s)) {
    addRenderOneFileJob('model', globals, model, jobs, path.join(outputSpec, 'model.json'));
  }

  // here are the single templates, we generate many things from the template name
  for (const t of singleFiles) {
    if (dusts[t]) {
      checkBasename(t);
      checkExtension(t);
      // generate filename.  for now, the dot is included in extensions! TODO
      const fname = languageSettings.basenames[t] + languageSettings.extensions[t];
      addRenderOneFileJob(t, globals, model, jobs, path.join(outputSpec, fname));
    }
  }

  // here are the multi templates
  function makeJob(extension) {
    return c => path.join(outputSpec, model.typePrefix + c.packedName + extension);
  }

  for (const t in multiFiles) {
    if (dusts[t]) {
      checkExtension(t);
      addRenderMultipleFilesJob(t, globals, model, multiFiles[t],
        languageSettings, jobs, makeJob(languageSettings.extensions[t]));
    }
  }

  return await Promise.all(jobs);
}
