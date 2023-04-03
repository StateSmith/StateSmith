// Autogenerated with StateSmith 0.8.12-alpha.
// Algorithm: Balanced1. See https://github.com/StateSmith/StateSmith/wiki/Algorithms

#include "blinky1_printf_sm.h"
// this ends up in the generated c file
#include "app_timer.h"
#include "led.h"
#include <stdbool.h> // required for `consume_event` flag
#include <string.h> // for memset

// This function is used when StateSmith doesn't know what the active leaf state is at
// compile time due to sub states or when multiple states need to be exited.
static void exit_up_to_state_handler(blinky1_printf_sm* sm, blinky1_printf_sm_func_t desired_state_exit_handler);

static void ROOT_enter(blinky1_printf_sm* sm);

static void ROOT_exit(blinky1_printf_sm* sm);

static void LED_OFF_enter(blinky1_printf_sm* sm);

static void LED_OFF_exit(blinky1_printf_sm* sm);

static void LED_OFF_do(blinky1_printf_sm* sm);

static void LED_ON_enter(blinky1_printf_sm* sm);

static void LED_ON_exit(blinky1_printf_sm* sm);

static void LED_ON_do(blinky1_printf_sm* sm);


// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.
void blinky1_printf_sm_ctor(blinky1_printf_sm* sm) {
    memset(sm, 0, sizeof(*sm));
}

// Starts the state machine. Must be called before dispatching events. Not thread safe.
void blinky1_printf_sm_start(blinky1_printf_sm* sm) {
  ROOT_enter(sm);
  // ROOT behavior
  // uml: TransitionTo(ROOT.InitialState)
 {
    // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
    
    // Step 2: Transition action: ``.
    
    // Step 3: Enter/move towards transition target `ROOT.InitialState`.
    // ROOT.InitialState is a pseudo state and cannot have an `enter` trigger.
    
    // ROOT.InitialState behavior
    // uml: TransitionTo(LED_OFF)
 {
      // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition). Already at LCA, no exiting required.
      
      // Step 2: Transition action: ``.
      
      // Step 3: Enter/move towards transition target `LED_OFF`.
      LED_OFF_enter(sm);
      
      // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
      sm->state_id = BLINKY1_PRINTF_SM_STATE_ID_LED_OFF;
      // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
      return;
    } // end of behavior for ROOT.InitialState
  } // end of behavior for ROOT
}

// Dispatches an event to the state machine. Not thread safe.
void blinky1_printf_sm_dispatch_event(blinky1_printf_sm* sm, blinky1_printf_sm_event_id_t event_id) {
  blinky1_printf_sm_func_t behavior_func = sm->current_event_handlers[event_id];
  
  while (behavior_func != NULL) {
    sm->ancestor_event_handler = NULL;
    behavior_func(sm);
    behavior_func = sm->ancestor_event_handler;
  }
}

// This function is used when StateSmith doesn't know what the active leaf state is at
// compile time due to sub states or when multiple states need to be exited.
static void exit_up_to_state_handler(blinky1_printf_sm* sm, blinky1_printf_sm_func_t desired_state_exit_handler) {
  while (sm->current_state_exit_handler != desired_state_exit_handler) {
    sm->current_state_exit_handler(sm);
  }
}


////////////////////////////////////////////////////////////////////////////////
// event handlers for state ROOT
////////////////////////////////////////////////////////////////////////////////

static void ROOT_enter(blinky1_printf_sm* sm) {
  // setup trigger/event handlers
  sm->current_state_exit_handler = ROOT_exit;
}

static void ROOT_exit(blinky1_printf_sm* sm) {
  // State machine root is a special case. It cannot be exited. Mark as unused.
  (void)sm;
}


////////////////////////////////////////////////////////////////////////////////
// event handlers for state LED_OFF
////////////////////////////////////////////////////////////////////////////////

