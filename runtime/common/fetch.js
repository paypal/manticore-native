// Inspired by
// https://github.com/bitinn/node-fetch
import Request from './fetch/Request';
import Response from './fetch/Response';

global.fetch = function fetcher(url, options) {
  return new Promise((accept, reject) => {
    const rq = new Request(url, options);
    global.manticore._fetch(rq,
      (err, native) => (err ? reject(err) : accept(new Response(native, rq))));
  });
};
