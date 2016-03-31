import fs from 'fs';
import util from 'util';
import assert from 'assert';
import Docchi from 'docchi';
import _ from 'lodash';

// We want async await support...
require('acorn-es7-plugin')(Docchi.acorn);

export class Parser {
  constructor(filename, typeInformation) {
    this.filename = filename;
    this.fileContents = fs.readFileSync(filename, 'utf8');
    this.docs = new Docchi(this.fileContents, {
      ecmaVersion: 7,
      plugins: {
        asyncawait: {
          awaitAnywhere: true,
        },
      },
    });
    this.docchiOutput = this.docs.output({
      render: false,  // don't render comments as HTML
    });
    this.typeInformation = typeInformation || {};
    this.pendingCallbacks = [];
    this.referencedTypes = {};
    _.defaults(this.typeInformation, {
      rootTypes: {},
      allTypes: [],
      classes: [],
      enums: [],
      allCallbacks: [],
      referencedTypes: [],
    });
  }

  readTypes() {
    // First pass gets the top level types
    for (let i = 0; i < this.docchiOutput.length; i++) {
      const type = this.commentType(this.docchiOutput[i]);
      if (type === 'class') {
        this.importClass(this.docchiOutput[i]);
      } else if (type === 'enum') {
        this.importEnum(this.docchiOutput[i]);
      } else if (type === 'callback') {
        this.importCallback(this.docchiOutput[i]);
      } else if (type === 'method' || type === 'constructor' ||
        type === 'property' || type === 'event') {
        // Skip
      } else if (type) {
        assert(false, `Unknown type ${type}`);
      }
    }
  }

  readTypeDetails() {
    // Second pass gets methods, events and other straggler comments
    for (let i = 0; i < this.docchiOutput.length; i++) {
      const type = this.commentType(this.docchiOutput[i]);
      if (type === 'constructor') {
        this.importMethod(this.docchiOutput[i], true);
      } else if (type === 'method') {
        this.importMethod(this.docchiOutput[i], false);
      } else if (type === 'property') {
        this.importProperty(this.docchiOutput[i]);
      } else if (type === 'event') {
        // Not ready yet.
        this.importEvent(this.docchiOutput[i]);
      }
    }
    // Extract enumeration values
    const lines = this.fileContents.replace('\r\n', '\n').split(/[\n]/);
    for (let i = 0; i < this.typeInformation.enums.length; i++) {
      const theEnum = this.typeInformation.enums[i];
      if (theEnum.filename !== this.filename) {
        continue; // somebody else's problem
      }
      let buf = lines[theEnum.location.start.line - 1].substring(theEnum.location.start.column);
      for (let l = theEnum.location.start.line; l < theEnum.location.end.line - 1; l++) {
        buf = [buf, lines[l]].join('\n');
      }
      buf = [
        buf,
        lines[theEnum.location.end.line - 1].substring(0, theEnum.location.end.column)]
        .join('\n');
      let enumValues = buf.split('=', 2);
      /* eslint-disable */
      enumValues = eval(`(function () { return ${enumValues[1]};})()`);
      /* eslint-enable */
      for (let v = 0; v < theEnum.values.length; v++) {
        theEnum.values[v].value = enumValues[theEnum.values[v].name];
      }
    }
    // Attach the callbacks to the types
    for (const cb of this.pendingCallbacks) {
      const typeName = cb.name.split('~')[0];
      const cbtype = this.resolveType(typeName);
      assert(cbtype, `Callback ${cb.name} missing corresponding type.`);
      cbtype.callbacks = cbtype.callbacks || [];
      cb.type = cbtype.name;
      cb.name = cb.name.split('~', 2)[1];
      cbtype.callbacks.push(cb);
      this.typeInformation.allCallbacks.push(cb);
    }
    // add the referenced types
    for (const rt in this.referencedTypes) {
      if (_.indexOf(this.typeInformation.referencedTypes, rt) < 0) {
        this.typeInformation.referencedTypes.push(rt);
      }
    }
    // Invert the inheritance tree
    for (const type of this.typeInformation.classes) {
      if (type.inherits) {
        const baseClass = this.resolveType(type.inherits);
        baseClass.subclasses = baseClass.subclasses || [];
        let found = false;
        for (const existing of baseClass.subclasses) {
          if (existing === type) {
            found = true;
            break;
          }
        }
        if (!found) {
          baseClass.subclasses.push(type);
        }
      }
    }
  }

