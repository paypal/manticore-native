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

  s.dependency 'ManticoreNative', '~> 1.0.0'

  s.libraries = 'c++', 'stdc++', 'z'
end
