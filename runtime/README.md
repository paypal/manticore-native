Manticore Native Runtimes
=========================

Manticore's native runtime libraries handle the boilerplate code required to move data into and out of a JavaScript engine.

Additionally, they provide *polyfill* -- ensuring that the "standard" functions one would expect in a JavaScript engine
are actually present.  The necessary functionality is built on the native side, then exposed to the JavaScript side by
a set of polyfill files.


Available Runtimes
------------------

* [C#](win/) - Windows and Windows Mobile (Jint and ClearScript engines)
* [Java](android/) - Android
* [Objective-C](objc/) - iOS and MacOS
