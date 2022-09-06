using StateSmith.Input.antlr4;
using StateSmith.Input.Expansions;
using StateSmith.Input.Yed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using StateSmith.compiler;
using StateSmith.Input;

namespace StateSmith.Compiling
{
    public class DiagramEdgeException : Exception
    {
        DiagramEdge Edge { get; }

        public DiagramEdgeException(DiagramEdge edge) : base()
        {
            Edge = edge;
        }

        public DiagramEdgeException(DiagramEdge edge, string message) : base(message)
        {
            Edge = edge;
        }

        public DiagramEdgeException(DiagramEdge edge, string message, Exception innerException) : base(message, innerException)
        {
            Edge = edge;
        }
    }
}
