Manticore
=========

A cross platform Javascript runtime environment, built specifically for native mobile development and enabled by code generation.

* Bidirectional invocation - JavaScript calling native callbacks and vice versa
* True object portability across the native boundary - not just primitive types, and no "frozen" types requiring synchronization
* Asynchronous Javascript methods (e.g. Promises) exposed in native-friendly ways
* The ability to completely hide the fact that it's "Javascript all the way down" from your code consumers


```
npm install manticore-native
```

### Code Portability

If you wrote a single library in JavaScript, here are all the platforms where Manticore could help you compile it into native code:

Platform | Version                   | Native | Distro    | Version | CI Status |
-------- | -------                   | ------ |  ------   | ------- |:------    |
iOS      | 7.0+                      | Obj C  | CocoaPods | 1.0.0   | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore)
MacOS    | 10.9+                     | Obj C  | CocoaPods | 1.0.0   | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore)
Android  | API 19+                   | Java   | JCenter   | 1.0.0   | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore)
Windows  | XP (.Net4)                | C#     | NuGet     | 1.0.0   | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
Windows  | 7,8,Vista (v8  )          | C#     | NuGet     | 1.0.0   | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
Windows  | 8.1, 8.1 Phone, 10 (jint) | C#     | NuGet     | 1.0.0   | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
ChromeOS | 32                        | JS     | npm       | 1.0.0   |
Node.js  | 0.12                      | JS     | npm       | 1.0.0   | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore)


How It Works
============

### Pre-Compilation: Generating Native Classes

The `manticore-gen` script generates native interface shims in a target language
(Objective-C, Java, and C# currently) -- code to interact with the proper Manticore runtime and Javascript engine for that
platform.  The generator reads class and type information from the JSDoc comments in the JavaScript, stores it as JSON, and
uses DustJS templates to render the native code.

As such, there is a different template for each language, which you can customize or replace.

For more information, see the [README in /gen](gen/README.md).


### Pre-Compilation: Generating Runtime JavaScript

Manticore provides scripts to combine all the source JavaScript code (and its dependencies) into one large file,
using (among other things) `browserify` and `folderify`.

For more information, see [Mantify.md](Mantify.md).


### Initialization: Launching Manticore-Powered Code

Manticore launches a native JavaScript environment and loads the unified JavaScript file into it.

For more information, see [Initialization.md](Initialization.md).


### Runtime: Crossing the Boundary

The Manticore runtime provides the boilerplate code that's used for each method and field of the underlying JavaScript:
native calls that dive into the JavaScript layer and convert types
back and forth. For primitive types this conversion is "simple." For other Manticore or non-native types, the objects
returned are still the same thin shims over JavaScript -- where all calls to properties and methods will be sent. This means
there is no "synchronization" of values or events or anything - **there is only one truth, and it lives in the JavaScript environment**.




# ISSUES

there are still places where we have language specific knowledge in the generator (but it should be factored out).

DustJS seemed great at the time we wrote this whole thing, but we don't really love it anymore. There's probably a better templating language for a case like this where whitespace matters (for example) - so you should suggest it to us.

`basenames` and `extensions` need to be updated

use varname library instead of the really weird .toUpperCase in the dust helpers

specify templates dir SEPARATELY from config.json, and let config.json be anything

continue to work on docs for adding manticore to project

add hello world example file

fix objc stack trace to pull lines from input js



License
=======

Manticore (`manticore-native`) is available under the Apache 2.0 License.  See [LICENSE.txt](LICENSE.txt).


Contributing
============

See [CONTRIBUTING.md](CONTRIBUTING.md).


Isn't There a Project That Already Does This?
=============================================

If you're referring to [Apache Cordova](https://cordova.apache.org/), [Adobe PhoneGap](http://phonegap.com/),
[React Native](https://facebook.github.io/react-native/), or [SWIG](http://www.swig.org/), see [Comparisons.md](Comparisons.md),
summarized in this table:

|                                  |Cordova / PhoneGap | React Native |   SWIG  |  Manticore |
|:-------------------------------  |:-----------------:|:------------:|:-------:|:----------:|
|TL;DR                             |Webviews everywhere | Learn once, run "anywhere" | C++ wrappers everywhere| JS wrappers everywhere |
|You write common code in          |HTML / CSS / JS | JSX / JS | C++ | JS / ES6 |
|Hardware access provided by       |Plugins | Plugins | (Native) | (Native) |
|`main()`defined in                |JS | JSX / JS | (Native) | (Native) |
|UI managed by                     |Cordova library | OS-specific modules | (Native) | (Native) |
|Runs on iOS                       | Yes | Yes |     | Yes |
|Runs on MacOS                     |     |     | Yes | Yes |
|Runs on Android                   | Yes | Yes | Yes | Yes |
|Runs on Windows Phone             | Yes |     | Yes | Yes |
|Runs on Windows XP and up         |     |     | Yes | Yes |
|Runs in web browsers              | Yes |     |     | Yes |
|Runs on Windows Phone (8.1)       |     |     | Yes | Yes |
|Runs on ChromeOS / Chrome Apps    |     |     |  ?  | Yes |
|Runs in Node.js                   |     |     | Yes | Yes |
|Compatible with Cordova / PhoneGap| -   | No  | ?   | Yes |
|Compatible with React Native      | No  | -   | ?   | Yes |


Help
====

Where-to-post summary:

* How do I? -- [StackExchange](http://stackoverflow.com/questions/ask?tags=manticore-native)
* I got this error, why? -- [StackExchange](http://stackoverflow.com/questions/ask?tags=manticore-native)
* I got this error and I'm sure it's a bug -- [file an issue](https://github.com/paypal/manticore-native/issues)
* I have an idea/request -- [file an issue](https://github.com/paypal/manticore-native/issues)
