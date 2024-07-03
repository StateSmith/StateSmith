using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using StateSmith.Output.Gil;
using Xunit;
using System;
using FluentAssertions;
using StateSmith.Output;

namespace StateSmithTest.Output.Gil;

public class GilHelperTests
{
    [Fact]
    public void GilHelperCompile()
    {
        Action a = () => new RoslynCompiler().Compile(GilCode, out CompilationUnitSyntax _, out SemanticModel _);
        a.Should().Throw<TranspilerException>().WithMessage("*CS0102*").WithMessage("*CS0229*");

        //StateSmith.Output.TranspilerException : (21, 19): error CS0102: The type 'blinky1_printf_sm' already contains a definition for 'state_id'
        //(68, 12): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(68, 23): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(158, 8): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(158, 19): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(205, 8): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(205, 19): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(213, 10): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(214, 10): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
        //(215, 10): error CS0229: Ambiguity between 'blinky1_printf_sm.state_id' and 'blinky1_printf_sm.state_id'
    }

    const string GilCode = """
        // Generated state machine
        public class blinky1_printf_sm {
        public enum event_id {
          EVENT_ID_DO = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.
        }

        public const int EVENT_ID_COUNT = 1;

        public enum state_id {
          STATE_ID_ROOT = 0,
          STATE_ID_LED_OFF = 1,
          STATE_ID_LED_ON = 2,
        }

        public const int STATE_ID_COUNT = 3;

        // event handler type
        private delegate void blinky1_printf_sm_func(blinky1_printf_sm sm);

          // Used internally by state machine. Feel free to inspect, but don't modify.
          public state_id state_id;

          // Used internally by state machine. Don't modify.
          private blinky1_printf_sm_func? ancestor_event_handler;

          // Used internally by state machine. Don't modify.
          private readonly blinky1_printf_sm_func?[] current_event_handlers = new blinky1_printf_sm_func[EVENT_ID_COUNT];

          // Used internally by state machine. Don't modify.
          private blinky1_printf_sm_func? current_state_exit_handler;

        // State machine variables. Can be used for inputs, outputs, user variables...
        public struct Vars {
          //>>>>>ECHO:uint32_t timer_started_at_ms;  // milliseconds
        }

          // Variables. Can be used for inputs, outputs, user variables...
          public Vars vars;

        // State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
        public blinky1_printf_sm() {
        }

        // Starts the state machine. Must be called before dispatching events. Not thread safe.
        public void start() {
          ROOT_enter(this);
          // ROOT behavior
          // uml: TransitionTo(ROOT.<InitialState>)
         {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `ROOT.<InitialState>`.
            // ROOT.<InitialState> is a pseudo state and cannot have an `enter` trigger.

            // ROOT.<InitialState> behavior
            // uml: TransitionTo(LED_OFF)
         {
              // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

              // Step 2: Transition action: ``.

              // Step 3: Enter/move towards transition target `LED_OFF`.
              LED_OFF_enter(this);

              // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
              this.state_id = state_id.STATE_ID_LED_OFF;
              // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
              return;
            } // end of behavior for ROOT.<InitialState>
          } // end of behavior for ROOT
        }

        // Dispatches an event to the state machine. Not thread safe.
        public void dispatch_event(event_id event_id) {
          blinky1_printf_sm_func? behavior_func = this.current_event_handlers[(int)event_id];

          while (behavior_func != null) {
            this.ancestor_event_handler = null;
            behavior_func(this);
            behavior_func = this.ancestor_event_handler;
          }
        }

        // This function is used when StateSmith doesn't know what the active leaf state is at
        // compile time due to sub states or when multiple states need to be exited.
        private static void exit_up_to_state_handler(blinky1_printf_sm sm, blinky1_printf_sm_func desired_state_exit_handler) {
          while (sm.current_state_exit_handler != desired_state_exit_handler) {
            sm.current_state_exit_handler!(sm);
          }
        }


        ////////////////////////////////////////////////////////////////////////////////
        // event handlers for state ROOT
        ////////////////////////////////////////////////////////////////////////////////

        private static void ROOT_enter(blinky1_printf_sm sm) {
          // setup trigger/event handlers
          sm.current_state_exit_handler = ROOT_exit;
        }

        [____GilNoEmit_GilAddessableFunction<blinky1_printf_sm_func>]
        private static void ROOT_exit(blinky1_printf_sm sm) {
          // State machine root is a special case. It cannot be exited. Mark as unused.
          ____GilNoEmit_GilUnusedVar(sm);
        }


        ////////////////////////////////////////////////////////////////////////////////
        // event handlers for state LED_OFF
        ////////////////////////////////////////////////////////////////////////////////

        private static void LED_OFF_enter(blinky1_printf_sm sm) {
          // setup trigger/event handlers
          sm.current_state_exit_handler = LED_OFF_exit;
          sm.current_event_handlers[(int)event_id.EVENT_ID_DO] = LED_OFF_do;

          // LED_OFF behavior
          // uml: enter / { turn_led_off(); }
         {
            // Step 1: execute action `turn_led_off();`
            //>>>>>ECHO:led_turn_off();;
          } // end of behavior for LED_OFF

          // LED_OFF behavior
          // uml: enter / { reset_timer(); }
         {
            // Step 1: execute action `reset_timer();`
            //>>>>>ECHO:sm->vars.timer_started_at_ms = app_timer_get_ms();
          } // end of behavior for LED_OFF
        }

        [____GilNoEmit_GilAddessableFunction<blinky1_printf_sm_func>]
        private static void LED_OFF_exit(blinky1_printf_sm sm) {
          // adjust function pointers for this state's exit
          sm.current_state_exit_handler = ROOT_exit;
          sm.current_event_handlers[(int)event_id.EVENT_ID_DO] = null;  // no ancestor listens to this event
        }

        [____GilNoEmit_GilAddessableFunction<blinky1_printf_sm_func>]
        private static void LED_OFF_do(blinky1_printf_sm sm) {
          // No ancestor state handles `do` event.

          // LED_OFF behavior
          // uml: do [after_ms(500)] TransitionTo(LED_ON)
          if (____GilNoEmit_echoStringBool("( (app_timer_get_ms() - sm->vars.timer_started_at_ms) >= 500 )")) {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            LED_OFF_exit(sm);

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `LED_ON`.
            LED_ON_enter(sm);

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            sm.state_id = state_id.STATE_ID_LED_ON;
            // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
            return;
          } // end of behavior for LED_OFF
        }

        ////////////////////////////////////////////////////////////////////////////////
        // event handlers for state LED_ON
        ////////////////////////////////////////////////////////////////////////////////

        private static void LED_ON_enter(blinky1_printf_sm sm) {
          // setup trigger/event handlers
          sm.current_state_exit_handler = LED_ON_exit;
          sm.current_event_handlers[(int)event_id.EVENT_ID_DO] = LED_ON_do;

          // LED_ON behavior
          // uml: enter / { turn_led_on();\nreset_timer(); }
         {
            // Step 1: execute action `turn_led_on();\nreset_timer();`
            //>>>>>ECHO:led_turn_on();;
            //>>>>>ECHO:sm->vars.timer_started_at_ms = app_timer_get_ms();
          } // end of behavior for LED_ON
        }

        [____GilNoEmit_GilAddessableFunction<blinky1_printf_sm_func>]
        private static void LED_ON_exit(blinky1_printf_sm sm) {
          // adjust function pointers for this state's exit
          sm.current_state_exit_handler = ROOT_exit;
          sm.current_event_handlers[(int)event_id.EVENT_ID_DO] = null;  // no ancestor listens to this event
        }

        [____GilNoEmit_GilAddessableFunction<blinky1_printf_sm_func>]
        private static void LED_ON_do(blinky1_printf_sm sm) {
          // No ancestor state handles `do` event.

          // LED_ON behavior
          // uml: do [elapsed_ms > 1000] TransitionTo(LED_OFF)
          if (____GilNoEmit_echoStringBool("(app_timer_get_ms() - sm->vars.timer_started_at_ms) > 1000")) {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            LED_ON_exit(sm);

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `LED_OFF`.
            LED_OFF_enter(sm);

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            sm.state_id = state_id.STATE_ID_LED_OFF;
            // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
            return;
          } // end of behavior for LED_ON
        }
        // Thread safe.
        public static string state_id_to_string(state_id id) {
          switch (id) {
            case state_id.STATE_ID_ROOT: return "ROOT";
            case state_id.STATE_ID_LED_OFF: return "LED_OFF";
            case state_id.STATE_ID_LED_ON: return "LED_ON";
            default: return "?";
          }
        }

        // Thread safe.
        public static string event_id_to_string(event_id id) {
          switch (id) {
            case event_id.EVENT_ID_DO: return "EVENT_ID_DO";
            default: return "?";
          }
        }

          public static bool ____GilNoEmit_echoStringBool(string toEcho) { return true; }
          public static void ____GilNoEmit_GilUnusedVar(object unusedVar) { }
          public class ____GilNoEmit_GilAddessableFunction<T> : System.Attribute where T : System.Delegate {}
        }
        
        """;
}


