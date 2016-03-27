Manticore Generator
===================
The manticore generator produces native code stubs in a target output language,

Usage
-----
You can run the `manticore` executable, or `node index.js` in the manticore-gen directory.

> Parallel execution of manticore-gen `generate` jobs for different output languages is not supported due to apparent limitations of the dust library.

### Command line
Until we flesh out this section, just run manticore with no arguments; it will show some helpful text.

### Programmatically
See [test/genTest.js](test/genTest.js) for examples.


Configuration
-------------
See [config/README.md](config/README.md).

Templates
---------
See [templates/README.md](templates/README.md).


Testing
-------
Execute tests with `npm test`.

A directory of expected generated outputs is stored within the test directory; to update the expected outputs, run `npm run accept-expected-outputs`.