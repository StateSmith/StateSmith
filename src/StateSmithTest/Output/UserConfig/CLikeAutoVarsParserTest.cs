#nullable enable

using FluentAssertions;
using StateSmith.Output.UserConfig;
using System;
using System.Collections.Generic;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

public class CLikeAutoVarsParserTest
{
    readonly CLikeAutoVarsParser autoVarsParser = new();

    private List<string> ParseIdentifiers(string str) => autoVarsParser.ParseIdentifiers(str);

    [Fact]
    public void Parse()
    {
        ParseIdentifiers("""
            struct MyType blah;
            int x [33];   
            uint8_t b ;
            uint8_t _cat : 54 ;
            bool _;
            int (*(*foo)(const void *))[3];
            const int (* volatile bar)[64];
            int count __attribute__((aligned (16))) = 0;
            unsigned long long a_long __attribute__ ((warn_if_not_aligned (16)));
            """).Should().BeEquivalentTo("blah, x, b, _cat, _, foo, bar, count, a_long".Split(", "));
    }

    [Fact]
    public void ParseMultiple()
    {
        ParseIdentifiers("""
            struct MyType blah1, blah2 ;
            int a, b, c;
            int an, *wacky[34][21], example;
            """).Should().BeEquivalentTo("blah1, blah2, a, b, c, an, wacky, example".Split(", "));

        ParseIdentifiers("""
            int an, **** wacky [34] [SOME_SIZE], example;
            """).Should().BeEquivalentTo("an, wacky, example".Split(", "));

        ParseIdentifiers("""
            int an, **** wacky [34];
            """).Should().BeEquivalentTo("an, wacky".Split(", "));
    }

    [Fact]
    public void ParseWithComments()
    {
        ParseIdentifiers("""
                     struct MyType blah; // some wacky leading white space
            
            // int x [33];   
            /* uint8_t b ;
            uint8_t _cat : 54 ;
            */bool _;
            int (*(*foo)(const void *))[3];
            
            const int (* volatile bar)[64];
            """).Should().BeEquivalentTo("blah, _, foo, bar".Split(", "));
    }

    [Fact]
    public void ParseAnonymousStructShouldThrow()
    {
        Action a = () => ParseIdentifiers("""
                          struct MyType blah;
            int x [33];   
            
            struct {
              int input_1;
            } inputs;

            uint8_t cat;
            """).Should().BeEquivalentTo("blah, x, b, _cat, _, foo, bar".Split(", "));

        a.Should().Throw<ArgumentException>()
            .WithMessage("*https://github.com/StateSmith/StateSmith/issues/91*")
            .WithMessage("*anonymous struct*")
            ;
    }
}
