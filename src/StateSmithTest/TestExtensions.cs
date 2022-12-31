using System;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.compiler;

namespace StateSmithTest
{
    public static class TestExtensions
    {
        public static string ConvertLineEndingsToN(this string code)
        {
            return code.Replace("\r\n", "\n");
        }

        public static string SanitizeCode(this string code)
        {
            return code.Trim().Replace("\r\n", "\n");
        }

        public static void EqualsCode(this string code, string expectedCode)
        {
            if (expectedCode.SanitizeCode() != code.SanitizeCode())
            {
                Console.WriteLine(expectedCode);
                Console.WriteLine(code);
                bool wantDiff = true;

                if (wantDiff)
                {
                    Assert.Equal(expectedCode.SanitizeCode(), code.SanitizeCode());
                }
                else
                {
                    code.SanitizeCode().Should().Be(expectedCode.SanitizeCode());
                }
            }
        }

        public static void ShouldHaveUmlBehaviors(this Vertex vertex, string expected)
        {
            // Assert.Equal(expected.ConvertLineEndingsToN(), vertex.Behaviors.UmlDescription());
            vertex.Behaviors.UmlDescription().ShouldBeShowDiff(expected);
        }

        public static void ShouldBeShowDiff(this string actual, string expected)
        {
            expected = expected.ConvertLineEndingsToN();

            if (expected != actual)
            {
                var diff = StringDiffer.Diff(expected.ConvertLineEndingsToN(), actual);
                Assert.True(false, diff);
            }
        }
    }
}
