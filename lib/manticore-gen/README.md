Manticore Generator
===================
The manticore generator produces native code stubs in a target output language.

Usage
-----

The executable `manticore-gen` should be available as a result of installing this module.

You can run that executable, or `node index.js` in this directory.

> Parallel execution of manticore-gen `generate` jobs for different output languages is not supported due to apparent limitations of the dust library.



### Command line

Until we flesh out this section, just run `manticore-gen` with no arguments; it will show some helpful text.

```
$ manticore-gen

 Usage:
   manticore-gen [options] <template_config_dir> <outputDirectory> <inputFile1 [inputFile2 [...]] >

 Example:
   manticore-gen --test ./templates native-src/generated/somewhere/ src/input*.js

 Supported options:
   -q                     Don't log stuff, just do your job
   -s                     Suppress generated code comments that imply the use of Javascript
   --print                Print the files before writing them
   --test                 Don't write files, just say which ones you would write
   --glob                 Execute file glob (defaults to true for win32)
   --base=<path>          Use path as a "base configuration" with templates and config.json
```


### Programmatically

See [test/test_gen.js](../test/test_gen.js) for examples.


Configuration of Generated Languages: `config.json`
---------------------------------------------------

The configuration file (for the time being) must be named `config.json`, and must reside in the templates directory
 passed to `--base`; examples can be found in the built-in templates folders in the runtimes.

The Manticore generator needs some hints on how to map javascript types to the types in the native interfaces.
The following parameters affect the generated code:

* `namespace` (`string`): The namespace to use
* `typeMap` (`hash`): Hints -- JavaScript type to native type
* `arrayType` (`string`): The name of the native array type to use; it's a string but you can use `%s` (e.g. "List<%s>")
* `arrayIsTyped` (`bool`): TODO
* `callbackSuffix` (`string`): Text to apply to the names of callbacks that were defined in JavaScript (e.g. "Delegate")
* `innerCallbacksAndEvents` (`bool`): Controls capitalization of variables, TODO
* `hasPointers` (`bool`): Whether to prefix native object variables with `*`.
* `typePrefix` (`string`): When JavaScript types are exposed to native, they will be given this prefix
* `baseClass` (`string`): The name of the type from which all native-exposed JavaScript objects will inherit.  This must be (or inherit) the proper object in the Manticore runtime for this language.
* `nativeConverters` (`hash`): TODO, it's a hash but not sure how it's different than `typeMap`
* `basenames` (`hash`): TODO: template name to a file basename.
* `extensions` (`hash`): TODO: template name to a filename extension (including the dot).
* `renames` (`hash`): TODO: not sure how it works or why it's different than `typeMap`


Templates
---------

Manticore uses dust.js to render its output into code files per language.  The following template file names follow a specific naming convention and purpose; they are known entry points for template generation.

* `header`: Renders a header for all types merged into a single output file
* `impl`: Renders all types merged into a single output file
* `enums`: Renders all enums into a single output file
* `typedefs`: Renders all typedefs into a single output file
* `classHeader`: Renders the header of an individual class
* `class`: Renders the definition of an individual class
* `enum`: Renders the definition of an individual enum
* `importAll`: Renders a reference to all class header files into a single output file


Caveats
=======

### Writing Manticore-Compatible Code

Unfortunately, you do have to modify your JS style a bit to fit the parser-generator. So far that means:

* Comments are the only way to get your type exposed up, because that's all that docchi/JSDoc look at. This is
probably a feature not a limitation, but there it is.
* Be precise with things like boolean values exposed to native. In some languages (such as Java),
Boolean can be null and that's not the same as false. If you want an exposed JS property to be false, make
sure you set it to false. (Or we should change the type and generator handling in those languages)
* Don't make methods that take ambiguous parameters or return values if they're going to be exposed to
native. Not every type system is as completely insane as JavaScript's.



### Writing Manticore-Compatible Modules

One of the most promising aspects of Manticore is the ability to use node modules across client/server boundaries. For
example, if we have a module that implements both data models and service calls for an invoicing app, we want to be able to
use it on Win/Android/iOS/Xamarin/etc, and also in Node.js. We want "npm install invoicing" to work for
both cases. However, there are some rules and conventions that will make this possible/performant for modules that are complex:

1. Reduce your dependencies as much as possible. That doesn't mean re-implement things in your own modules, but it does
mean you should be intentional about dependencies. This includes built in node modules - e.g. no `fs` or `process`.
2. No native modules. **If it has to compile something, it's not going to work in Manticore.** Instead, manticore has
the ability to plugin native methods at runtime and expose them to JS. For example, this is how the fetch API is implemented
because the JS engines don't have XMLHttpRequest inherently (that's a browser thing not a JS thing).
3. Use ES6. It's just nicer. All our tooling is built around babel, and async/await is the bee's knees.
4. Use only supported "specials" like setTimeout, Promise and the fetch API. If something is missing - add it via a plugin.
