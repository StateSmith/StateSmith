var Printer = module("Printer")

Printer.trace = def(msg)
    print(msg)
end

Printer.trace_guard = def(msg, condition)
    var suffix = condition ? "Behavior running." : "Behavior skipped."
    print(msg + " " + suffix)
    return condition
end

Printer.print_divider = def()
    print("===================================================")
end

Printer.print_start = def()
    print("Start Statemachine")
    Printer.print_divider()
end

Printer.print_dispatch_event_name = def(event_name)
    print("Dispatch event " + event_name)
    Printer.print_divider()
end

return Printer
