#pragma once
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

////////////////////////////////////////////////////////////////////////////////
// enums
////////////////////////////////////////////////////////////////////////////////

enum PlayerClass
{
    PlayerClass_ENGINEER,
    PlayerClass_HEAVY,
    PlayerClass_ARCHER,
    PlayerClass_WIZARD,
    PlayerClass_SPY,
};


////////////////////////////////////////////////////////////////////////////////
// public functions
////////////////////////////////////////////////////////////////////////////////

// Must be called before other App functions are called.
void App_setup();

// call periodically
void App_step();

void App_save_player_class(uint8_t class_id);

enum PlayerClass App_get_player_class(void);

#ifdef __cplusplus
}
#endif