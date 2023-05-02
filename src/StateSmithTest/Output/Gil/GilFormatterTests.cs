using FluentAssertions;
using StateSmith.Output.Gil;
using Xunit;

namespace StateSmithTest.Output.Gil;

public class GilFormatterTests
{
    [Fact]
    public void Test1()
    {
        var input = """
            public class Class1
            {
                int x = 55;
                int y = 33;
            }
            """;

        const string Expected = """
            public class Class1
            {
                int x = 55;
                int y = 33;
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Test1b()
    {
        var input = """
            public class Class1
            {
                int x = 55;    // some comment that shouldn't be de-indented
                int y = 33;
            }
            """;

        const string Expected = """
            public class Class1
            {
                int x = 55;    // some comment that shouldn't be de-indented
                int y = 33;
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Test2()
    {
        var input = """
            public class Class1 {
                int x = 55;
                int y = 33;
            }
            """;

        const string Expected = """
            public class Class1 {
                int x = 55;
                int y = 33;
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Enum()
    {
        var input = """
            public class Class1
            {
                public enum EventId 
                {
                    A,
                    B,
                    C
                }
            }
            """;

        const string Expected = """
            public class Class1
            {
            public enum EventId 
            {
                A,
                B,
                C
            }
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Const()
    {
        var input = """
            public class Class1
            {
                public const int EventIdCount = 2;
            }
            """;

        const string Expected = """
            public class Class1
            {
            public const int EventIdCount = 2;
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Method()
    {
        var input = """
            public class Class1 {
                int x = 55;
                int y = 33;

            public void SomeMethod() {
                int x = 55;
            }
            }
            """;

        const string Expected = """
            public class Class1 {
                int x = 55;
                int y = 33;

            public void SomeMethod() {
                int x = 55;
            }
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Comment()
    {
        var input = """
            public class Class1 {
                public void SomeMethod() {
                    int x = 55;
                    /* line1
                    line2 */
                }
            }
            """;

        const string Expected = """
            public class Class1 {
            public void SomeMethod() {
                int x = 55;
                /* line1
                line2 */
            }
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Constructor()
    {
        var input = """
            public class Class1 {
                int x = 55;
                int y = 33;

                public Class1() {
                    int x = 55;
                }
            }
            """;

        const string Expected = """
            public class Class1 {
                int x = 55;
                int y = 33;

            public Class1() {
                int x = 55;
            }
            }
            """;

        Test(input, Expected);
    }

    [Fact]
    public void Test4()
    {
        var input = """
            public class Class1 {
                public class Inner1
                {
                    public void SomeMethod()
                    {
                        int x = 55;
                    }
                }
            }
            """;

        const string Expected = """
            public class Class1 {
            public class Inner1
            {
            public void SomeMethod()
            {
                int x = 55;
            }
            }
            }
            """;

        Test(input, Expected);
    }


    [Fact]
    public void Test5()
    {
        var input = """
            public class Class1 {
                public class Inner1
                {
                    public void SomeMethod()
                    {
                        int x = 55;
                    }
                    public class Inner2 // blah blah
                    {
                        int y = 231;

                        public void SomeMethod()
                        {
                            int x = 55;
                        }
                    }
                }
            }
            """;

        const string Expected = """
            public class Class1 {
            public class Inner1
            {
            public void SomeMethod()
            {
                int x = 55;
            }
            public class Inner2 // blah blah
            {
                int y = 231;

            public void SomeMethod()
            {
                int x = 55;
            }
            }
            }
            }
            """;

        Test(input, Expected);
    }

    private static void Test(string input, string Expected)
    {
        GilFormatter.Format(input).ShouldBeShowDiff(Expected);
    }
}


