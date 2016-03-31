#!/bin/bash

if [ "$BUILD_ITEM" == "mac" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING mac =*=*=*=*=*=*=*=*=*=*=*=*"
  node -v
  npm -v
  brew update
  brew unlink node
  brew install homebrew/versions/node4-lts
  npm install -g npm@3
  npm install
  npm run build-test-js
  npm run build-objc-polyfill
  npm run gen-objc-test
  cd runtime/objc
#  instruments -s devices
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-OSX | tee xcodebuild-osx.log | xcpretty
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=9.3' | tee xcodebuild9.log | xcpretty
elif [ "$BUILD_ITEM" == "node" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING node =*=*=*=*=*=*=*=*=*=*=*=*"
  npm run lint
  npm test
elif [ "$BUILD_ITEM" == "android" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING android =*=*=*=*=*=*=*=*=*=*=*=*"
  curl -o- https://raw.githubusercontent.com/creationix/nvm/v0.31.0/install.sh | bash
  nvm install 4.4
  npm install -g npm@3
  npm install
  npm run build-test-js
  npm run build-android-polyfill
  npm run gen-android-test
  cd runtime/android
  ./gradlew testDebug --stacktrace --info
else
  echo "=*=*=*=*=*=*=*=*=*=*=*=* MISSING BUILD_ITEM env var =*=*=*=*=*=*=*=*=*=*=*=*"
fi
