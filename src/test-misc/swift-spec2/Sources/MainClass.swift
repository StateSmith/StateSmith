@MainActor
public class MainClass {
    static let eventMapping: [String: Spec2Sm.EventId] = [
        "DO": .DO,
        "EV1": .EV1,
        "EV10": .EV10,
        "EV2": .EV2,
        "EV3": .EV3,
        "EV4": .EV4,
        "EV5": .EV5,
        "EV6": .EV6,
        "EV7": .EV7,
        "EV8": .EV8,
        "EV9": .EV9,
        "EVBACK": .EVBACK,
        "EVCLOSE": .EVCLOSE,
        "EVOPEN": .EVOPEN,
        "EVSTEP": .EVSTEP,
    ]

    public static func trace(_ str: String) {
        print(str)
    }

    public static func traceGuard(_ str: String, _ `guard`: Bool) -> Bool {
        print(str + " ", terminator:"")

        if `guard` {
            print("Behavior running.", terminator:"")
        } else {
            print("Behavior skipped.", terminator:"")
        }

        print("")

        return `guard`
    }

    public static func printDivider() {
        print("===================================================")
    }

    public static func printStart() {
        print("Start Statemachine")
        printDivider()
    }

    public static func printDispatchEventName(_ eventName: String) {
        print("Dispatch event " + eventName)
        printDivider()
    }

    public static func actualMain(_ args: [String]) {
        let sm = Spec2Sm()

        printStart()
        sm.start()
        print("")

        for arg in args {
            guard let eventId = eventMapping[arg] else {
                print("bad arg: `" + arg + "`")
            }
            printDispatchEventName(arg)
            sm.dispatchEvent(eventId)
            print("")
        }
    }
}

@main
struct SpecSwift {
    static func main() {
        MainClass.actualMain(CommandLine.arguments)
    }
}