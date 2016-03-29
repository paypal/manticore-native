npm install
npm run build-test-js
npm run build-objc-polyfill
npm run gen-objc-test
cd runtime/objc
xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=9.2' | tee xcodebuild.log | xcpretty