#!/bin/bash

if [ "$BUILD_ITEM" == "objc" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING objc =*=*=*=*=*=*=*=*=*=*=*=*"
  node -v
  npm -v
  brew update
  brew unlink node
  brew install homebrew/versions/node4-lts
  npm install -g npm@3
  npm install
  npm run build-testjs
  npm run objc-polyfill
  npm run objc-testjs
  cd runtime/objc
#  instruments -s devices
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-OSX | tee xcodebuild-osx.log | xcpretty
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=9.3' | tee xcodebuild9.log | xcpretty
elif [ "$BUILD_ITEM" == "node" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING node =*=*=*=*=*=*=*=*=*=*=*=*"
  npm run lint
  npm test
elif [ "$BUILD_ITEM" == "java" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING java =*=*=*=*=*=*=*=*=*=*=*=*"
  . /home/travis/.nvm/nvm.sh
  nvm install 4.4
  npm install -g npm@3
  npm install
  npm run build-testjs
  npm run android-polyfill
  npm run android-testjs
  find . -name polyfill_pack.js
  cd runtime/android
  ./gradlew --stacktrace --info clean :manticore:generateDebugSources :manticore:mockableAndroidJar :manticore:prepareDebugUnitTestDependencies :manticore:generateDebugAndroidTestSources testDebug
else
  echo "=*=*=*=*=*=*=*=*=*=*=*=* MISSING BUILD_ITEM env var =*=*=*=*=*=*=*=*=*=*=*=*"
  echo "The environment variable BUILD_ITEM contained the unrecognized value '$BUILD_ITEM'"
  exit 1
fi
