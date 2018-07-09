
Adding Manticore-Native to an Android Project
=============================================

This guide accompanies the example android project found in [examples/hello-world/](../../examples/hello-world/src/android/).

### 0. The Existing Project

```
$ ls                              # in Android source directory
```

```
$ ls                              # in JavaScript source directory
Hello.js
index.js
```

### 1. Install Manticore-Native Components

```
$ npm init
$ npm install --save manticore-native
```
This provides the necessary executables and templates for code generation.


### 2. Generate Native Bindings for JavaScript Code

In this example, the code we will expose to native comes from a local npm module called `hello-world`.

```
$ mkdir -p generated/src
$ ./node_modules/.bin/manticore-gen \
    node_modules/manticore-native/runtime/objc/templates \
    AndroidManticoreHelloWorld/app/src/main/java/com/paypal/manticore \
    node_modules/hello-world/*.js
```

This creates native bindings for the JavaScript code shared between platforms.


### 3. Add Generated Files to the Project

TODO


### 4. Add Manticore-Native Runtimes to the Project

TODO, gradle stuff


### 5. Run gradle? TODO

TODO


### 6. Prepare the JavaScript Code for Native

```
$ ./node_modules/.bin/mantify AndroidManticoreHelloWorld/app/src/main/res/raw/manticore_modules.js node_modules/hello-world/*.js
```

This consolidates all JavaScript code and dependencies into a single file called `manticore_modules.js`.


### 7. Instantiate Manticore-Native in Native Code

```java

// TODO

```

Note the reference to the `manticore_modules.js` file that we mantify'd in the previous step.


Developing With Manticore-Native
================================

TODO: more here

* If you make changes to JavaScript, you must re-run `mantify` to bring your changes into the app bundle.
* You may need to delete relevant directores from `node_modules` so that `npm install` will properly update them after code changes
* If all else fails, the output of `mantify` is readable and can be edited to aid in debugging.
