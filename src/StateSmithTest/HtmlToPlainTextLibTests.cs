using Xunit;
using StateSmith.Input.DrawIo;

namespace StateSmithTest.DrawIo;

/// <summary>
/// See https://github.com/StateSmith/StateSmith/issues/414
/// </summary>
public class HtmlToPlainTextLibTests
{
    [Fact]
    public void Test()
    {
        static void Assert(string input)
        {
            string expected = "$NOTES\nblah";
            AssertExpected(input, expected: expected);
        }

        Assert("<div>$NOTES</div>blah");
        Assert("$NOTES<div>blah</div>");
        Assert("<p>$NOTES</p>blah");
        Assert("$NOTES<p>blah</p>");

        Assert("$NOTES</br>blah");
        Assert("$NOTES<br>blah");
        Assert("$NOTES<br/>blah");
        Assert("$NOTES<br style=''>blah");
        Assert("$NOTES<br style='color:blue;'>blah");
    }

    [Fact]
    public void TestStateLabel()
    {
        AssertExpected("""
            <div>S1</div>
            PRESS / a++;
            """, expected: """
            S1
            PRESS / a++;
            """);
    }

    [Fact]
    public void TestColored()
    {
        AssertExpected("""<font color="#00aaff">enter&nbsp;</font><font color="#ffd700">/</font><font color="#dcdcaa">&nbsp;{&nbsp;<br></font><font color="#dcdcaa">&nbsp; set_timeout(5 min);<br>&nbsp; x *= 23;<br>}</font><font color="#00aaff"><br></font>""",
            expected: """
            enter / { 
              set_timeout(5 min);
              x *= 23;
            }
            """);
    }

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/414
    /// </summary>
    [Fact]
    public void GithubTestCases_414()
    {
        // # 3 lines of text
        AssertExpected("123<div>456</div><div>789</div>", expected: """
            123
            456
            789
            """);
        AssertExpected("123<br>456<br>789", expected: """
            123
            456
            789
            """);

        // # immediate new line
        // below is technically incorrect (has an extra trailing \n), but it's fine.
        // It's also what draw.io does when converting from html to plain text.
        AssertExpected("<div><br></div><div>123</div><div><br></div>", expected: "\n123\n\n");

        // # leading whitespace
        AssertExpected("&nbsp; &nbsp;<div>123</div><div>&nbsp; &nbsp;</div>", expected: """
               
            123
               
            """);

        // Multiple text alignments & horizontal rule
        AssertExpected("""123<div><hr><div style="text-align: right;"><span style="background-color: initial;">123</span></div></div><div style="text-align: right;">456 789</div><div style="text-align: left;">abc def ghi</div>""", expected: """
            123
            123
            456 789
            abc def ghi
            """);

        // # more alignments
        AssertExpected("<div style=\"text-align: left;\"><span style=\"background-color: initial;\">123</span></div><div>abc</div><div>123 456 789</div>", expected: """
            123
            abc
            123 456 789
            """);

        // # </div> followed by text
        AssertExpected("""<div>New "not nested" state machine declaration. You can only have a single one of these in a draw.io file</div>https://github.com/StateSmith/StateSmith/issues/359""", expected: """
            New "not nested" state machine declaration. You can only have a single one of these in a draw.io file
            https://github.com/StateSmith/StateSmith/issues/359
            """);

        // # </div> followed by something other than another div
        AssertExpected("""<div>New</div>&nbsp;https""", expected: """
            New
             https
            """);
    }

    static void AssertExpected(string input, string expected)
    {
        new HtmlToPlainTextLib().Convert(input).ShouldBeShowDiff(expected, outputCleanActual: true);
    }
}

