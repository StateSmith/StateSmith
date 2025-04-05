public class MyApp {
    public static void main(String[] args) throws Exception {
        LightbulbDelegate delegate = new LightbulbDelegate();    
        Lightbulb bulb = new Lightbulb(delegate);

        bulb.start();

        System.out.println("Press <enter> to toggle the light switch.");
        System.out.println("Press ^C to quit.");

        while(true) {
            System.in.read();
            bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
        }
        
    }
}
