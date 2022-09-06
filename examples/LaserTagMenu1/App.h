#pragma once
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

enum PlayerClass
{
    PlayerClass_ENGINEER,
    PlayerClass_HEAVY,
    PlayerClass_ARCHER,
    PlayerClass_WIZARD,
    PlayerClass_SPY,
};

void App_setup();
void App_step();

void App_save_player_class(uint8_t class_id);

enum PlayerClass App_get_player_class(void);

#ifdef __cplusplus
}
#endif