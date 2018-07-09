Hello Manticore in iOS
======================

Prerequisite: Configure Local Dependencies
------------------------------------------

This example has module dependencies on the JavaScript `manticore-native` NPM module, as well as the CocoaPod -- the local
versions of both.  This means you must prepare those modules locally before you can successfully continue.

In the root of the `manticore-native` repository, run:

* `npm install`
* `npm run postinstall`
* `npm run objc-polyfill`

This will flesh out both the node module and the CocoaPod.


Building the Example
--------------------

To build, run the `./build.sh` script.  If you experience a runtime failure on launch, double check the prerequisites above.


See Also
--------

* For more information on integrating manticore into Objective-C, see the [/runtime/objc/README.md](../../../../runtime/objc/README.md) file.