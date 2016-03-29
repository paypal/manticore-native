#!/bin/bash

function osxSetup {
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
}

if [ "$BUILD_ITEM" == "ios" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING ios =*=*=*=*=*=*=*=*=*=*=*=*"
  osxSetup
  cd runtime/objc
  instruments -s devices
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=9.2' | tee xcodebuild9.log | xcpretty
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-iOS -destination 'platform=iOS Simulator,name=iPhone 6,OS=8.1' | tee xcodebuild8.log | xcpretty
elif [ "$BUILD_ITEM" == "osx" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING osx =*=*=*=*=*=*=*=*=*=*=*=*"
  osxSetup
  cd runtime/objc
  instruments -s devices
  xcodebuild test -workspace Manticore.xcworkspace -scheme ManticoreContainer-OSX | tee xcodebuild.log | xcpretty
elif [ "$BUILD_ITEM" == "node" ]
then
  echo "=*=*=*=*=*=*=*=*=*=*=*=* BUILDING node =*=*=*=*=*=*=*=*=*=*=*=*"
  node -v
  npm -v
  npm install -g npm@3
  npm install
  npm run lint
  npm test
else
  echo "=*=*=*=*=*=*=*=*=*=*=*=* MISSING BUILD_ITEM env var =*=*=*=*=*=*=*=*=*=*=*=*"
fi