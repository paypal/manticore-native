#!/usr/bin/env sh
JS_SRC=node_modules/hello-world/*.js
set -ex
bundle install
npm install --production
./node_modules/.bin/manticore-gen node_modules/manticore-native/runtime/objc/templates generated/src $JS_SRC
./node_modules/.bin/mantify generated/manticore_modules.js $JS_SRC
pod install
xcodebuild -workspace "Hello Manticore.xcworkspace" -list
xcodebuild clean build -workspace "Hello Manticore.xcworkspace" -scheme "Hello Manticore" -sdk iphonesimulator
