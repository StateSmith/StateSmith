using StateSmith.SmGraph;
using StateSmith.Runner;
using Xunit;
using FluentAssertions;

namespace StateSmithTest
{
    public class PrefixingModderTests
    {
        StateMachine sm;
        State mainMenu;
        State mmSelectBeverage;
        State mmBevNone;
        State mmBevTea;
        State mmBevWater;
        State mmFood;
        State mmFoodNone;
        State mmFoodSludge;
        State mmFoodPorridge;
        State mmFoodBalls;
        PrefixingModder prefixingModder = new();

        public PrefixingModderTests()
        {
            BuildSm();
        }

        [Fact]
        public void WithoutPrefix()
        {
            // the below shouldn't match. No prefixing expected
            mainMenu.AddBehavior(new Behavior(trigger: "mod", actionCode: "prefix.auto()"));
            mainMenu.AddBehavior(new Behavior(trigger: "mod", actionCode: "$prefix.auto()"));
            mainMenu.AddBehavior(new Behavior(trigger: "$modd", actionCode: "prefix.auto()"));
            mainMenu.AddBehavior(new Behavior(trigger: "$mod", actionCode: "my_prefix.auto()"));
            mainMenu.AddBehavior(new Behavior(trigger: "$mod", actionCode: "prefix_add(blah)"));

            prefixingModder.Visit(sm);
            mmSelectBeverage.Name.Should().Be("SELECT_BEVERAGE");
            /**/mmBevNone.Name.Should().Be("NONE");
            /**/mmBevTea.Name.Should().Be("TEA");
            //--------------------------------------------------------------
            mmFood.Name.Should().Be("SELECT_FOOD");
            /**/mmFoodNone.Name.Should().Be("NONE");
            /**/mmFoodSludge.Name.Should().Be("SPACE_SLUDGE");
            /**/mmFoodBalls.Name.Should().Be("SPACE_BALLS");
        }

        [Fact]
        public void AutoAddSetPrefix()
        {
            mainMenu.AddBehavior(new Behavior(trigger:"$mod", actionCode: "prefix.auto()"));
            mmSelectBeverage.AddBehavior(new Behavior(trigger: "$mod", actionCode: "prefix.add(BEV)"));
            mmFood.AddBehavior(new Behavior(trigger: "$mod", actionCode: "prefix.set( SEL_FOOD )"));

            prefixingModder.Visit(sm);

            mmSelectBeverage.Name.Should().Be("MAIN_MENU__SELECT_BEVERAGE");
            /**/mmBevNone.Name.Should().Be("MAIN_MENU__BEV__NONE");
            /**/mmBevTea.Name.Should().Be("MAIN_MENU__BEV__TEA");
            //--------------------------------------------------------------
            mmFood.Name.Should().Be("MAIN_MENU__SELECT_FOOD");
            /**/mmFoodNone.Name.Should().Be("SEL_FOOD__NONE");
            /**/mmFoodSludge.Name.Should().Be("SEL_FOOD__SPACE_SLUDGE");
            /**/mmFoodBalls.Name.Should().Be("SEL_FOOD__SPACE_BALLS");
        }

        private void BuildSm()
        {
            sm = new StateMachine("sm");

            mainMenu = sm.AddChild(new State("MAIN_MENU"));
            sm.AddChild(new InitialState()).AddBehavior(new Behavior(transitionTarget: mainMenu));

            mmSelectBeverage = mainMenu.AddChild(new State("SELECT_BEVERAGE"));
            mmBevNone = mmSelectBeverage.AddChild(new State("NONE"));
            mmBevTea = mmSelectBeverage.AddChild(new State("TEA"));
            mmBevWater = mmSelectBeverage.AddChild(new State("WATER"));

            mmFood = mainMenu.AddChild(new State("SELECT_FOOD"));
            mmFoodNone = mmFood.AddChild(new State("NONE"));
            mmFoodSludge = mmFood.AddChild(new State("SPACE_SLUDGE"));
            mmFoodPorridge = mmFood.AddChild(new State("SPACE_PORRIDGE"));
            mmFoodBalls = mmFood.AddChild(new State("SPACE_BALLS"));
        }
    }
}
