#!/usr/bin/env node
import * as Spec2Sm from "./Spec2Sm";

// ported from C spec test

/** @type {string[]} */
const args = process.argv.slice(2); // skip past first two which are node exe and file being run

let sm = new Spec2Sm.Spec2Sm();
print_start()
sm.start();
console.log();

args.forEach(eventArg => {
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

function print_dispatch_event_name(event_name: string) {
    console.log(`Dispatch event ${event_name}`);
    print_divider();
}

export function trace_guard(msg: string, condition: boolean) {
    trace(msg);
    return condition;
}

export function trace(msg: string) {
    console.log(msg);
}