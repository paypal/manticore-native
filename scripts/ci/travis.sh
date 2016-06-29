#!/bin/bash
: ${BUILD_ITEM:=$1}  # allow "travis.sh objc" shorthand instead of "BUILD_ITEM=objc travis.sh"
set -o pipefail


echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING $BUILD_ITEM =*=*=*=*=*=*=*=*=*=*=*=*"
if [ "$BUILD_ITEM" == "objc" ]
then
  set -ex
  node -v
  npm -v
  npm install -g npm@3
  npm install
  npm run postinstall
  npm run build-testjs
  npm run objc-polyfill
  npm run objc-testjs

  # now the test harness
  pushd runtime/objc
  pod install
  instruments -s devices
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-OSX | tee xcodebuild-osx.log | xcpretty
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=9.3' | tee xcodebuild9.log | xcpretty
  popd

  # now the hello world app
  pushd examples/hello-world/src/ios/
  ./build.sh
  popd

elif [ "$BUILD_ITEM" == "node" ]
then
  set -ex
  npm run lint
  npm test
elif [ "$BUILD_ITEM" == "java" ]
then
  set -ex
  . /home/travis/.nvm/nvm.sh
  nvm install 4.4
  npm install -g npm@3
  npm install
  npm run postinstall
  npm run build-testjs
  npm run android-polyfill
  npm run android-testjs
  find . -name polyfill_pack.js
  cd runtime/android
  ./gradlew --stacktrace --info clean :manticore:generateDebugSources :manticore:mockableAndroidJar :manticore:prepareDebugUnitTestDependencies :manticore:generateDebugAndroidTestSources testDebug
else
  echo "The environment variable BUILD_ITEM contained the unrecognized value '$BUILD_ITEM' (expected {objc, java, node})"
  exit 1
fi
