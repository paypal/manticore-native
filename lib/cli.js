import path from 'path';

// get a decent approximation of the command that was typed in
export function determineCommand(argv) {
  const nodeCmd = path.basename(argv[0]);
  const script = path.basename(argv[1]);

  if (path.extname(script) === '') {
    return path.basename(argv[1]);  // something like 'manticore'
  }
  return `${nodeCmd} ${script}`;  // something like 'node index.js'
}
