import Lightbulb;

public class MyApp {
    public static void main(String[] args) {        
        Lightbulb bulb = new Lightbulb();
        bulb.start();
        bulb.dispatchEvent(Lightbulb.Events.SWITCH);

    }
}