  /**
   * Find the type information (from topLevel) for the given type name
   * @param name
   */
  resolveType(name) {
    const nameInfo = name.split('.');
    let type = this.typeInformation.rootTypes[nameInfo[0]];
    for (let i = 1; i < nameInfo.length; i++) {
      type = type.innerTypes[nameInfo[i]];
    }
    return type;
  }

  commentType(info) {
    const tags = info.comment.tags;

    if (!info.context && _.some(tags, (t) => t.title === 'event')) {
      const eventInfo = _.find(tags, (t) => t.title === 'event');
      info.context = { target: eventInfo.description.split('#')[0] };
    } else if (_.some(tags, (t) => t.title === 'callback')) {
      return 'callback';
    }

    // Private constructors matter for codegen, so let those through
    if (info.context && info.context.type === 'constructor') {
      return 'constructor';
    }

    // But all other privates do not (as do unattached comments)
    if (!info.context || _.some(tags, (t) => t.title === 'private')) {
      return null;
    }
    if (info.context.type === 'method') {
      return 'method';
    } else if (_.some(tags, (t) => t.title === 'class')) {
      return 'class';
    } else if (_.some(tags, (t) => t.title === 'enum')) {
      return 'enum';
    } else if (_.some(tags, (t) => t.title === 'event')) {
      return 'event';
    } else if (info.context.type === 'property') {
      return 'property';
    }
    console.log(util.inspect(info, { depth: null }));
    console.log(`Unknown comment type '${info.context.type}' in '${this.filename}' `,
      info.context.target, info.context.name, info.context);
    throw new Error(`Parsing seems to have failed on ${JSON.stringify(info)}`);
  }

  importEnum(info) {
    const enumInfo = {
      name: info.context.name,
      description: info.comment.description,
      filename: this.filename,
      location: info.context.location,
      values: [],
      enum: true,
      // TODO I'm not sure how to cleanly do string enums in objective-c, so for now, all enums
      // that cross the boundary must be numeric. This might be ok, not sure.
      type: _.find(info.comment.tags, (t) => t.title === 'enum').type.name,
    };
    this.typeInformation.allTypes.push(enumInfo);
    this.typeInformation.enums.push(enumInfo);
    if (info.context.target) {
      const t = this.resolveType(info.context.target);
      const c = info.context;
      assert(t,
        `Could not resolve type for enum '${c.target}.${c.name}' in file ${this.filename}.`
      );
      t.innerTypes[enumInfo.name] = enumInfo;
      enumInfo.packedName = t.packedName + enumInfo.name;
      enumInfo.dottedName = [t.dottedName, enumInfo.name].join('.');
    } else {
      this.typeInformation.rootTypes[enumInfo.name] = enumInfo;
      enumInfo.dottedName = enumInfo.packedName = enumInfo.name;
    }
  }

