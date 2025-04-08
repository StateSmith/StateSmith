using CommandLine;
using Spectre.Console.Testing;
using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using StateSmith.Runner;

namespace StateSmith.Exe.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void ParseCommands_ValidArgs_ReturnsZero()
        {
            // Arrange
            var args = new[] { "--lang", "C99" };
            var console = new TestConsole();
            var program = new Program();

            // Act
            var result = program.ParseCommands(args, console);

            // Assert
            result.Should().Be(0);
            program._options.Lang.Should().Be(TranspilerId.C99);
        }

        [Fact]
        public void ParseCommands_InvalidArgs_ReturnsOne()
        {
            // Arrange
            var args = new[] { "--invalidOption" };
            var console = new TestConsole();
            var program = new Program();

            // Act
            var result = program.ParseCommands(args, console);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void Run_MissingFile()
        {
            // Arrange
            var args = new string[] { "--lang=JavaScript", "test.puml" };
            var console = new TestConsole();
            Program._console = new TestConsole();

            // Act
            Action act = () => Program.Main(args);

            // Assert
            act.Should().NotThrow();
            ((TestConsole)Program._console).Output.Should().Contain("Failed while trying to read '`test.puml`' for settings.");

        }


        [Fact]
        public void Run_NoArgs()
        {
            // Arrange
            var args = new string[] {  };
            Program._console = new TestConsole();
            
            // Act
            Program.Main(args);

            // Assert
            ((TestConsole)Program._console).Output.Should().Contain("StateSmith - a state machine diagram tool.");
        }

        [Fact]
        public void PrintUsage_DisplaysHelpText()
        {
            // Arrange
            var args = new[] { "--help" };
            var console = new TestConsole();
            var program = new Program();
            var parserResult = new Parser().ParseArguments<ProgramOptions>(args);

            // Act
            Program.PrintUsage(parserResult, console);

            // Assert
            console.Output.Should().Contain("StateSmith - a state machine diagram tool.");
        }
    }
}