using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StateSmith.Common;

public class TimeStringParser
{
    public const int SecondsToMs = 1000;
    public const int MinutesToMs = 60 * SecondsToMs;
    public const int HoursToMs = 60 * MinutesToMs;

    public static int ElapsedTimeStringToMs(string elapsedTimeStr)
    {
        ParseNumberAndUnits(elapsedTimeStr, out double number, out string units);

        double multiplier;

        switch (units.ToLower())
        {
            case "millisecond":
            case "milliseconds":
            case "millis":
            case "ms":
                multiplier = 1;
                break;

            case "s":
            case "secs":
            case "seconds":
            case "second":
                multiplier = SecondsToMs;
                break;

            case "min":  // 1 min
            case "mins": // 4 mins
            case "minute":
            case "minutes":
                multiplier = MinutesToMs;
                break;

            case "h":
            case "hr":
            case "hrs":
            case "hours":
            case "hour":
                multiplier = HoursToMs;
                break;

            default: throw new ArgumentException("Unsupported time units: " + units);
        }

        return (int)(number * multiplier);
    }

    private static void ParseNumberAndUnits(string elapsedTimeStr, out double number, out string units)
    {
        var match = new Regex(@"(?x)
             ^ \s*
             # capture int or floating point number
             (?<number> \d+ 
                (?: [.] \d+ )?
             )
             \s*
             (?<units> \w+ )
             \s*
             $").Match(elapsedTimeStr);

        if (!match.Success)
            throw new ArgumentException("Failed to match elapsed time for input: " + elapsedTimeStr);

        number = double.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
        units = match.Groups["units"].Value;
    }
}
