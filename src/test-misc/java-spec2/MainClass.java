// package statesmithtest.spec._2.java;

public class MainClass {

    public static void trace(String str) {
        System.out.println(str);
        System.out.flush();      // might be useful in case code fails in someway
    }

    public static boolean traceGuard(String str, boolean guard) {
        System.out.print(str + " ");

        if (guard) {
            System.out.print("Behavior running.");
        } else {
            System.out.print("Behavior skipped.");
        }

        System.out.println();

        return guard;
    }

    public static void printDivider() {
        System.out.println("===================================================");
    }

    public static void printStart() {
        System.out.println("Start Statemachine");
        printDivider();
    }

    public static void printDispatchEventName(String eventName) {
        System.out.println("Dispatch event " + eventName);
        printDivider();
    }

    public static void actualMain(String[] args) {
        Spec2Sm sm = new Spec2Sm();

        printStart();
        sm.start();
        System.out.println();

        for (int i = 0; i < args.length; i++) {
            String arg = args[i];

            try {
                Spec2Sm.EventId eventId = Spec2Sm.EventId.valueOf(arg.toUpperCase());
                printDispatchEventName(arg);
                sm.dispatchEvent(eventId);
                System.out.println();
            } catch (IllegalArgumentException e) {
                throw new IllegalArgumentException("bad i:" + i + " arg: `" + arg + "`");
            }
        }
    }

    public static void main(String[] args) {
        actualMain(args);
    }
}
