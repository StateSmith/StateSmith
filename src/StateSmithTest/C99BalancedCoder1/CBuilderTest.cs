//todo_low rework to test new Balanced1 code
//using Xunit;
//using System.Linq;
//using StateSmith.SmGraph;

//namespace StateSmithTest.C99BalancedCoder1
//{
//    public class CBuilderTest
//    {
//        CodeGenContext ctx;
//        CBuilder builder;
//        OutputFile file;

//        public CBuilderTest()
//        {
//            ctx = ExamplesTestHelpers.SetupCtxForSimple1();
//            builder = new(ctx);
//            file = new(ctx, ctx.cFileSb);
//        }

//        [Fact]
//        public void OutputTriggerHandlerPrototypes_Test()
//        {
//            builder.OutputTriggerHandlerPrototypes();

//            string expected =
//@"static void ROOT_enter(Simple1* self);
//static void ROOT_exit(Simple1* self);
//static void ROOT_do(Simple1* self);

//static void S1_enter(Simple1* self);
//static void S1_exit(Simple1* self);
//static void S1_do(Simple1* self);
//static void S1_ev1(Simple1* self);

//static void S1_1_enter(Simple1* self);
//static void S1_1_exit(Simple1* self);
//static void S1_1_zip(Simple1* self);

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputFuncCtor_Test()
//        {
//            builder.OutputFuncCtor();

//            string expected =
//@"void Simple1_ctor(Simple1* self)
//{
//    memset(self, 0, sizeof(*self));
//}

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact(Skip = "needs updating")]
//        public void OutputFuncStart_Test()
//        {
//            builder.OutputFuncStart();

//            string expected =
//@"void Simple1_start(Simple1* self)
//{
//    ROOT_enter(self);
//    self->state_id = Simple1_StateId_ROOT;
//}

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputFuncDispatchEvent_Test()
//        {
//            builder.OutputFuncDispatchEvent();

//            string expected =
//$@"void Simple1_dispatch_event(Simple1* self, enum Simple1_EventId event_id)
//{{
//    Simple1_Func behavior_func = self->current_event_handlers[event_id];
    
//    while (behavior_func != NULL)
//    {{
//        self->ancestor_event_handler = NULL;
//        behavior_func(self);
//        behavior_func = self->ancestor_event_handler;
//    }}
//}}

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputFuncStateEnter_Test()
//        {
//            var map = new NamedVertexMap(ctx.Sm);
//            builder.OutputFuncStateEnter(map.GetState("s1"));

//            string expected =
//$@"static void S1_enter(Simple1* self)
//{{
//    // setup trigger/event handlers
//    self->current_state_exit_handler = S1_exit;
//    self->current_event_handlers[Simple1_EventId_DO] = S1_do;
//    self->current_event_handlers[Simple1_EventId_EV1] = S1_ev1;
//}}

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputFuncStateExit_Test()
//        {
//            var map = new NamedVertexMap(ctx.Sm);
//            builder.OutputFuncStateExit(map.GetState("s1"));

//            string expected =
//$@"static void S1_exit(Simple1* self)
//{{
//    // adjust function pointers for this state's exit
//    self->current_state_exit_handler = ROOT_exit;
//    self->current_event_handlers[Simple1_EventId_DO] = ROOT_do;  // the next ancestor that handles this event is ROOT
//    self->current_event_handlers[Simple1_EventId_EV1] = NULL;  // no ancestor listens to this event
//}}

//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }
//    }
//}
