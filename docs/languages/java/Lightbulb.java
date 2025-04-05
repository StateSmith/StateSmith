// Autogenerated with StateSmith 0.17.3+9cafc79694e90b77d15218b578b2ab255e1a90a7.
// Algorithm: Balanced2. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

// Generated state machine
public class Lightbulb
{
    private LightbulbDelegate delegate;
    public Lightbulb(LightbulbDelegate delegate) {
        this.delegate = delegate;
    }
    public enum EventId
    {
        SWITCH,
    }
    
    public final int EventIdCount = 1;
    
    public enum StateId
    {
        ROOT,
        OFF,
        ON,
    }
    
    public final int StateIdCount = 3;
    
    // Used internally by state machine. Feel free to inspect, but don't modify.
    public StateId stateId;
    
    // State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
    public Lightbulb()
    {
    }
    
    // Starts the state machine. Must be called before dispatching events. Not thread safe.
    public void start()
    {
        ROOT_enter();
        // ROOT behavior
        // uml: TransitionTo(ROOT.<InitialState>)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `ROOT.<InitialState>`.
            // ROOT.<InitialState> is a pseudo state and cannot have an `enter` trigger.
            
            // ROOT.<InitialState> behavior
            // uml: TransitionTo(Off)
            {
                // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
                
                // Step 2: Transition action: ``.
                
                // Step 3: Enter/move towards transition target `Off`.
                OFF_enter();
                
                // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
                return;
            } // end of behavior for ROOT.<InitialState>
        } // end of behavior for ROOT
    }
    
    // Dispatches an event to the state machine. Not thread safe.
    // Note! This function assumes that the `eventId` parameter is valid.
    public void dispatchEvent(EventId eventId)
    {
        
        switch (this.stateId)
        {
            // STATE: Lightbulb
            case ROOT:
                // state and ancestors have no handler for `switch` event.
                break;
            
            // STATE: Off
            case OFF:
                OFF_switch(); 
                break;
            
            // STATE: On
            case ON:
                ON_switch(); 
                break;
        }
        
    }
    
    // This function is used when StateSmith doesn't know what the active leaf state is at
    // compile time due to sub states or when multiple states need to be exited.
    private void exitUpToStateHandler(StateId desiredState)
    {
        while (this.stateId != desiredState)
        {
            switch (this.stateId)
            {
                case OFF: OFF_exit(); break;
                
                case ON: ON_exit(); break;
                
                default: return;  // Just to be safe. Prevents infinite loop if state ID memory is somehow corrupted.
            }
        }
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ROOT
    ////////////////////////////////////////////////////////////////////////////////
    
    private void ROOT_enter()
    {
        this.stateId = StateId.ROOT;
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state OFF
    ////////////////////////////////////////////////////////////////////////////////
    
    private void OFF_enter()
    {
        this.stateId = StateId.OFF;
        
        // Off behavior
        // uml: enter / { delegate.enter_off(); }
        {
            // Step 1: execute action `delegate.enter_off();`
            delegate.enter_off();
        } // end of behavior for Off
    }
    
    private void OFF_exit()
    {
        this.stateId = StateId.ROOT;
    }
    
    private void OFF_switch()
    {
        // Off behavior
        // uml: Switch TransitionTo(On)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            OFF_exit();
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `On`.
            ON_enter();
            
            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for Off
        
        // No ancestor handles this event.
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ON
    ////////////////////////////////////////////////////////////////////////////////
    
    private void ON_enter()
    {
        this.stateId = StateId.ON;
        
        // On behavior
        // uml: enter / { delegate.enter_on(); }
        {
            // Step 1: execute action `delegate.enter_on();`
            delegate.enter_on();
        } // end of behavior for On
    }
    
    private void ON_exit()
    {
        this.stateId = StateId.ROOT;
    }
    
    private void ON_switch()
    {
        // On behavior
        // uml: Switch TransitionTo(Off)
        {
            // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
            ON_exit();
            
            // Step 2: Transition action: ``.
            
            // Step 3: Enter/move towards transition target `Off`.
            OFF_enter();
            
            // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
            return;
        } // end of behavior for On
        
        // No ancestor handles this event.
    }
    
    // Thread safe.
    public static String stateIdToString(StateId id)
    {
        switch (id)
        {
            case ROOT: return "ROOT";
            case OFF: return "OFF";
            case ON: return "ON";
            default: return "?";
        }
    }
    
    // Thread safe.
    public static String eventIdToString(EventId id)
    {
        switch (id)
        {
            case SWITCH: return "SWITCH";
            default: return "?";
        }
    }
}
