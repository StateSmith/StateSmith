#!/usr/bin/env node
"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.trace_guard = trace_guard;
exports.trace = trace;
var Spec2Sm = __importStar(require("./Spec2Sm"));
// ported from C spec test
/** @type {string[]} */
var args = process.argv.slice(2); // skip past first two which are node exe and file being run
var sm = new Spec2Sm.Spec2Sm();
print_start();
sm.start();
console.log();
args.forEach(function (eventArg) {
    print_dispatch_event_name(eventArg);
    sm.dispatchEvent(Spec2Sm.EventId[eventArg.toUpperCase()]);
    console.log();
});
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