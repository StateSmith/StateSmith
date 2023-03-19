export function trace_guard(msg, condition) {
    let toAppend = "";
    
    if (condition)
    {
        toAppend = "Behavior running.";
    }
    else
    {
        toAppend = "Behavior skipped.";
    }

    trace(msg + " " + toAppend);
    return condition;
}

export function trace(msg) {
    console.log(msg);
}