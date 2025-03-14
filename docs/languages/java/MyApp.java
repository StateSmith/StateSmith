public class MyApp {
    LightbulbCallback callback = new LightbulbCallback();    
    Lightbulb bulb = new Lightbulb(callback);
    
    public static void main(String[] args) throws Exception {
        bulb.start();

        System.out.println("Press <enter> to toggle the light switch.");
        System.out.println("Press ^C to quit.");

        while(true) {
            System.in.read();
            bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
        }
        
    }
}
