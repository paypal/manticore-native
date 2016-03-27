import tape from 'tape';

export function test(name, fn) {
  return tape(name, fn);
}

export function wrapTest(fn) {
  return (done) => {
    const result = fn(done);
    if (result instanceof Promise) {
      result.catch((err) => {
        done(err);
      });
    }
  };
}