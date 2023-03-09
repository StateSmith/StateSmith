#nullable enable
// Generated state machine
public class CSharpNoNameSpaceExampleSm
{
    public enum EventId
    {
        DO = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.
    }

    public const int EventIdCount = 1;

    public enum StateId
    {
        ROOT = 0,
        STATE_1 = 1,
        STATE_2 = 2,
    }

    public const int StateIdCount = 3;

    // event handler type
    private delegate void Func(CSharpNoNameSpaceExampleSm sm);

    // Used internally by state machine. Feel free to inspect, but don't modify.
    public StateId state_id;

    // Used internally by state machine. Don't modify.
    private Func? ancestor_event_handler;

    // Used internally by state machine. Don't modify.
    private readonly Func?[] current_event_handlers = new Func[EventIdCount];

    // Used internally by state machine. Don't modify.
    private Func? current_state_exit_handler;

    // State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
    public CSharpNoNameSpaceExampleSm()
    {
    }

    // Starts the state machine. Must be called before dispatching events. Not thread safe.
    public void Start()
    {
        ROOT_enter(this);
        // ROOT behavior
        // uml: TransitionTo(ROOT.InitialState)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `ROOT.InitialState`.
            // ROOT.InitialState is a pseudo state and cannot have an `enter` trigger.

            // ROOT.InitialState behavior
            // uml: TransitionTo(STATE_1)
            {
                // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.

                // Step 2: Transition action: ``.

                // Step 3: Enter/move towards transition target `STATE_1`.
                STATE_1_enter(this);

                // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
                this.state_id = StateId.STATE_1;
                // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
                return;
            } // end of behavior for ROOT.InitialState
        } // end of behavior for ROOT
    }

    // Dispatches an event to the state machine. Not thread safe.
    public void DispatchEvent(EventId event_id)
    {
        Func? behavior_func = this.current_event_handlers[(int)event_id];

        while (behavior_func != null)
        {
            this.ancestor_event_handler = null;
            behavior_func(this);
            behavior_func = this.ancestor_event_handler;
        }
    }

    // This function is used when StateSmith doesn't know what the active leaf state is at
    // compile time due to sub states or when multiple states need to be exited.
    private static void ExitUpToStateHandler(CSharpNoNameSpaceExampleSm sm, Func desired_state_exit_handler)
    {
        while (sm.current_state_exit_handler != desired_state_exit_handler)
        {
            sm.current_state_exit_handler!(sm);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ROOT
    ////////////////////////////////////////////////////////////////////////////////

    private static void ROOT_enter(CSharpNoNameSpaceExampleSm sm)
    {
        // setup trigger/event handlers
        sm.current_state_exit_handler = ROOT_exit;
    }

    private static readonly Func ROOT_exit = (CSharpNoNameSpaceExampleSm sm) =>
    {
        // State machine root is a special case. It cannot be exited. Mark as unused.
        _ = sm;
    };


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state STATE_1
    ////////////////////////////////////////////////////////////////////////////////

    private static void STATE_1_enter(CSharpNoNameSpaceExampleSm sm)
    {
        // setup trigger/event handlers
        sm.current_state_exit_handler = STATE_1_exit;
        sm.current_event_handlers[(int)EventId.DO] = STATE_1_do;
    }

    private static readonly Func STATE_1_exit = (CSharpNoNameSpaceExampleSm sm) =>
    {
        // adjust function pointers for this state's exit
        sm.current_state_exit_handler = ROOT_exit;
        sm.current_event_handlers[(int)EventId.DO] = null;  // no ancestor listens to this event
    };

    private static readonly Func STATE_1_do = (CSharpNoNameSpaceExampleSm sm) =>
    {
        // No ancestor state handles `do` event.

        // STATE_1 behavior
        // uml: do TransitionTo(STATE_2)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            STATE_1_exit(sm);

            // Step 2: Transition action: ``.

            // Step 3: Enter/move towards transition target `STATE_2`.
            STATE_2_enter(sm);

            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            sm.state_id = StateId.STATE_2;
            // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
            return;
        } // end of behavior for STATE_1
    };


    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state STATE_2
    ////////////////////////////////////////////////////////////////////////////////

    private static void STATE_2_enter(CSharpNoNameSpaceExampleSm sm)
    {
        // setup trigger/event handlers
        sm.current_state_exit_handler = STATE_2_exit;
    }

    private static readonly Func STATE_2_exit = (CSharpNoNameSpaceExampleSm sm) =>
    {
        // adjust function pointers for this state's exit
        sm.current_state_exit_handler = ROOT_exit;
    };

    // Thread safe.
    public static string StateIdToString(StateId id)
    {
        switch (id)
        {
            case StateId.ROOT: return "ROOT";
            case StateId.STATE_1: return "STATE_1";
            case StateId.STATE_2: return "STATE_2";
            default: return "?";
        }
    }

    // Thread safe.
    public static string EventIdToString(EventId id)
    {
        switch (id)
        {
            case EventId.DO: return "DO";
            default: return "?";
        }
    }
}

