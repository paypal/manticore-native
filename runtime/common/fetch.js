// Inspired by
// https://github.com/bitinn/node-fetch
import Request from './fetch/Request';
import Response from './fetch/Response';

global.fetch = function fetcher(url, options) {
  return new Promise((accept, reject) => (
    global.manticore.http(new Request(url, options),
      (err, native) => {
        if (err) {
          reject(err);
        } else {
          accept(new Response(native));
        }
      })));
};
