// Autogenerated with StateSmith 0.9.7-alpha.
// Algorithm: Balanced1. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

#pragma once
#include <stdint.h>

typedef enum a1b_EventId
{
    a1b_EventId_DO = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.
    a1b_EventId_MY_EVENT_1 = 1,
    a1b_EventId_MY_EVENT_2 = 2,
} a1b_EventId;

enum
{
    a1b_EventIdCount = 3
};

typedef enum a1b_StateId
{
    a1b_StateId_ROOT = 0,
    a1b_StateId_STATE_1 = 1,
    a1b_StateId_STATE_2 = 2,
} a1b_StateId;

enum
{
    a1b_StateIdCount = 3
};


// Generated state machine
// forward declaration
typedef struct a1b a1b;

// event handler type
typedef void (*a1b_Func)(a1b* sm);

// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
void a1b_ctor(a1b* sm);

// Starts the state machine. Must be called before dispatching events. Not thread safe.
void a1b_start(a1b* sm);

// Dispatches an event to the state machine. Not thread safe.
void a1b_dispatch_event(a1b* sm, a1b_EventId event_id);

// Thread safe.
char const * a1b_state_id_to_string(a1b_StateId id);

// Thread safe.
char const * a1b_event_id_to_string(a1b_EventId id);

// Generated state machine
struct a1b
{
    // Used internally by state machine. Feel free to inspect, but don't modify.
    a1b_StateId state_id;
    
    // Used internally by state machine. Don't modify.
    a1b_Func ancestor_event_handler;
    
    // Used internally by state machine. Don't modify.
    a1b_Func current_event_handlers[a1b_EventIdCount];
    
    // Used internally by state machine. Don't modify.
    a1b_Func current_state_exit_handler;
};
