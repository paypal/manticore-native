#!/usr/bin/env sh
set -ex
npm install --production
./node_modules/.bin/manticore-gen node_modules/manticore-native/runtime/objc/templates generated/src node_modules/hello-world/*.js
./node_modules/.bin/mantify generated/manticore_modules.js node_modules/hello-world/*.js
pod install