  /**
   * Helper function for importing things that have parameters and return values
   *  (i.e. methods, callbacks, and events).
   * @param info The full output from docchi
   * @param localInfo The info object we're importing. We'll add 'args' and
   *  'returns' properties here
   * @param nameOfThingBeingImported i.e. 'method', 'callback', etc. Used for logging
   * @param shouldNotErrorOnTagCallback If we see a tag with a title we don't recognize, we'll
   *  raise an error unless this callback returns true given the tag.
   */
  addParamsAndReturnsToInfoObject(info, localInfo,
                                  nameOfThingBeingImported, shouldNotErrorOnTagCallback) {
    _.defaults(localInfo, {
      args: [],
    });
    _.forEach(info.comment.tags, (t) => {
      // Array types coming from docchi don't have a type name, so we make up our own type name
      // by taking the first element in docchi's array and wrapping it with '[]'.
      // This means that we only support arrays that contain a single type of object, even though
      // docchi can parse more complex array types.
      let referencedTypeName = null;
      if (t.type && !t.type.name && t.type.type && t.type.type === 'ArrayType' && t.type.elements) {
        t.type.name = `[${t.type.elements[0].name}]`;
        referencedTypeName = t.type.elements[0].name;
      }

      if (t.title === 'param') {
        if (t.type && t.type.type === 'OptionalType') {
          localInfo.args.push({
            name: t.name,
            type: t.type.expression.name,
            description: t.description,
            optional: true,
          });
          return;
        }

        const missingTypeSpecifier = !(t.type && t.type.name);
        if (missingTypeSpecifier) {
          const name = info.context ? info.context.name : 'unknown';
          assert(false,
            `Parameter of ${nameOfThingBeingImported} is missing type specifier ${name}: ${t.name}`
          );
        }

        localInfo.args.push({
          name: t.name,
          type: t.type.name,
          description: t.description,
        });

        if (!referencedTypeName) {
          referencedTypeName = t.type.name;
        }
        this.referencedTypes[referencedTypeName] = true;
      } else if (t.title === 'returns') {
        localInfo.returns = {
          type: t.type.name,
          description: t.description,
        };
      } else if (t.title === 'protected') {
        localInfo.protected = true;
      } else if (t.title === 'async') {
        localInfo.async = true;
      } else if (shouldNotErrorOnTagCallback(t)) {
        // ¯\_(ツ)_/¯
      } else {
        console.log(JSON.stringify(info, null, '\t'));
        assert(false, `Unknown ${nameOfThingBeingImported} tag ${t.title}`);
      }
    });
    this.saveMethodCallbackInformation(info, localInfo, nameOfThingBeingImported);
  }


  saveMethodCallbackInformation(info, localInfo, nameOfThingBeingImported) {
    if (nameOfThingBeingImported === 'method' && this.commentType(info) !== 'callback'
      && this.commentType(info) !== 'constructor' && info.comment.tags.length > 0) {
      this.getCallbackInfoIfExists(info.comment.tags,
        (callbackArgIndex, callbackObject, callbackDtoType) => {
          if (callbackArgIndex >= 0) {
            // Adding the following four to method info which are bing used by
            // PayPal Android Business App (overridden) dust templates.
            localInfo.hasCallbackArg = true;
            localInfo.callbackMethodName = callbackObject.type.name.split('~', 2)[1];
            localInfo.callbackArgIndex = callbackArgIndex;
            localInfo.callbackDtoType = callbackDtoType;
          }
        });
    }
  }

  getCallbackInfoIfExists(methodParamList, callback) {
    methodParamList.some((methodParam, paramIndex) =>
      this.isCallbackPresent(methodParam, paramIndex, callback));
  }

  isCallbackPresent(methodParam, paramIndex, callback) {
    let foundIndex = -1;
    let foundElement;
    let callbackDtoName;
    this.pendingCallbacks.some((element) => {
      if (methodParam.type && element.name === methodParam.type.name) {
        foundIndex = paramIndex;
        foundElement = methodParam;
        const argLength = element.args.length;
        if (argLength > 1) {
          callbackDtoName = element.args[argLength - 1].type;
        }
        return true;
      }
      return false;
    });

    callback(foundIndex, foundElement, callbackDtoName);
  }

  importCallback(info) {
    const callbackInfo = {
      description: info.comment.description,
    };
    this.addParamsAndReturnsToInfoObject(info, callbackInfo, 'method', (t) => {
      if (t.title === 'callback') {
        callbackInfo.name = t.description;
        return true;
      }
      return false;
    });
    this.pendingCallbacks.push(callbackInfo);
  }

  importEvent(info) {
    const eventTag = _.find(info.comment.tags, (t) => t.title === 'event');
    const eventDetails = eventTag.description.split('#');
    if (eventDetails.length !== 2) {
      throw new Error('@event tag must have <class>#<eventName>');
    }
    const eventInfo = {
      name: eventDetails[1],
      description: info.comment.description,
    };
    const targetType = this.resolveType(eventDetails[0]);
    assert(targetType, `Could not resolve type ${info.context.target}`);
    targetType.events = targetType.events || [];
    eventInfo.containingType = targetType.name;
    this.addParamsAndReturnsToInfoObject(info, eventInfo, 'event',
      (t) => (t.title === 'event' || t.title === 'private'));
    targetType.events.push(eventInfo);
  }

