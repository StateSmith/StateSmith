// package statesmithtest.spec._2.java;

class MainClass {

    companion object {
        fun trace(str: String) {
            System.out.println(str)
            System.out.flush()      // might be useful in case code fails in someway
        }

        fun traceGuard(str: String, guard: Boolean): Boolean {
            System.out.print(str + " ")

            if (guard) {
                System.out.print("Behavior running.")
            } else {
                System.out.print("Behavior skipped.")
            }

            System.out.println()

            return guard;
        }

        fun printDivider() {
            System.out.println("===================================================")
        }

        fun printStart() {
            System.out.println("Start Statemachine");
            printDivider()
        }

        fun printDispatchEventName(eventName: String) {
            System.out.println("Dispatch event " + eventName);
            printDivider()
        }

        fun actualMain(args: Array<String>) {
            val sm = Spec2Sm()

            printStart()
            sm.start()
            System.out.println()

            for (arg in args) {
                try {
                    val eventId = Spec2Sm.EventId.valueOf(arg.uppercase())
                    printDispatchEventName(arg)
                    sm.dispatchEvent(eventId)
                    System.out.println()
                } catch (e: IllegalArgumentException) {
                    throw IllegalArgumentException("bad arg: `" + arg + "`")
                }
            }
        }

        fun main(args: Array<String>) {
            actualMain(args)
        }
    }
}
