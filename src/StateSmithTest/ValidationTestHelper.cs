using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;


namespace StateSmithTest
{
    public class ValidationTestHelper
    {
        public Compiler compiler;

        public ValidationTestHelper()
        {
            compiler = new Compiler();
        }

        public void ExpectBehaviorValidationException(string exceptionMessagePart, Action? additionalAction = null)
        {
            compiler.SetupRoots();

            Action action = () =>
            {
                compiler.Validate();
                additionalAction?.Invoke();
            };
            action.Should().Throw<BehaviorValidationException>()
                .Where(e => e.Message.Contains(exceptionMessagePart));
        }

        public void ExpectVertexValidationException(string exceptionMessagePart, Action? additionalAction = null)
        {
            compiler.SetupRoots();

            Action action = () =>
            {
                compiler.Validate();
                additionalAction?.Invoke();
            };
            action.Should().Throw<VertexValidationException>()
                .Where(e => e.Message.Contains(exceptionMessagePart));
        }

        public void ExpectVertexValidationExceptionWildcard(string wildcardMessage)
        {
            compiler.SetupRoots();

            Action action = () => compiler.Validate();
            action.Should().Throw<VertexValidationException>()
                .WithMessage(wildcardMessage);
        }

        public void ExpectVertexValidationExceptionWildcard(Vertex vertex, string wildcardMessage)
        {
            compiler.SetupRoots();

            Action action = () => compiler.Validate();
            action.Should().Throw<VertexValidationException>()
                .WithMessage(wildcardMessage)
                .Where(e => e.vertex == vertex)
                ;
        }

        public void ExpectValid()
        {
            compiler.SetupRoots();
            compiler.Validate();
        }
    }
}
