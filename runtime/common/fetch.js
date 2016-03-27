global.fetch = function fetcher(url, options) {
  return new Promise((accept, reject) => (
    global.manticore.http(Object.assign({
      url,
      method: 'GET',
    }, options), (err, response) => (err ? reject(err) : accept(response)))
  ));
};
