The below diagram helps explain how StateSmith turns a diagram file into state machine code.

![image](https://user-images.githubusercontent.com/274012/218192250-f4b4b814-3cbe-462e-bc1a-02c68b9d6488.png)

## Step 1
First, a diagram file (draw.io, yEd, PlantUML) is parsed into simple diagram objects. This intermediate step makes it relatively easy to add new diagram parsers to StateSmith.

```c#
public class DiagramNode
{
    public string id, label;
    public DiagramNode? parent;
    public List<DiagramNode> children;
}

public class DiagramEdge
{
    public string id, label;
    public DiagramNode source;
    public DiagramNode target;
}
```

## Step 2
The diagram objects are converted into SmGraph vertices which may be one of the following types (as of StateSmith 0.7.11):

![image](https://user-images.githubusercontent.com/274012/218194239-c69ed4e8-0526-4bd5-90de-6f4a7a4b0261.png)

```c#
public abstract class Vertex
{
    public string DiagramId;
    public Vertex? Parent;
    public List<Vertex> Children;
    public List<Behavior> Behaviors;
    public List<Behavior> IncomingTransitions;
}
```

## Step 3
The root vertices are then searched for the StateMachine Vertex to use (diagrams can define more than one).

## Step 4
The StateMachine Vertex then goes through a transformation pipeline ([SmTransformer](https://github.com/StateSmith/StateSmith/blob/24d57b3bf5bebea3d1b2a494521da28a64159d91/src/StateSmith/Runner/StandardSmTransformer.cs)) to support various features and get ready for code generation.

The transformation pipeline can easily be modified by user scripts. This allows you to add custom features, validations, whatever you might need. [This example project](https://github.com/StateSmith/StateSmith-examples/tree/main/modding-logging) shows how to easily add custom logging to specific states.

1. `RemoveNotesVertices`
1. `SupportRenderConfigVerticesAndRemove`
1. `SupportParentAlias`
1. `SupportEntryExit`
1. `SupportPrefixingModder`
1. `SupportHistory`
1. `SupportOrderAndElse`
1. `Validation1`
1. `DefaultUnspecifiedEventsAsDoEvent`
1. `AddUsedEventsToSm`
1. `FinalValidation`

## Step 5
Once the StateMachine (and its child vertices) is finished being transformed, it is passed to the code generation stage which plops out ready to run code.

## Deep User Customization
The core of StateSmith has been modified to use Dependency Injection so that users can swap in their own code generation classes, or customize almost any aspect of how StateSmith works.

More documentation will be coming for this. If you are keen now, hit us up on [discord](https://discord.com/channels/1056394875278462996/1056394875278462999).