static void LED_OFF_enter(blinky1_printf_sm* sm) {
  // setup trigger/event handlers
  sm->current_state_exit_handler = LED_OFF_exit;
  sm->current_event_handlers[BLINKY1_PRINTF_SM_EVENT_ID_DO] = LED_OFF_do;
  
  // LED_OFF behavior
  // uml: enter / { turn_led_off(); }
 {
    // Step 1: execute action `turn_led_off();`
    led_turn_off();;
  } // end of behavior for LED_OFF
  
  // LED_OFF behavior
  // uml: enter / { reset_timer(); }
 {
    // Step 1: execute action `reset_timer();`
    sm->vars.timer_started_at_ms = app_timer_get_ms();
  } // end of behavior for LED_OFF
}

static void LED_OFF_exit(blinky1_printf_sm* sm) {
  // adjust function pointers for this state's exit
  sm->current_state_exit_handler = ROOT_exit;
  sm->current_event_handlers[BLINKY1_PRINTF_SM_EVENT_ID_DO] = NULL;  // no ancestor listens to this event
}

static void LED_OFF_do(blinky1_printf_sm* sm) {
  // No ancestor state handles `do` event.
  
  // LED_OFF behavior
  // uml: do [after_ms(500)] TransitionTo(LED_ON)
  if (( (app_timer_get_ms() - sm->vars.timer_started_at_ms) >= 500 ))
 {
    // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
    LED_OFF_exit(sm);
    
    // Step 2: Transition action: ``.
    
    // Step 3: Enter/move towards transition target `LED_ON`.
    LED_ON_enter(sm);
    
    // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
    sm->state_id = BLINKY1_PRINTF_SM_STATE_ID_LED_ON;
    // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
    return;
  } // end of behavior for LED_OFF
}

////////////////////////////////////////////////////////////////////////////////
// event handlers for state LED_ON
////////////////////////////////////////////////////////////////////////////////

static void LED_ON_enter(blinky1_printf_sm* sm) {
  // setup trigger/event handlers
  sm->current_state_exit_handler = LED_ON_exit;
  sm->current_event_handlers[BLINKY1_PRINTF_SM_EVENT_ID_DO] = LED_ON_do;
  
  // LED_ON behavior
  // uml: enter / { turn_led_on();\nreset_timer(); }
 {
    // Step 1: execute action `turn_led_on();\nreset_timer();`
    led_turn_on();;
    sm->vars.timer_started_at_ms = app_timer_get_ms();
  } // end of behavior for LED_ON
}

static void LED_ON_exit(blinky1_printf_sm* sm) {
  // adjust function pointers for this state's exit
  sm->current_state_exit_handler = ROOT_exit;
  sm->current_event_handlers[BLINKY1_PRINTF_SM_EVENT_ID_DO] = NULL;  // no ancestor listens to this event
}

static void LED_ON_do(blinky1_printf_sm* sm) {
  // No ancestor state handles `do` event.
  
  // LED_ON behavior
  // uml: do [elapsed_ms > 1000] TransitionTo(LED_OFF)
  if ((app_timer_get_ms() - sm->vars.timer_started_at_ms) > 1000)
 {
    // Step 1: Exit states until we reach `ROOT` state (Least Common Ancestor for transition).
    LED_ON_exit(sm);
    
    // Step 2: Transition action: ``.
    
    // Step 3: Enter/move towards transition target `LED_OFF`.
    LED_OFF_enter(sm);
    
    // Step 4: complete transition. Ends event dispatch. No other behaviors are checked.
    sm->state_id = BLINKY1_PRINTF_SM_STATE_ID_LED_OFF;
    // No ancestor handles event. Can skip nulling `ancestor_event_handler`.
    return;
  } // end of behavior for LED_ON
}
// Thread safe.
char const * blinky1_printf_sm_state_id_to_string(blinky1_printf_sm_state_id_t id) {
  switch (id) {
    case BLINKY1_PRINTF_SM_STATE_ID_ROOT: return "ROOT";
    case BLINKY1_PRINTF_SM_STATE_ID_LED_OFF: return "LED_OFF";
    case BLINKY1_PRINTF_SM_STATE_ID_LED_ON: return "LED_ON";
    default: return "?";
  }
}

// Thread safe.
char const * blinky1_printf_sm_event_id_to_string(blinky1_printf_sm_event_id_t id) {
  switch (id) {
    case BLINKY1_PRINTF_SM_EVENT_ID_DO: return "BLINKY1_PRINTF_SM_EVENT_ID_DO";
    default: return "?";
  }
}
