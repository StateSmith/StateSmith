//todo_low rework to test new Balanced1 code

//using Xunit;

//namespace StateSmithTest.C99BalancedCoder1
//{
//    public class CHeaderBuilderTest
//    {
//        [Fact]
//        public void OutputTypedefs_Test()
//        {
//            var ctx = ExamplesTestHelpers.SetupCtxForTiny2Sm();
//            CHeaderBuilder builder = new(ctx);
//            OutputFile file = new(ctx, ctx.hFileSb);

//            builder.OutputTypedefs(file);

//            string expected =
//@"typedef struct Tiny2 Tiny2;
//typedef void (*Tiny2_Func)(Tiny2* sm);
//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputStructDefinition_Test()
//        {
//            var ctx = ExamplesTestHelpers.SetupCtxForTiny2Sm();
//            CHeaderBuilder builder = new CHeaderBuilder(ctx);
//            OutputFile file = new(ctx, ctx.hFileSb);

//            builder.OutputStructDefinition(file);

//            string expected =
//@"struct Tiny2
//{
//    // Used internally by state machine. Feel free to inspect, but don't modify.
//    enum Tiny2_StateId state_id;

//    // Used internally by state machine. Don't modify.
//    Tiny2_Func ancestor_event_handler;

//    // Used internally by state machine. Don't modify.
//    Tiny2_Func current_event_handlers[Tiny2_EventIdCount];

//    // Used internally by state machine. Don't modify.
//    Tiny2_Func current_state_exit_handler;
//};
//".ConvertLineEndingsToN();

//            // could also add vars for testing

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }

//        [Fact]
//        public void OutputFunctionPrototypes_Test()
//        {
//            var ctx = ExamplesTestHelpers.SetupCtxForTiny2Sm();
//            CHeaderBuilder builder = new CHeaderBuilder(ctx);
//            OutputFile file = new(ctx, ctx.hFileSb);

//            builder.OutputFunctionPrototypes(file);

//            string expected =
//@"// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
//void Tiny2_ctor(Tiny2* self);

//// Starts the state machine. Must be called before dispatching events. Not thread safe.
//void Tiny2_start(Tiny2* self);

//// Dispatches an event to the state machine. Not thread safe.
//void Tiny2_dispatch_event(Tiny2* self, enum Tiny2_EventId event_id);

//// Converts a state id to a string. Thread safe.
//const char* Tiny2_state_id_to_string(const enum Tiny2_StateId id);
//".ConvertLineEndingsToN();

//            string code = file.ToString();
//            Assert.Equal(expected, code);
//        }
//    }
//}
