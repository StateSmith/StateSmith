using System;
using Xunit;
using FluentAssertions;

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
    }
}
