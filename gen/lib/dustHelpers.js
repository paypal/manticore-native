import dust from 'dustjs-linkedin';
import util from 'util';
import _ from 'lodash';

require('dustjs-helpers');

function isEnum(enums, type) {
  for (let i = 0; i < enums.length; i++) {
    if (type === enums[i].dottedName) {
      return true;
    }
  }
  return false;
}

/**
 * Workhorse function that will render the appropriate body based on the type parameter
 * Renders one of bodies.:
 *  array - it's an array type
 *  enum - it's an enum type
 *  custom - the type is in "customConverters" in settings.json
 *  builtin - the type is in "nativeConverters" in settings.json
 *  block - none of the above are true (usually means it's our type)
 */
dust.helpers.converter = (outerchunk, context, bodies, params) =>
  outerchunk.map(chunk => {
    const lang = context.get('languageSettings');
    const type = dust.helpers.tap(params.type, chunk, context);
    if (type[0] === '[') {
      if (bodies.array) {
        return bodies.array(chunk, context.push({
          elementType: type.substring(1, type.length - 1),
        })).end();
      }
    } else if (isEnum(context.get('enums'), type)) {
      if (bodies.enum) {
        return bodies.enum(chunk, context).end();
      }
    } else if (_.includes(lang.customConverters, type)) {
      if (bodies.custom) {
        return bodies.custom(chunk, context).end();
      }
    } else if (lang.nativeConverters && lang.nativeConverters[type]) {
      const modcontext = context.push({ converter: lang.nativeConverters[type] });
      if (bodies.builtin) {
        return bodies.builtin(chunk, modcontext).end();
      }
    } else {
      if (bodies.block) {
        return bodies.block(chunk, context).end();
      }
    }
    return chunk.end('');
  });


/**
 * Get the native language type for a given JS type
 */
dust.helpers.type = (outerchunk, context, bodies, params) => {
  const lang = context.get('languageSettings');
  let append = '';
  let noPointers = params.noPointers;
  return outerchunk.capture(bodies.block, context, (original, chunk) => {
    let isArray = false;
    let string = original;
    if (string[0] === '[') {
      isArray = true;
      string = string.substring(1, string.length - 1);
    }
    let backup = string;
    let custom = lang.typeMap[string];
    if (string.indexOf('~') > 0) {
      // This is a callback, handle it separately...
      const cbInfo = string.split('~');
      append = lang.callbackSuffix;
      noPointers = true;
      if (lang.innerCallbacksAndEvents) {
        backup = cbInfo[1][0].toUpperCase() + cbInfo[1].substring(1);
      } else {
        backup = cbInfo[0] + cbInfo[1][0].toUpperCase() + cbInfo[1].substring(1);
      }
    }
    if (lang.hasPointers) {
      let ptr = noPointers ? '' : '*';
      if (isEnum(context.get('enums'), string) ||
        (lang.valueTypes && _.includes(lang.valueTypes, string))) {
        ptr = '';
      }
      let packed = backup.replace('.', '');
      if (lang.renames && lang.renames[packed]) {
        packed = lang.renames[packed];
      }
      backup = context.get('typePrefix') + packed + ptr;
      if (custom) {
        custom += ptr;
      }
    }
    let final = (custom || backup).replace('.', '');
    if (lang.renames && lang.renames[final]) {
      final = lang.renames[final];
    }
    if (append) {
      final = final + append;
    }
    if (isArray) {
      if (lang.arrayIsTyped) {
        final = util.format(lang.arrayType, final);
      } else {
        final = lang.arrayType;
      }
    }
    chunk.end(final);
  });
};

/**
 * Conditionally print the source of this file
 */
dust.helpers.sourceDocument = (chunk, context) => {
  if (context.get('suppressJs') === true) return chunk;
  return chunk.write(`Generated from: ${context.get('filename')}`);
};

dust.helpers.sdkType = (outerchunk, context, bodies, params) =>
  outerchunk.map(chunk => {
    const type = dust.helpers.tap(params.type, chunk, context);
    const lang = context.get('languageSettings');
    if (type.indexOf('~') >= 0 || _.some(context.get('enums'), t => t.dottedName === type)) {
      // TODO this is a hack... We're going to need to just have modules specify interdependencies.
      return chunk.end();
    }
    if (type === 'error' || type.indexOf(lang.prefix) === 0 || !lang.typeMap[type]) {
      return bodies.block(chunk, context).end();
    }
    return chunk.end();
  });


