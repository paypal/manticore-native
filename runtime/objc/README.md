
Adding Manticore-Native to an Objective-C Project
=================================================

This guide accompanies the example iOS project found in `examples/hello-world/`

### 0. The Existing Project

```
$ ls                              # in Objective-C source directory
Hello Manticore
Hello Manticore.xcworkspace
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
    generated/src \
    node_modules/hello-world/*.js
```

This creates native bindings for the JavaScript code shared between platforms.


### 3. Add Generated Files to the Project

You could do this manually in Xcode, but the simplest way is to wrap it into its own CocoaPod file; any new files will automagically appear in the Xcode project.  First, create a CocoaPod for the generated files:

```
$ cat gen/ManticoreGenerated.podspec
Pod::Spec.new do |s|
  s.name             = "ManticoreGenerated"
  s.version          = "0.0.1"

  s.requires_arc = true

  s.ios.frameworks = [
    'Foundation'
  ]

  s.ios.source_files = ['**/*.{h,m}']
  s.ios.public_header_files = ['**/*.h']

  s.xcconfig = {
    'OTHER_LDFLAGS' => '-weak_library /usr/lib/libstdc++.dylib',
  }

  s.dependency 'Manticore', '~> 0.0.1'

  s.libraries = 'c++', 'stdc++', 'z'
end
```

Next, add that podspec to the local `Podfile`.

```
$ cat Podfile
xcodeproj 'Hello Manticore/Hello Manticore.xcodeproj'

pod 'ManticoreGenerated', :path => 'generated/', :inhibit_warnings => true
```

### 4. Add Manticore-Native Runtimes to the Project

Just another line in the local `Podfile`.

$ cat Podfile

```
xcodeproj 'Hello Manticore/Hello Manticore.xcodeproj'

pod 'ManticoreGenerated', :path => 'generated/', :inhibit_warnings => true
pod 'Manticore', :path => '../../runtime/objc', :inhibit_warnings => true
```

> TODO: Instructions for linking against the repository version of manticore-native.  Generating the polyfill and whatnot.

### 5. Install CocoaPods

```
$ pod install
```

This adds the manticore-native libraries and javascript wrappers to the project.


### 6. Prepare the JavaScript Code for Native

```
$ ./node_modules/.bin/mantify generated/manticore_modules.js node_modules/hello-world/*.js
```

This consolidates all JavaScript code and dependencies into a single file called `manticore_modules.js`.


### 7. Instantiate Manticore-Native in Native Code

```objectivec
#import <ManticoreNative/PPManticoreEngine.h>
#import "PPManticoreJSBackedObject.h"

[PPManticoreJSBackedObject setManticoreEngine:[PPManticoreEngine new]];
NSString *path = [[NSBundle mainBundle] pathForResource:@"manticore_modules" ofType:@"js"];
[[PPManticoreJSBackedObject engine] loadScript:[NSString stringWithContentsOfFile:path encoding:NSUTF8StringEncoding error:NULL] withName:nil];
```


Developing With Manticore-Native
================================

TODO: more here

* If you make changes to JavaScript, you must re-run `mantify` to bring your changes into the app bundle.
* You may need to delete relevant directores from `node_modules` so that `npm install` will properly update them after code changes
* If all else fails, the output of `mantify` is readable and can be edited to aid in debugging.
