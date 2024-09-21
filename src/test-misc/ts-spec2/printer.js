"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.trace_guard = trace_guard;
exports.trace = trace;
function trace_guard(msg, condition) {
    var toAppend = "";
    if (condition) {
        toAppend = "Behavior running.";
    }
    else {
        toAppend = "Behavior skipped.";
    }
    trace(msg + " " + toAppend);
    return condition;
}
function trace(msg) {
    console.log(msg);
}
//# sourceMappingURL=printer.js.map