  importMethod(info, isConstructor) {
    assert(info.context.target, 'Missing target for method.');
    const targetType = this.resolveType(info.context.target);
    assert(targetType, `Could not resolve type ${info.context.target}`);

    const methodInfo = {
      description: info.comment.description,
    };
    if (isConstructor) {
      if (_.some(info.comment.tags, (t) => t.title === 'private')) {
        methodInfo.private = true;
      }
      targetType.instanceConstructor = methodInfo;
    } else {
      if (info.context.static) {
        targetType.staticMethods.push(methodInfo);
      } else {
        targetType.methods.push(methodInfo);
      }
      methodInfo.name = info.context.name;
      if (methodInfo.name === 'toString') {
        targetType.hasToString = true;
      }
    }
    this.addParamsAndReturnsToInfoObject(info, methodInfo, 'method', (t) =>
      (t.title === 'method' || t.title === 'constructor' || t.title === 'private')
    );
  }

  /**
   * This is (at the moment) an enumeration value assignment
   */
  importProperty(info) {
    for (let ei = 0; ei < this.typeInformation.enums.length; ei++) {
      const theEnum = this.typeInformation.enums[ei];
      if (theEnum.filename === this.filename
        && theEnum.location.start.line <= info.context.location.start.line
        && info.context.location.end.line <= theEnum.location.end.line) {
        // That's us.
        theEnum.values.push({
          name: info.context.name,
          description: info.comment.description,
        });
        return;
      }
    }
    const infoStr = util.inspect(info, { depth: null });
    assert(false, `Unknown property usage ${infoStr}`);
  }

  importClass(info) {
    const classInfo = {
      name: info.context.name,
      filename: this.filename,
      description: info.comment.description,
      methods: [],
      staticMethods: [],
      fields: [],
      innerTypes: {},
    };
    for (const t of info.comment.tags) {
      switch (t.title) {
        case 'property':
          this.importField(classInfo, t);
          break;
        case 'class':
          break;
        case 'extends':
          classInfo.inherits = t.name;
          break;
        case 'protected':
          classInfo.protected = true;
          break;
        default:
          console.error(`Invalid tag ${t.title} in ${JSON.stringify(info, null, '\t')}`);
      }
    }
    this.typeInformation.allTypes.push(classInfo);
    this.typeInformation.classes.push(classInfo);
    if (info.context.target) {
      const t = this.resolveType(info.context.target);
      if (!t) {
        throw new Error(`Unable to resolve type from ${JSON.stringify(info.context, null, '\t')}`);
      }
      t.innerTypes[classInfo.name] = classInfo;
      classInfo.packedName = t.packedName + classInfo.name;
      classInfo.dottedName = [t.dottedName, classInfo.dottedName].join('.');
    } else {
      this.typeInformation.rootTypes[classInfo.name] = classInfo;
      classInfo.packedName = classInfo.name;
      classInfo.dottedName = classInfo.name;
    }
  }

  importField(classInfo, tag) {
    assert(tag.name, `Tags must have name in class ${classInfo.name}`);
    assert(tag.type && (tag.type.name || tag.type.type),
      `Field is missing type specifier ${classInfo.name}: #{tag.name}`);
    const fieldInfo = {
      name: tag.name,
      type: tag.type.name || tag.type.elements[0].name,
      description: tag.description,
    };
    this.referencedTypes[fieldInfo.type] = true;
    if (!tag.type.name) {
      fieldInfo.type = `[${fieldInfo.type}]`;
    }
    // TODO full regex parse of @ tags with ()
    if (tag.description && tag.description.indexOf('@readonly') >= 0) {
      fieldInfo.readonly = true;
    }
    classInfo.fields.push(fieldInfo);
  }
}
