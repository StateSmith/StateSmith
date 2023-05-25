using System.Collections.Generic;

namespace StateSmith.SmGraph.Visitors;

public interface IVertexVisitor
{
    void Visit(Vertex v);
    void Visit(NamedVertex v);
    void Visit(State v);
    void Visit(OrthoState v);
    void Visit(StateMachine v);
    void Visit(NotesVertex v);
    void Visit(InitialState v);

    void Visit(ChoicePoint v);
    void Visit(EntryPoint v);
    void Visit(ExitPoint v);
    void Visit(HistoryVertex v);
    void Visit(HistoryContinueVertex v);
    void Visit(RenderConfigVertex v);
    void Visit(ConfigOptionVertex v);
}
