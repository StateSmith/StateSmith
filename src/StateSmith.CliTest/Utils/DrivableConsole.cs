using FluentAssertions;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;
using StateSmith.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateSmith.CliTest.Utils;

/// <summary>
/// This is a mostly copy paste of the <see cref="TestConsole"/> class from the Spectre.Console library.
/// It is different in that it allows input to be driven live without having to pre-define it.
/// </summary>
public class DrivableConsole : IAnsiConsole
{
    private readonly IAnsiConsole _console;
    private readonly StringWriter _writer;
    private IAnsiConsoleCursor? _cursor;

    /// <inheritdoc/>
    public Profile Profile => _console.Profile;

    /// <inheritdoc/>
    public IExclusivityMode ExclusivityMode => _console.ExclusivityMode;

    /// <summary>
    /// Gets the console input.
    /// </summary>
    public DrivableConsoleInput Input { get; }    // @adamfk change here

    /// <inheritdoc/>
    public RenderPipeline Pipeline => _console.Pipeline;

    /// <inheritdoc/>
    public IAnsiConsoleCursor Cursor => _cursor ?? _console.Cursor;

    /// <inheritdoc/>
    IAnsiConsoleInput IAnsiConsole.Input => Input;

    /// <summary>
    /// Gets the console output.
    /// </summary>
    public string Output => _writer.ToString(); // @adamfk todo - ensure this is thread safe for reading

    /// <summary>
    /// Gets the console output lines.
    /// </summary>
    public IReadOnlyList<string> Lines => Output.NormalizeLineEndings().TrimEnd('\n').Split(new char[] { '\n' });

    /// <summary>
    /// Gets or sets a value indicating whether or not VT/ANSI sequences
    /// should be emitted to the console.
    /// </summary>
    public bool EmitAnsiSequences { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestConsole"/> class.
    /// </summary>
    public DrivableConsole()
    {
        _writer = new StringWriter();
        _cursor = new NoopCursor();

        Input = new DrivableConsoleInput();
        EmitAnsiSequences = false;

        _console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = (ColorSystemSupport)ColorSystem.TrueColor,
            Out = new AnsiConsoleOutput(_writer),
            Interactive = InteractionSupport.Yes,   // @adamfk change here
            ExclusivityMode = new NoopExclusivityMode(),
            Enrichment = new ProfileEnrichment
            {
                UseDefaultEnrichers = false,
            },
        });

        _console.Profile.Width = 80;
        _console.Profile.Height = 24;
        _console.Profile.Capabilities.Ansi = true;
        _console.Profile.Capabilities.Unicode = true;
    }

    public void ClearOutput()
    {
        _writer.GetStringBuilder().Clear();
    }

    public void SendDownArrow()
    {
        Input.PushKey(ConsoleKey.DownArrow);
        SendEnter();
    }

    public void SendEnter()
    {
        Input.PushKey(ConsoleKey.Enter);
    }

    public void WaitForOutput(string expected, StringComparison compareType = StringComparison.CurrentCulture)
    {
        Stopwatch sw = new();
        sw.Start();

        while (!Output.Contains(expected, compareType))
        {
            Thread.Sleep(10);

            if (sw.ElapsedMilliseconds >= 2000)
            {
                Output.Should().Contain(expected); // this will fail the test
            }
        }
    }

    public void WaitForOutputIgnoreCase(string expected)
    {
        WaitForOutput(expected, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _writer.Dispose();
    }

    /// <inheritdoc/>
    public void Clear(bool home)
    {
        _console.Clear(home);
    }

    /// <inheritdoc/>
    public void Write(IRenderable renderable)
    {
        if (EmitAnsiSequences)
        {
            _console.Write(renderable);
        }
        else
        {
            foreach (var segment in renderable.GetSegments(this))
            {
                if (segment.IsControlCode)
                {
                    continue;
                }

                Profile.Out.Writer.Write(segment.Text);
            }
        }
    }

    internal void SetCursor(IAnsiConsoleCursor? cursor)
    {
        _cursor = cursor;
    }

    class NoopCursor : IAnsiConsoleCursor
    {
        public void Move(CursorDirection direction, int steps)
        {
        }

        public void SetPosition(int column, int line)
        {
        }

        public void Show(bool show)
        {
        }
    }

    class NoopExclusivityMode : IExclusivityMode
    {
        public T Run<T>(Func<T> func)
        {
            return func();
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> func)
        {
            return await func().ConfigureAwait(false);
        }
    }

}
