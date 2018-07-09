Pod::Spec.new do |s|
  s.name             = "ManticoreGenerated"
  s.version          = "0.0.1"
  s.authors          = "PayPal Boston iOS Team"
  s.license          = "Apache 2.0"
  s.homepage         = "https://github.com/paypal/manticore-native/tree/master/examples/hello-world/src/ios"
  s.source           = { :path => './' }
  s.summary          = "Shortcut to including all Manticore generated files"

  s.requires_arc = true

  s.ios.frameworks = [
    'Foundation'
  ]

  s.ios.source_files = ['**/*.{h,m}']
  s.ios.public_header_files = ['**/*.h']

  s.xcconfig = {
    'OTHER_LDFLAGS' => '-weak_library /usr/lib/libstdc++.dylib',
  }

  s.dependency 'ManticoreNative', '~> 1.0.0'

  s.libraries = 'c++', 'stdc++', 'z'
end
