//todo_low rework to test new Balanced1 code

//using FluentAssertions;
//using StateSmith.SmGraph;
//using StateSmith.Runner;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using static StateSmithTest.VertexTestHelper;

//namespace StateSmithTest.C99BalancedCoder1
//{
//    public class CEnumBuilderTest
//    {
//        CodeGenContext ctx;

//        private void SetupCtxForWaffleSm()
//        {
//            var inputSmBuilder = new InputSmBuilder();

//            StateMachine sm = new StateMachine("WaffleSm");
//            sm.AddBehavior(new Behavior()
//            {
//                triggers = TriggerList("do", "EV1", "EV2")
//            });

//            inputSmBuilder.SetStateMachineRoot(sm);
//            var s1 = sm.AddChild(new State(name: "s1"));
//            var initialStateVertex = sm.AddChild(new InitialState());
//            initialStateVertex.AddTransitionTo(s1);

//            inputSmBuilder.FinishRunning();
//            ctx = new(sm);
//        }

//        [Fact]
//        public void Test1()
//        {
//            SetupCtxForWaffleSm();

//            CEnumBuilder cEnumBuilder = new(ctx);
//            cEnumBuilder.OutputEventIdCode();
//            string expected =
//@"enum WaffleSm_EventId
//{
//    WaffleSm_EventId_DO = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.
//    WaffleSm_EventId_EV1 = 1,
//    WaffleSm_EventId_EV2 = 2,
//};

//enum
//{
//    WaffleSm_EventIdCount = 3
//};
//".ConvertLineEndingsToN();

//            string code = ctx.hFileSb.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void StateEnumTest()
//        {
//            ctx = ExamplesTestHelpers.SetupCtxForTiny2Sm();

//            CEnumBuilder cEnumBuilder = new(ctx);
//            cEnumBuilder.OutputStateIdCode();
//            string expected =
//@"enum Tiny2_StateId
//{
//    Tiny2_StateId_ROOT = 0,
//    Tiny2_StateId_S1 = 1,
//    Tiny2_StateId_S1_1 = 2,
//    Tiny2_StateId_S1_1_1 = 3,
//    Tiny2_StateId_S1_1_2 = 4,
//    Tiny2_StateId_S1_2 = 5,
//    Tiny2_StateId_S2 = 6,
//};

//enum
//{
//    Tiny2_StateIdCount = 7
//};
//".ConvertLineEndingsToN();

//            string code = ctx.hFileSb.ToString();
//            Assert.Equal(expected, code);
//        }
//    }
//}
