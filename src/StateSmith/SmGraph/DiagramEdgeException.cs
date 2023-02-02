using StateSmith.Input.Antlr4;
using StateSmith.Input.Expansions;
using StateSmith.Input.Yed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using StateSmith.SmGraph;
using StateSmith.Input;

namespace StateSmith.SmGraph
{
    public class DiagramEdgeException : Exception
    {
        public DiagramEdge edge;

        public DiagramEdgeException(DiagramEdge edge, string message, Exception ex = null) : base(message, ex)
        {
            this.edge = edge;
        }
    }

    public class DiagramEdgeParseException : DiagramEdgeException
    {
        public Vertex sourceVertex;
        public Vertex targetVertex;

        public DiagramEdgeParseException(DiagramEdge edge, Vertex sourceVertex, Vertex targetVertex, string message) : base(edge, message)
        {
            this.sourceVertex = sourceVertex;
            this.targetVertex = targetVertex;
        }
    }
}
