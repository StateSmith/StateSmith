// Autogenerated with StateSmith 0.17.5.
// Algorithm: Balanced2. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

#pragma once  // You can also specify normal include guard. See https://github.com/StateSmith/StateSmith/blob/main/docs/settings.md
#include <stdint.h>

class DefaultBaseClass {};

// Generated state machine
template <typename BaseClass = DefaultBaseClass>
class lightbulb : public BaseClass
{
public:
    enum class EventId: uint8_t
    {
        SWITCH = 0,
    };
    
    enum
    {
        EventIdCount = 1
    };
    
    enum class StateId: uint8_t
    {
        ROOT = 0,
        OFF = 1,
        ON = 2,
    };
    
    enum
    {
        StateIdCount = 3
    };
    
    // Used internally by state machine. Feel free to inspect, but don't modify.
    StateId stateId;
    
    // State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
    lightbulb()
    {
    }
    
    // Starts the state machine. Must be called before dispatching events. Not thread safe.
    void start();
    
    // Dispatches an event to the state machine. Not thread safe.
    // Note! This function assumes that the `eventId` parameter is valid.
    void dispatchEvent(EventId eventId);
    
    // Thread safe.
    static char const * stateIdToString(StateId id);
    
    // Thread safe.
    static char const * eventIdToString(EventId id);


// ################################### PRIVATE MEMBERS ###################################
private:
    
    // This function is used when StateSmith doesn't know what the active leaf state is at
    // compile time due to sub states or when multiple states need to be exited.
    void exitUpToStateHandler(StateId desiredState);
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ROOT
    ////////////////////////////////////////////////////////////////////////////////
    
    void ROOT_enter();
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state OFF
    ////////////////////////////////////////////////////////////////////////////////
    
    void OFF_enter();
    
    void OFF_exit();
    
    void OFF_switch();
    
    
    ////////////////////////////////////////////////////////////////////////////////
    // event handlers for state ON
    ////////////////////////////////////////////////////////////////////////////////
    
    void ON_enter();
    
    void ON_exit();
    
    void ON_switch();
};



// Autogenerated with StateSmith 0.17.5.
// Algorithm: Balanced2. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

#include "lightbulb.hpp"
#include <stdbool.h> // required for `consume_event` flag
#include <string.h> // for memset


// Starts the state machine. Must be called before dispatching events. Not thread safe.
template <typename BaseClass>
void lightbulb<BaseClass>::start()
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
template <typename BaseClass>
void lightbulb<BaseClass>::dispatchEvent(EventId eventId)
{
    
    switch (this->stateId)
    {
        // STATE: lightbulb
        case StateId::ROOT:
            // state and ancestors have no handler for `switch` event.
            break;
        
        // STATE: Off
        case StateId::OFF:
            OFF_switch(); 
            break;
        
        // STATE: On
        case StateId::ON:
            ON_switch(); 
            break;
    }
    
}

// This function is used when StateSmith doesn't know what the active leaf state is at
// compile time due to sub states or when multiple states need to be exited.
template <typename BaseClass>
void lightbulb<BaseClass>::exitUpToStateHandler(StateId desiredState)
{
    while (this->stateId != desiredState)
    {
        switch (this->stateId)
        {
            case StateId::OFF: OFF_exit(); break;
            
            case StateId::ON: ON_exit(); break;
            
            default: return;  // Just to be safe. Prevents infinite loop if state ID memory is somehow corrupted.
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// event handlers for state ROOT
////////////////////////////////////////////////////////////////////////////////

template <typename BaseClass>
void lightbulb<BaseClass>::ROOT_enter()
{
    this->stateId = StateId::ROOT;
}


////////////////////////////////////////////////////////////////////////////////
// event handlers for state OFF
////////////////////////////////////////////////////////////////////////////////

template <typename BaseClass>
void lightbulb<BaseClass>::OFF_enter()
{
    this->stateId = StateId::OFF;
    
    // Off behavior
    // uml: enter / { MyBaseClass::powerOff(); }
    {
        // Step 1: execute action `MyBaseClass::powerOff();`
        MyBaseClass::powerOff();
    } // end of behavior for Off
}

template <typename BaseClass>
void lightbulb<BaseClass>::OFF_exit()
{
    this->stateId = StateId::ROOT;
}

template <typename BaseClass>
void lightbulb<BaseClass>::OFF_switch()
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

template <typename BaseClass>
void lightbulb<BaseClass>::ON_enter()
{
    this->stateId = StateId::ON;
    
    // On behavior
    // uml: enter / { MyBaseClass::powerOn(); }
    {
        // Step 1: execute action `MyBaseClass::powerOn();`
        MyBaseClass::powerOn();
    } // end of behavior for On
}

template <typename BaseClass>
void lightbulb<BaseClass>::ON_exit()
{
    this->stateId = StateId::ROOT;
}

template <typename BaseClass>
void lightbulb<BaseClass>::ON_switch()
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
template <typename BaseClass>
char const * lightbulb<BaseClass>::stateIdToString(StateId id)
{
    switch (id)
    {
        case StateId::ROOT: return "ROOT";
        case StateId::OFF: return "OFF";
        case StateId::ON: return "ON";
        default: return "?";
    }
}

// Thread safe.
template <typename BaseClass>
char const * lightbulb<BaseClass>::eventIdToString(EventId id)
{
    switch (id)
    {
        case EventId::SWITCH: return "SWITCH";
        default: return "?";
    }
}
