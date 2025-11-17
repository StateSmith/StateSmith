import Spec2Sm
import Printer
import string

class MainClass
    static def actual_main(args)
        var sm = Spec2Sm.Spec2Sm()

        Printer.print_start()
        sm.start()
        print("")

        var index = 0
        for event_arg : args
            var event_name = string.toupper(event_arg)
            var event_id = MainClass._to_event_id(event_name)
            if event_id == nil
                raise "bad i:" + str(index) + " arg: `" + event_arg + "`"
            end

            Printer.print_dispatch_event_name(event_arg)
            sm.dispatchEvent(event_id)
            print("")

            index += 1
        end
    end

    static def main(args)
        MainClass.actual_main(args)
    end

    static def _to_event_id(name)
        if name == "DO"
            return Spec2Sm.EventId.DO
        elif name == "EV1"
            return Spec2Sm.EventId.EV1
        elif name == "EV2"
            return Spec2Sm.EventId.EV2
        elif name == "EV3"
            return Spec2Sm.EventId.EV3
        elif name == "EV4"
            return Spec2Sm.EventId.EV4
        elif name == "EV5"
            return Spec2Sm.EventId.EV5
        elif name == "EV6"
            return Spec2Sm.EventId.EV6
        elif name == "EV7"
            return Spec2Sm.EventId.EV7
        elif name == "EV8"
            return Spec2Sm.EventId.EV8
        elif name == "EV9"
            return Spec2Sm.EventId.EV9
        elif name == "EV10"
            return Spec2Sm.EventId.EV10
        elif name == "EVBACK"
            return Spec2Sm.EventId.EVBACK
        elif name == "EVCLOSE"
            return Spec2Sm.EventId.EVCLOSE
        elif name == "EVOPEN"
            return Spec2Sm.EventId.EVOPEN
        elif name == "EVSTEP"
            return Spec2Sm.EventId.EVSTEP
        else
            return nil
        end
    end
end

var args = []
var skip_first = true
for value : _argv
    if skip_first
        skip_first = false
        continue
    end
    args.push(value)
end

MainClass.main(args)
