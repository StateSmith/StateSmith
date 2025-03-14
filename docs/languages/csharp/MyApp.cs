import Lightbulb;
import LightbulbCallback;

public class MyApp
{
    public void Main()
    {
        LightbulbCallback callback = new LightbulbCallback();
        Lightbulb lightbulb = new Lightbulb(callback);
        lightbulb.Start();
        while(true) {            
            lightbulb.DispatchEvent(Lightbulb.EventId.SWITCH);
        }
    }
}