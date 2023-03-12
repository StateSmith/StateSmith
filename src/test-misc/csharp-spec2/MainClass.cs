using Csharp.Spec2smTests;
using System;

namespace StateSmithTest.spec._2.CSharp;

public class MainClass
{
    public static void Trace(string str)
    {
        Console.WriteLine(str);
        Console.Out.Flush();      // might be useful in case code fails in someway
    }

    public static bool trace_guard(string str, bool guard)
    {
        Console.Write(str + " ");

        if (guard)
        {
            Console.Write("Behavior running.");
        }
        else
        {
            Console.Write("Behavior skipped.");
        }

        Console.Write("\n");

        return guard;
    }

    public static void print_divider()
    {
        Console.Write("===================================================\n");
    }

    public static void print_start()
    {
        Console.Write("Start Statemachine\n");
        print_divider();
    }

    public static void print_dispatch_event_name(string event_name)
    {
        Console.Write($"Dispatch event {event_name}\n");
        print_divider();
    }

    public static void ActualMain(string[] args)
    {
        Spec2Sm sm = new();

        print_start();
        sm.Start();
        Console.WriteLine();

        for (int i = 0; i < args.Length; i++) // start at 1 to skip program name
        {
            string arg = args[i];
            
            if (!Enum.TryParse(arg.ToUpper(), out Spec2Sm.EventId eventId))
            {
                throw new ArgumentException($"bad i:{i} arg: `{arg}`");
            }

            print_dispatch_event_name(arg);

            sm.DispatchEvent(eventId);
            Console.WriteLine();
        }
    }

    public static void Main(string[] args)
    {
        ActualMain(args);
    }
}