/**
 * For Objective-C, get "assign" or "strong" for ARC management
 */
dust.helpers.arcSetting = (outerchunk, context, bodies) => {
  const lang = context.get('languageSettings');
  return outerchunk.capture(bodies.block, context, (string, chunk) => {
    if (_.includes(lang.valueTypes, string) || isEnum(context.get('enums'), string)) {
      chunk.end('assign');
    } else {
      chunk.end('strong');
    }
  });
};

dust.helpers.nullabilityProp = (outerchunk, context, bodies) => {
  const lang = context.get('languageSettings');
  return outerchunk.capture(bodies.block, context, (string, chunk) => {
    if (_.includes(lang.valueTypes, string) || isEnum(context.get('enums'), string)) {
      chunk.end('');
    } else {
      chunk.end(',nullable');
    }
  });
};

dust.helpers.nullabilityType = (outerchunk, context, bodies) => {
  const lang = context.get('languageSettings');
  return outerchunk.capture(bodies.block, context, (string, chunk) => {
    if (string.indexOf('~') > 0) {
      // It's a callback...
      chunk.end(' _Nullable');
    } else if (_.includes(lang.valueTypes, string) || isEnum(context.get('enums'), string)) {
      chunk.end('');
    } else {
      chunk.end(' _Nullable');
    }
  });
};

dust.helpers.isStrong = (outerchunk, context, bodies, params) => {
  const lang = context.get('languageSettings');
  return outerchunk.map(chunk => {
    const type = dust.helpers.tap(params.type, chunk, context);
    if (_.includes(lang.valueTypes, type) || isEnum(context.get('enums'), type)) {
      if (bodies.else) {
        return bodies.else(chunk, context).end();
      }
    } else {
      if (bodies.block) {
        return bodies.block(chunk, context).end();
      }
    }
    return chunk.end('');
  });
};

/**
 * Capitalize the first letter of a string.
 */
dust.helpers.firstCap = (outerchunk, context, bodies) =>
  outerchunk.capture(bodies.block, context, (string, chunk) => {
    if (!string || !string[0]) {
      return chunk.end();
    }
    return chunk.end(string[0].toUpperCase() + string.substring(1));
  });

/**
 * Split multiline strings into something formated like this docstring
 * (the one you're reading right now)
 * with indentation and a prefix string
 */
dust.helpers.lineify = (outerchunk, context, bodies, params) => {
  // Is there content between our helper tags?
  const indent = params.indent || 1;
  const prefix = params.prefix || '* ';

  if (bodies.block) {
    return outerchunk.capture(bodies.block, context, (string, chunk) => {
      function formatLines(currentVal, index) {
        const ind = new Array(indent + 1).join(' ');
        return index === 0 ? currentVal : ind + prefix + currentVal.trim();
      }
      chunk.end(string.split('\n').map(formatLines).join('\n'));
    });
  }
  // If not, just return the existing chunk
  return outerchunk;
};

dust.helpers.isCallback = (outerchunk, context, bodies, params) =>
  outerchunk.map(chunk => {
    const type = dust.helpers.tap(params.type, chunk, context);
    if (type.indexOf('~') > 0) {
      if (bodies.block) {
        return bodies.block(chunk, context).end();
      }
    } else {
      if (bodies.else) {
        return bodies.else(chunk, context).end();
      }
    }
    return chunk.end('');
  });

/**
 * Strip off the stuff before ~ in a callback type name
 */
dust.helpers.callbackName = (outerchunk, context, bodies) =>
  outerchunk.capture(bodies.block, context, (string, chunk) => {
    chunk.end(string.split('~')[1]);
  });

/**
 * Strip off the braces
 */
dust.helpers.arrayType = (outerchunk, context, bodies) =>
  outerchunk.capture(bodies.block, context, (string, chunk) => {
    chunk.end(string.substring(1, string.length - 1));
  });
