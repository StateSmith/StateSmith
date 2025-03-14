import Lightbulb;
import LightbulbCallback;

public class MyApp
{
    LightbulbCallback callback = new LightbulbCallback();
    Lightbulb lightbulb = new Lightbulb(callback);
    
    public void Main()
    {
        lightbulb.Start();

        Console.WriteLine("Press <enter> to toggle the light switch.");
        Console.WriteLine("Press ^C to quit.");

        while (true) {            
            Console.ReadLine();
            lightbulb.DispatchEvent(Lightbulb.EventId.SWITCH);
        }
    }
}