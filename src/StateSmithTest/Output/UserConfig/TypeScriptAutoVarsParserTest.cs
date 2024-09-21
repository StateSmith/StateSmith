#nullable enable

using FluentAssertions;
using StateSmith.Output.UserConfig.AutoVars;
using System.Collections.Generic;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

public class TypeScriptAutoVarsParserTest
{
    readonly TypeScriptAutoVarsParser autoVarsParser = new();

    private List<string> ParseIdentifiers(string str) => autoVarsParser.ParseIdentifiers(str);

    [Fact]
    public void Parse()
    {
        // as long as it ends with a semicolon
        ParseIdentifiers("""
            /* some_comment : stuff, */
            blah: any;
            count = 0;
            count2: number;
            count3: number = 0;
            str1 = " public noMatch = 2; ";
            str2 = ' noMatch = 2; ';
            str3 = `
                public bad4;
                public bad5;
                public bad6: number = -1;
            `;
            public count4;
            public count5;
            public count6: number = -1;
            count7 ; 
            obj1: {a: 1, b: 2} = {a: 1, b: 2};
            public obj2: List<string> = [];
            """).Should().BeEquivalentTo("blah, count, count2, count3, count4, count5, count6, count7, obj1, obj2, str1, str2, str3".Split(", "));
    }
}
