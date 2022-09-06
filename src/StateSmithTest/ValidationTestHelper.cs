using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest.InitialStateProcessor
{

    public class ValidationTestHelper
    {
        protected Compiler compiler;

        public ValidationTestHelper()
        {
            compiler = new Compiler();
        }

        protected void ExpectValidationException(string exceptionMessagePart)
        {
            compiler.SetupRoots();

            Action action = () => compiler.Validate();
            action.Should().Throw<VertexValidationException>()
                .Where(e => e.Message.Contains(exceptionMessagePart));
        }

        protected void ExpectValidationExceptionWildcard(string wildcardMessage)
        {
            compiler.SetupRoots();

            Action action = () => compiler.Validate();
            action.Should().Throw<VertexValidationException>()
                .WithMessage(wildcardMessage);
        }
    }
}
