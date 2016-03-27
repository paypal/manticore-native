
global.fetch = function fetcher(url, options) {
  return new Promise((accept, reject) => {
    manticore.http(Object.assign({
      url,
      method: 'GET',
    }, options), (err, response) => {
      return err ? reject(err) : accept(response);
    });
  });
};