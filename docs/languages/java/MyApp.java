public class MyApp {
    public static void main(String[] args) throws Exception {
        LightbulbCallback callback = new LightbulbCallback();    
        Lightbulb bulb = new Lightbulb(callback);
        bulb.start();

        System.out.println("Press <enter> to toggle the light switch.");
        System.out.println("Press ^C to quit.");

        while(true) {
            System.in.read();
            bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
        }
        
    }
}
