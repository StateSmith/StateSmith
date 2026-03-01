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

            println()

            return guard;
        }

        fun printDivider() {
            println("===================================================")
        }

        fun printStart() {
            println("Start Statemachine");
            printDivider()
        }

        fun printDispatchEventName(eventName: String) {
            println("Dispatch event " + eventName);
            printDivider()
        }

        fun actualMain(args: Array<String>) {
            val sm = Spec2Sm()

            printStart()
            sm.start()
            println()

            for (arg in args.drop(1)) {
                try {
                    val eventId = Spec2Sm.EventId.valueOf(arg.uppercase())
                    printDispatchEventName(arg)
                    sm.dispatchEvent(eventId)
                    println()
                } catch (e: IllegalArgumentException) {
                    throw IllegalArgumentException("bad arg: `" + arg + "`")
                }
            }
        }
    }
}

fun main(args: Array<String>) {
    MainClass.actualMain(args)
}