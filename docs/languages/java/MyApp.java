public class MyApp {
    public static void main(String[] args) throws Exception {
        Lightbulb bulb = new Lightbulb();

        bulb.start();

        System.out.println("Press <enter> to toggle the light switch.");
        System.out.println("Press ^C to quit.");

        while(true) {
            System.in.read();
            bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
        }
        
    }
}
