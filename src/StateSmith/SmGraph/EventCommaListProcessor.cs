#nullable enable

using System;
using System.Text.RegularExpressions;
using StateSmith.Output.UserConfig;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventCommaListProcessor
{
    private readonly EventMapping mappingToPopulate;
    private readonly RenderConfigBaseVars renderConfig;

    private static Regex parsingRegex = new Regex(@"
            ^  # start of string

            # event name
            (?<eventName>
                \w+
            )

            # optional event ID mapping
            (?:
                \s*  # optional whitespace

                # assignment operator options
                (?: = | => )

                \s*  # optional whitespace

                (?<eventValue>
                    # we allow numbers and words here so users can use external values like `EV1 = SYSTEM_EV1`
                    \w+
                )
            )?

            $ # end of string
            ", RegexOptions.IgnorePatternWhitespace);

    public EventCommaListProcessor(EventMapping mappingToPopulate, RenderConfigBaseVars renderConfig)
    {
        this.mappingToPopulate = mappingToPopulate;
        this.renderConfig = renderConfig;
    }

    public static void ParseStringToEventMapping(EventMapping mappingToPopulate, string eventCommaList)
    {
        // split the input string by commas and trim whitespace
        var mappingElements = eventCommaList.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        try
        {
            foreach (var element in mappingElements)
            {
                ParseEventElement(element, eventName: out string eventName, eventValue: out string eventValue);

                mappingToPopulate.AddEventValueMapping(
                    eventName,
                    eventValue
                );
            }

            if (!mappingToPopulate.IsEmpty)
            {
                ValidateAllImplicitOrExplicitValues(mappingToPopulate);
                ValidateNoDuplicateValues(mappingToPopulate);
            }
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException($"Failed processing user RenderConfig.{nameof(RenderConfigBaseVars.EventCommaList)} string: `{eventCommaList}`. "
                + $"Mapping so far: `{JsonSerializer.Serialize(mappingToPopulate.Map)}`. Info: https://github.com/StateSmith/StateSmith/issues/470", e);
        }
    }

    private static void ValidateAllImplicitOrExplicitValues(EventMapping mapping)
    {
        bool startingExplicitness = EventMapping.IsExplicitEventValue(mapping.Map.First().Value);

        foreach (var keyValuePair in mapping.Map)
        {
            var eventName = keyValuePair.Key;
            var eventValue = keyValuePair.Value;
            bool isExplicitlyDefined = EventMapping.IsExplicitEventValue(eventValue);

            bool differs = isExplicitlyDefined != startingExplicitness;

            if (differs)
            {
                throw new ArgumentException($"Mapping for event `{eventName}` must specify a value because other events have a specified value. "
                    + $"All events must either have a value or not have a value. Info: https://github.com/StateSmith/StateSmith/issues/470");
            }
        }
    }

    public static void ValidateNoDuplicateValues(EventMapping mapping)
    {
        // throw exception if multiple events map to the same value
        var valueToEventMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var keyValuePair in mapping.Map)
        {
            var eventName = keyValuePair.Key;
            var eventValue = keyValuePair.Value;

            if (eventValue == "")
            {
                // If no value is specified, we can skip this check.
                continue;
            }

            if (valueToEventMap.ContainsKey(eventValue))
            {
                throw new ArgumentException($"Multiple events were mapped to value `{eventValue}`. Not allowed.");
            }
            valueToEventMap[eventValue] = eventName;
        }
    }

    public static void ParseEventElement(string element, out string eventName, out string eventValue)
    {
        var match = parsingRegex.Match(element);
        if (!match.Success)
        {
            throw new ArgumentException($"Failed parsing event mapping item `{element}`. See valid examples at https://github.com/StateSmith/StateSmith/issues/470 .");
        }

        eventName = match.Groups["eventName"].Value;
        eventValue = match.Groups["eventValue"].Value;
    }

    public void Process()
    {
        ParseStringToEventMapping(mappingToPopulate, renderConfig.EventCommaList);
    }
}
