Initializing Manticore in Your App
==================================

For a practical example, see the HelloWorld apps in [`examples/`](examples/).


Adding Manticore Files to Your Project
--------------------------------------

### Objective-C and Xcode

Here is an example of bringing generated stubs into a project by using a CocoaPod.
We create a podspec to encapsulate all the generated files, and refer to it from the `Podfile`
in the root of the project.

Note the dependency on the ManticoreNative runtime CocoaPod.

```ruby
Pod::Spec.new do |s|
  s.name             = "ManticoreGenerated"
  s.version          = "0.0.1"

  s.requires_arc = true

  s.ios.frameworks = [
    'Foundation'
  ]

  s.ios.source_files = ['**/*.{h,m}']    # your generated files
  s.ios.public_header_files = ['**/*.h'] # your generated files

  s.xcconfig = {
    'OTHER_LDFLAGS' => '-weak_library /usr/lib/libstdc++.dylib',
    'ENABLE_BITCODE' => false
  }

  s.dependency 'ManticoreNative'

  s.libraries = 'c++', 'stdc++', 'z'
end

```


### Java

TODO: Abrar


### C&#35;

TODO: Max


Adding Manticore Features to Your Application
------------------------------------------

In short, there are 3 steps.  The first step is to initialize the Manticore engine, load the browserified JavaScript into that engine,
and load the engine into the class that will act as the base class for all incoming and outgoing Manticore objects.

### Objective-C and Xcode

TODO: Boris


### Java

TODO: Abrar


### C&#35;

TODO: Max