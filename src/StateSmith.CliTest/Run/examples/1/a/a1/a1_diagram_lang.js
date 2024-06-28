// Autogenerated with StateSmith 0.11.1-alpha+64dc77b4b449b1d3e3859f6364af0387913932b5.
// Algorithm: Balanced1. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

// Generated state machine
class a1_diagram_lang
{
    static EventId = 
    {
        DO : 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.
        MY_EVENT_1 : 1,
        MY_EVENT_2 : 2,
    }
    static { Object.freeze(this.EventId); }
    
    static EventIdCount = 3;
    static { Object.freeze(this.EventIdCount); }
    
    static StateId = 
    {
        ROOT : 0,
        STATE_1 : 1,
        STATE_2 : 2,
    }
    static { Object.freeze(this.StateId); }
    
    static StateIdCount = 3;
    static { Object.freeze(this.StateIdCount); }
    
    // Used internally by state machine. Feel free to inspect, but don't modify.
    stateId;
    
    // Used internally by state machine. Don't modify.
    #ancestorEventHandler;
    
    // Used internally by state machine. Don't modify.
    #currentEventHandlers = Array(a1_diagram_lang.EventIdCount).fill(undefined);
    
    // Used internally by state machine. Don't modify.
    #currentStateExitHandler;
    
    // Starts the state machine. Must be called before dispatching events. Not thread safe.
    start()
    {
        this.#ROOT_enter();
        // ROOT behavior
        // uml: TransitionTo(ROOT.<InitialState>)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `ROOT.<InitialState>`.
            // ROOT.<InitialState> is a pseudo state and cannot have an `enter` trigger.
            
            // ROOT.<InitialState> behavior
            // uml: TransitionTo(STATE_1)
            {
                // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
                
                // Step 2: Transition action: ``.
                
                // Step 3: Enter/move towards transition target `STATE_1`.
                this.#STATE_1_enter();
                
                // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
                this.stateId = a1_diagram_lang.StateId.STATE_1;
                // No ancestor handles event. Can skip nulling `ancestorEventHandler`.
                return;
            } // end of behavior for ROOT.<InitialState>
        } // end of behavior for ROOT
    }
    
    // Dispatches an event to the state machine. Not thread safe.
    dispatchEvent(eventId)
    {
        let behaviorFunc = this.#currentEventHandlers[eventId];
        
        while (behaviorFunc != null)
        {
            this.#ancestorEventHandler = null;
            behaviorFunc.call(this);
            behaviorFunc = this.#ancestorEventHandler;
        }
    }
    
    // This function is used when StateSmith doesn't know what the active leaf state is at
    // compile time due to sub states or when multiple states need to be exited.
    #exitUpToStateHandler(desiredStateExitHandler)
    {
        while (this.#currentStateExitHandler != desiredStateExitHandler)
        {
            this.#currentStateExitHandler.call(this);
        }
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ROOT
    ////////////////////////////////////////////////////////////////////////////////
    
    #ROOT_enter()
    {
        // setup trigger/event handlers
        this.#currentStateExitHandler = this.#ROOT_exit;
    }
    
    #ROOT_exit()
    {
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state STATE_1
    ////////////////////////////////////////////////////////////////////////////////
    
    #STATE_1_enter()
    {
        // setup trigger/event handlers
        this.#currentStateExitHandler = this.#STATE_1_exit;
        this.#currentEventHandlers[a1_diagram_lang.EventId.DO] = this.#STATE_1_do;
        this.#currentEventHandlers[a1_diagram_lang.EventId.MY_EVENT_1] = this.#STATE_1_my_event_1;
        
        // STATE_1 behavior
        // uml: enter / { print("S1 enter"); }
        {
            // Step 1: execute action `print("S1 enter");`
            print("S1 enter");
        } // end of behavior for STATE_1
    }
    
    #STATE_1_exit()
    {
        // STATE_1 behavior
        // uml: exit / { print("S1 exit"); }
        {
            // Step 1: execute action `print("S1 exit");`
            print("S1 exit");
        } // end of behavior for STATE_1
        
        // adjust function pointers for this state's exit
        this.#currentStateExitHandler = this.#ROOT_exit;
        this.#currentEventHandlers[a1_diagram_lang.EventId.DO] = null;  // no ancestor listens to this event
        this.#currentEventHandlers[a1_diagram_lang.EventId.MY_EVENT_1] = null;  // no ancestor listens to this event
    }
    
    #STATE_1_do()
    {
        // No ancestor state handles `do` event.
        
        // STATE_1 behavior
        // uml: do / { print("S1 do"); }
        {
            // Step 1: execute action `print("S1 do");`
            print("S1 do");
            
            // Step 2: determine if ancestor gets to handle event next.
            // Don't consume special `do` event.
        } // end of behavior for STATE_1
    }
    
    #STATE_1_my_event_1()
    {
        // No ancestor state handles `my_event_1` event.
        
        // STATE_1 behavior
        // uml: MY_EVENT_1 TransitionTo(STATE_2)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            this.#STATE_1_exit();
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `STATE_2`.
            this.#STATE_2_enter();
            
            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            this.stateId = a1_diagram_lang.StateId.STATE_2;
            // No ancestor handles event. Can skip nulling `ancestorEventHandler`.
            return;
        } // end of behavior for STATE_1
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state STATE_2
    ////////////////////////////////////////////////////////////////////////////////
    
    #STATE_2_enter()
    {
        // setup trigger/event handlers
        this.#currentStateExitHandler = this.#STATE_2_exit;
        this.#currentEventHandlers[a1_diagram_lang.EventId.MY_EVENT_2] = this.#STATE_2_my_event_2;
        
        // STATE_2 behavior
        // uml: enter / { print("S2 enter"); }
        {
            // Step 1: execute action `print("S2 enter");`
            print("S2 enter");
        } // end of behavior for STATE_2
    }
    
    #STATE_2_exit()
    {
        // STATE_2 behavior
        // uml: exit / { print("S2 exit"); }
        {
            // Step 1: execute action `print("S2 exit");`
            print("S2 exit");
        } // end of behavior for STATE_2
        
        // adjust function pointers for this state's exit
        this.#currentStateExitHandler = this.#ROOT_exit;
        this.#currentEventHandlers[a1_diagram_lang.EventId.MY_EVENT_2] = null;  // no ancestor listens to this event
    }
    
    #STATE_2_my_event_2()
    {
        // No ancestor state handles `my_event_2` event.
        
        // STATE_2 behavior
        // uml: MY_EVENT_2 TransitionTo(STATE_1)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            this.#STATE_2_exit();
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `STATE_1`.
            this.#STATE_1_enter();
            
            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            this.stateId = a1_diagram_lang.StateId.STATE_1;
            // No ancestor handles event. Can skip nulling `ancestorEventHandler`.
            return;
        } // end of behavior for STATE_2
    }
    
    // Thread safe.
    static stateIdToString(id)
    {
        switch (id)
        {
            case a1_diagram_lang.StateId.ROOT: return "ROOT";
            case a1_diagram_lang.StateId.STATE_1: return "STATE_1";
            case a1_diagram_lang.StateId.STATE_2: return "STATE_2";
            default: return "?";
        }
    }
    
    // Thread safe.
    static eventIdToString(id)
    {
        switch (id)
        {
            case a1_diagram_lang.EventId.DO: return "DO";
            case a1_diagram_lang.EventId.MY_EVENT_1: return "MY_EVENT_1";
            case a1_diagram_lang.EventId.MY_EVENT_2: return "MY_EVENT_2";
            default: return "?";
        }
    }
}
