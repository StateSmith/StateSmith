using StateSmith.compiler.Visitors;
using StateSmith.compiler;
using System.Linq;

#nullable enable

namespace StateSmith.Compiling
{
    public class HistoryContinueProcessor : DummyVertexVisitor
    {
        public override void Visit(HistoryContinueVertex hc)
        {
            var statesToTrack = hc.Siblings<NamedVertex>().ToList();

            foreach (var h in hc.historyVertices)
            {
                HistoryProcessor.TrackStates(h, null, statesToTrack);
            }

            hc.ParentState.RemoveChild(hc);
        }
    }
}
