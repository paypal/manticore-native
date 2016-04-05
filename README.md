paypal-manticore
================

[![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore)

A cross platform Javascript runtime environment with code generation

Native Class Generator
======================
This module takes Javascript objects and creates native interfaces in the target languages
(Objective-C, Java, and C#) that interact with the Manticore runtime and the Javascript engine on that platform.
Essentially each method and field are translated into calls that dive into the Javascript layer and convert types
back and forth. This is done via DustJS templates built per language. The generator script **tries** to
isolate these changes in the templates themselves, but there are still places where we have language specific
knowledge in the generator.

Generator configuration
=======================
Manticore needs some hints on how to map javascript types to the types in the native interfaces. The configuration
file typically lives alongside the templates, examples can be found in the built-in templates folders in the runtimes.
Typically you will customize these to change class name prefixes, namespaces, base classes, etc.

Language configuration
----------------------
* `namespace`: The namespace to use
* `typeMap`: A hash of javascript type to native type
* `arrayType`: TODO, it's a string but you can use `%s`. e.g. "List<%s>"
* `arrayIsTyped`: TODO boolean
* `callbackSuffix`: TODO e.g. "Delegate",
* `innerCallbacksAndEvents`: TODO boolean
* `typePrefix`: TODO string
* `baseClass`: TODO string
* `nativeConverters`: TODO, it's a hash but not sure how it's different than `typeMap`
* `basenames`: a hash of template name to a file basename
* `extensions`: a hash of template name to a filename extension (including the dot).
* `renames`: TODO: a hash, not sure how it works or why it's different than `typeMap`

Writing Manticore-Compatible Modules
====================================
One of the most promising aspects of Manticore is the ability to use node modules across client/server boundaries. For
example, if we have a module that implements both data models and service calls for an invoicing app, we want to be able to
use it on Win/Android/iOS/Xamarin/etc, and also in Node.js. We want "npm install invoicing" to work for
both cases. However, there are some rules and conventions that will make this possible/performant for modules that are complex:

1. Reduce your dependencies as much as possible. That doesn't mean re-implement things in your own modules, but it does
mean you should be intentional about dependencies. This includes built in node modules - e.g. no fs or process.
2. No native modules. If it has to compile something, it's not going to work in Manticore.
3. Use ES6. It's just nicer.
4. Use only supported "specials" like setTimeout, Promise and the fetch API. If something is missing - add it!

My Way or the Highway
=====================
Unfortunately, you do have to modify your JS style a bit to fit the parser-generator. So far that means:

* Comments are the only way to get your type exposed up, because that's all that docchi/JSDoc look at. This is
probably a feature not a limitation, but there it is.
* Be precise with things like boolean values exposed to native. In some languages (such as Java),
Boolean can be null and that's not the same as false. If you want an exposed JS property to be false, make
sure you set it to false. (Or we should change the type and generator handling in those languages)
* Don't make methods that take ambiguous parameters or return values if they're going to be exposed to
native. Not every type system is as completely insane as Javascripts.

Manticore vs React Native
=========================

Someone more familiar with React Native should send a PR to this description, but
the main points of differentiation in our mind are:

* Manticore runs "natively" on Windows XP+, Mac OS, iOS, and Android. Since it's Javascript,
the code you write that targets Manticore also runs in a browser, as a Chrome app,
in Node.js or in node-webkit. Manticore didn't get you those, but it does enable
the bulk of your non-UI code working on a bazillion platforms.
* Manticore does not deal with UI in any way. It is purely a computation layer.
* As a result, we are not super sensitive to performance. JS is what it is - if you get sloppy
it will get slow. If you're making a video game, Manticore probably isn't right for you.
* Manticore is a "smaller decision" about the way you write apps. You can still do
whatever fancy UI stuff you do to make the app look like a million bucks, but add Manticore
to make the boring network service layers, personalization, etc. better. Manticore was created
to provide common code for an SDK, where we cared about low overhead within reason, and we
didn't want to force a complete app rewrite to integrate our components.
* Manticore exposes your Javascript objects to native code. Generally React Native thinks
about the world the other way around.

We love React Native, and we strive to make code targeted for Manticore work in React Native
as well.

Platform Support
================

Platform | Version | CI Status | Distro | Version
-------- | ------- |------ | ------ | -------
iOS      | 7.0+ | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore) | CocoaPods | 1.0.0
MacOS    | 10.9+ | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore) | CocoaPods | 1.0.0
Android  | API 19+ | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore) | JCenter | 1.0.0
Windows  | XP (.Net4) | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
| NuGet | 1.0.0
Windows  | 7,8,Vista (v8) | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
| NuGet | 1.0.0
Windows  | 8.1, 8.1 Phone, 10 (jint) | [![Build status](https://ci.appveyor.com/api/projects/status/e67m7icv3538oo4n/branch/master?svg=true)](https://ci.appveyor.com/project/djMaxM/paypal-manticore/branch/master)
| NuGet | 1.0.0
ChromeOS/Chrome App | 32 | - | npm | 1.0.0
Node.js | 0.12 | [![Build Status](https://travis-ci.com/paypal/paypal-manticore.svg?token=zgKPydNJ3A6t49VkNfPv&branch=master)](https://travis-ci.com/paypal/paypal-manticore) | npm | 1.0.0
