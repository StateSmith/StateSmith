#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;

namespace StateSmith.Output;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/528
/// </summary>
public class SmGraphJsonExporter
{
    private Export export = new();

    // fields are nullable so that they can be omitted in json output
    public class Export
    {
        public string comment = "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528";
        public List<JsonNode>? beforeTransformations;
        public List<JsonNode>? afterTransformations;
    }

    // fields are nullable so that they can be omitted in json output
    public class JsonBehavior
    {
        public string? diagramId;
        public string? transitionTargetNodeId;
        public List<string>? triggers;
        public double? order;
        public string? guardCode;
        public string? actionCode;
        public string? viaEntry;    // https://github.com/StateSmith/StateSmith/issues/3
        public string? viaExit;     // https://github.com/StateSmith/StateSmith/issues/3
    }

    // fields are nullable so that they can be omitted in json output.
    // we may need to allow sub classing at some point in the future.
    public class JsonNode
    {
        public string? nodeId;
        public string? type;
        public string? name;
        public string? parentId;
        public string? diagramId;
        public string? comment;
        public List<string>? childrenIds;
        public List<JsonBehavior>? behaviors;
    }

    public static List<JsonNode> BuildNodes(StateMachine root)
    {
        int count = 0;
        Dictionary<Vertex, string> vertexToJsonIdMap = new();
        Dictionary<string, JsonNode> jsonIdToNodeMap = new();
        
        // add all nodes first before adding relationships
        root.VisitRecursively((Vertex vertex) =>
        {
            StringBuilder sb = new();
            var describer = new ShortDescribingVisitor(sb)
            {
                outputStateMachineName = true
            };

            string jsonId = describer.GetDescription(vertex);

            // ensure a unique id
            {
                Vertex? parent = vertex.Parent;               
                while (jsonIdToNodeMap.ContainsKey(jsonId) && parent != null)
                {
                    jsonId = describer.GetDescription(parent) + "." + jsonId;
                    parent = parent.Parent;
                }

                int count = 0;
                while (jsonIdToNodeMap.ContainsKey(jsonId))
                {
                    jsonId += "-" + count;
                    count++;
                }
            }

            var node = new JsonNode()
            {
                nodeId = jsonId,
                diagramId = vertex.DiagramId,
                type = vertex.GetType().Name
            };

            vertexToJsonIdMap[vertex] = jsonId;
            jsonIdToNodeMap[jsonId] = node;
            count++;
        });

        // now that all nodes are added, add relationships
        vertexToJsonIdMap.Keys.ToList().ForEach(vertex =>
        {
            string vertexJsonId = vertexToJsonIdMap[vertex];
            JsonNode node = jsonIdToNodeMap[vertexJsonId];

            if (vertex is NamedVertex namedVertex)
            {
                node.name = namedVertex.Name;
            }

            // add parent/child relationships
            if (vertex.Parent != null)
            {
                node.parentId = vertexToJsonIdMap[vertex.Parent];

                JsonNode parentNode = jsonIdToNodeMap[node.parentId];
                parentNode.childrenIds ??= new List<string>();
                parentNode.childrenIds.Add(node.nodeId.ThrowIfNull());
            }

            // add behaviors
            foreach (var behavior in vertex.Behaviors)
            {
                JsonBehavior jsonBehavior = new()
                {
                    diagramId = behavior.DiagramId,
                    order = behavior.order != Behavior.DEFAULT_ORDER ? behavior.order : null,
                    triggers = behavior.Triggers.Any() ? behavior.Triggers.ToList() : null,
                    guardCode = NullifyBlankString(behavior.guardCode),
                    actionCode = NullifyBlankString(behavior.actionCode),
                    viaEntry = behavior.viaEntry,
                    viaExit = behavior.viaExit
                };

                if (behavior.TransitionTarget != null)
                {
                    jsonBehavior.transitionTargetNodeId = vertexToJsonIdMap[behavior.TransitionTarget];
                }

                node.behaviors ??= new List<JsonBehavior>();
                node.behaviors.Add(jsonBehavior);
            }
        });

        return jsonIdToNodeMap.Values.ToList();
    }

    private static string? NullifyBlankString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        return str;
    }

    public void RecordBeforeTransformations(StateMachine stateMachine)
    {
        export.beforeTransformations = BuildNodes(stateMachine);
    }

    public void RecordAfterTransformations(StateMachine stateMachine)
    {
        export.afterTransformations = BuildNodes(stateMachine);
    }

    public string ExportToJson()
    {
        string json = JsonSerializer.Serialize(export, new JsonSerializerOptions { 
            WriteIndented = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // no need to escape "<>" chars in "<StateMachine>(MySm)"
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        return json;
    }
}
