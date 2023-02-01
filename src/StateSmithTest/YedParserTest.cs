using StateSmith;
using StateSmith.Input.Yed;
using StateSmith.Input;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using System.Collections.Generic;

namespace StateSmithTest
{
    public class YedParserTest
    {
        ITestOutputHelper output;
        Dictionary<string, DiagramNode> nodeMap;
        YedParser parser = new YedParser();


        public YedParserTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/ButtonSm1.graphml";

        private DiagramNode GetAndAssertNode(string id, string label, int childCount, DiagramNode parent)
        {
            var node = nodeMap[id];
            node.label.Trim().Should().Be(label);
            node.children.Count.Should().Be(childCount);
            node.parent.Should().Be(parent);

            if (parent != null)
            {
                parent.children.Should().Contain(node);
            }

            return node;
        }

        [Fact]
        public void TestParser()
        {
            //handy when printing what nodes are
            //var converter = new ConsoleCaptureConverter(output);
            //Console.SetOut(converter);

            parser.Parse(filepath);
            nodeMap = parser.nodeMap;

            GetAndAssertNode(id: "n0", label: "$STATEMACHINE: ButtonSm1", childCount: 3, parent: null);

            // todolow port more from old tests
        }
    }

}
