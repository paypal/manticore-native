
Manticore vs React Native
=========================

Someone more familiar with React Native should send a PR to this description, but
the main points of differentiation in our mind are:

* **Wider Platform Targets** - Manticore runs "natively" on Windows XP+, Mac OS, iOS, and Android. Since it's Javascript,
your code will run natively (without manticore wrappers) in a browser, as a Chrome app,
in Node.js or in node-webkit.
* **No UI** - Manticore does not deal with UI in any way. It is purely a computation and business logic layer. We
don't have to deal with all the differences in UI philosophies among the platforms. There aren't too many different
opinions on for loops and async/await, etc. So it truly is write once run anywhere for Manticore JS code.
* As a result of not being UI-focused, we are not super sensitive to performance. JS is what it is - if you get sloppy
it will get slow. If you're making a video game or a cryptographic toolkit, Manticore probably isn't right for you
(unless of course all the UI and number-crunchery is a Manticore plugin)
* **Lighter Weight** - Manticore is a "smaller decision" about the way you write apps. You can still do
whatever fancy UI stuff you do to make the app look like a million bucks, but add Manticore
to make the boring network service layers, personalization, etc. better. Manticore was created
to provide common code for an SDK, where we cared about low overhead within reason, and we
didn't want to force a complete app rewrite to integrate our components.
* Manticore exposes your Javascript objects to native code. Generally React Native is UI-focused
and up-front - meaning you KNOW React is involved. In theory Manticore users don't even know
that the logic "down there" is in Javascript.

We love React Native, and we strive to make code targeted for Manticore work in React Native
as well. We started this project before React Native existed (and since we target Windows, Chrome, etc)
it's not an option for us. But there are probably a small set of changes to React Native on iOS
and Android which would allow it to serve as an "output platform" for Manticore JS code, and we
continue to explore that avenue.


What we have in common:
-----------------------

* Babel
* ES6 + async/await
* require module system
* fetch support


Manticore vs Apache Cordova / PhoneGap
======================================

Cordova is a UI-centric system for writing cross-platform apps -- your entire app must be written in HTML, CSS
and JavaScript, your code runs in a web browser, and "plugins" are necessary to access native components.

Manticore is the opposite of UI-centric; it can't touch the UI at all -- your entire app can be written in the
native language for the platform, your Manticore code runs in a javascript engine, and no plugins are required.

That said, since Manticore doesn't deal with UI and runs as JavaScript, it should be possible for Manticore
JavaScript to run in Cordova as a plugin. However, no testing has been done in that area and we have no data either way.