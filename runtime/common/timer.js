const g = global;
const m = g.manticore;

// Some VMs do not like argument count mismatch, so we fix it up here.
g.setTimeout = g.setTimeout || ((fn, t) => m._setTimeout(fn, t || 0));
