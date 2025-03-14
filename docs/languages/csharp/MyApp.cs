
public class MyApp
{
    
    public static void Main()
    {
        LightbulbCallback callback = new LightbulbCallback();
        Lightbulb bulb = new Lightbulb(callback);
        bulb.Start();

        Console.WriteLine("Press <enter> to toggle the light switch.");
        Console.WriteLine("Press ^C to quit.");

        while (true) {            
            Console.ReadLine();
            bulb.DispatchEvent(Lightbulb.EventId.SWITCH);
        }
    }
}