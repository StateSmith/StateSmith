
public class Program
{
    
    public static void Main()
    {
        Lightbulb bulb = new Lightbulb();
        bulb.Start();

        Console.WriteLine("Press <enter> to toggle the light switch.");
        Console.WriteLine("Press ^C to quit.");

        while (true) {            
            Console.ReadLine();
            bulb.DispatchEvent(Lightbulb.EventId.SWITCH);
        }
    }
}