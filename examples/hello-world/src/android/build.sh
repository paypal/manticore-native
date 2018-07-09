#!/usr/bin/env sh
JS_SRC=node_modules/hello-world/*.js
set -ex
npm install --production
./node_modules/.bin/manticore-gen node_modules/manticore-native/runtime/android/templates AndroidManticoreHelloWorld/app/src/main/java/com/paypal/manticore/ $JS_SRC
./node_modules/.bin/mantify AndroidManticoreHelloWorld/app/src/main/res/raw/manticore_modules.js $JS_SRC
cd AndroidManticoreHelloWorld
./gradlew --info --stacktrace assembleDebug
