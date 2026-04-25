using FluentAssertions;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Output.BalancedCoder1;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/538
/// </summary>
public class SubtreeIdMapperTests
{
    /*
    Diagram text for test hierarchy below
    @startuml
    skinparam defaultFontName "Consolas"
    
    state ROOT
    ROOT : id = 0\nendId = 16

    state Animal
    Animal : id = 1\nendId = 7

    state Canine
    Canine : id = 2\nendId = 4

    state Dog
    Dog : id = 3\nendId = 3

    state Wolf
    Wolf : id = 4\nendId = 4

    state Feline
    Feline : id = 5\nendId = 7

    state Lion
    Lion : id = 6\nendId = 6

    state Panther
    Panther : id = 7\nendId = 7

    state Plant
    Plant : id = 8\nendId = 16

    state Flower
    Flower : id = 9\nendId = 12

    state Rose
    Rose : id = 10\nendId = 12

    state RedRose
    RedRose : id = 11\nendId = 11

    state WhiteRose
    WhiteRose : id = 12\nendId = 12

    state Tree
    Tree : id = 13\nendId = 16

    state Oak
    Oak : id = 14\nendId = 16

    state RedOak
    RedOak : id = 15\nendId = 15

    state WhiteOak
    WhiteOak : id = 16\nendId = 16

    ROOT --> Animal
    ROOT --> Plant

    Animal --> Canine
    Animal --> Feline

    Canine --> Dog
    Canine --> Wolf

    Feline --> Lion
    Feline --> Panther

    Plant --> Flower
    Plant --> Tree

    Flower --> Rose
    Rose --> RedRose
    Rose --> WhiteRose

    Tree --> Oak
    Oak --> RedOak
    Oak --> WhiteOak

    @enduml

    @startuml MySm
    [*] -> Animal
    state Animal {
        state Canine {
            state Dog
            state Wolf
        }
        state Feline {
            state Lion
            state Panther
        }
    }

    state Plant {
        state Flower {
            state Rose {
                state RedRose
                state WhiteRose
            }
        }
        state Tree {
            state Oak {
                state RedOak
                state WhiteOak
            }
        }
    }
    @enduml

    */
    [Fact]
    public void Test1()
    {
        // NOTE that graph is indexed depth first search, but children are sorted by name
        StateMachine sm = new("MySm1");
        var animal = sm.AddChild(new State("Animal"));
            var feline = animal.AddChild(new State("Feline"));
                var lion = feline.AddChild(new State("Lion"));
                var panther = feline.AddChild(new State("Panther"));
            var canine = animal.AddChild(new State("Canine"));
                var wolf = canine.AddChild(new State("Wolf"));
                var dog = canine.AddChild(new State("Dog"));
        var plant = sm.AddChild(new State("Plant"));
            var tree = plant.AddChild(new State("Tree"));
                var oak = tree.AddChild(new State("Oak"));
                    var redOak = oak.AddChild(new State("RedOak"));
                    var whiteOak = oak.AddChild(new State("WhiteOak"));
            var flower = plant.AddChild(new State("Flower"));
                var rose = flower.AddChild(new State("Rose"));
                    var whiteRose = rose.AddChild(new State("WhiteRose"));
                    var redRose = rose.AddChild(new State("RedRose"));
        
        SubtreeIdMapper mapper = new();
        var map = mapper.MapSubtree(sm);

        void Expect(NamedVertex vertex, int id, NamedVertex subtreeEndVertex, int subtreeEndId)
        {
            SubtreeIdMapper.SubtreeData subtreeData = map[vertex];
            subtreeData.vertex.Should().Be(vertex);
            subtreeData.id.Should().Be(id);
            subtreeData.subtreeEndVertex.Should().Be(subtreeEndVertex);
            subtreeData.subtreeEndId.Should().Be(subtreeEndId);
        }

        Expect(vertex: sm, id: 0, subtreeEndVertex: whiteOak, subtreeEndId: 16);

        Expect(vertex: animal, id: 1, subtreeEndVertex: panther, subtreeEndId: 7);
        Expect(vertex: canine, id: 2, subtreeEndVertex: wolf, subtreeEndId: 4);
        Expect(vertex: dog, id: 3, subtreeEndVertex: dog, subtreeEndId: 3);
        Expect(vertex: wolf, id: 4, subtreeEndVertex: wolf, subtreeEndId: 4);

        Expect(vertex: feline, id: 5, subtreeEndVertex: panther, subtreeEndId: 7);
        Expect(vertex: lion, id: 6, subtreeEndVertex: lion, subtreeEndId: 6);
        Expect(vertex: panther, id: 7, subtreeEndVertex: panther, subtreeEndId: 7);

        Expect(vertex: plant, id: 8, subtreeEndVertex: whiteOak, subtreeEndId: 16);
        Expect(vertex: flower, id: 9, subtreeEndVertex: whiteRose, subtreeEndId: 12);
        Expect(vertex: rose, id: 10, subtreeEndVertex: whiteRose, subtreeEndId: 12);
        Expect(vertex: redRose, id: 11, subtreeEndVertex: redRose, subtreeEndId: 11);
        Expect(vertex: whiteRose, id: 12, subtreeEndVertex: whiteRose, subtreeEndId: 12);

        Expect(vertex: tree, id: 13, subtreeEndVertex: whiteOak, subtreeEndId: 16);
        Expect(vertex: oak, id: 14, subtreeEndVertex: whiteOak, subtreeEndId: 16);
        Expect(vertex: redOak, id: 15, subtreeEndVertex: redOak, subtreeEndId: 15);
        Expect(vertex: whiteOak, id: 16, subtreeEndVertex: whiteOak, subtreeEndId: 16);
    }
}
