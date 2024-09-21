#!/usr/bin/env node
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.trace_guard = trace_guard;
exports.trace = trace;
var Spec2Sm_1 = require("./Spec2Sm");
// ported from C spec test
/** @type {string[]} */
var args = process.argv.slice(2); // skip past first two which are node exe and file being run
var sm = new Spec2Sm_1.Spec2Sm();
print_start();
// sm.start();
// console.log();
// args.forEach(eventArg => {
//     print_dispatch_event_name(eventArg);
//     sm.dispatchEvent(Spec2Sm.EventId[eventArg.toUpperCase()]);
//     console.log();
// });
function print_divider() {
    console.log("===================================================");
}
function print_start() {
    console.log("Start Statemachine");
    print_divider();
}
function print_dispatch_event_name(event_name) {
    console.log("Dispatch event ".concat(event_name));
    print_divider();
}
function trace_guard(msg, condition) {
    trace(msg);
    return condition;
}
function trace(msg) {
    console.log(msg);
}
//# sourceMappingURL=index.js.map