#pragma once
#include "ButtonSm1.h"

#ifdef __cplusplus
extern "C" {
#endif

enum ButtonId
{
    ButtonId_DOWN,
    ButtonId_UP,
    ButtonId_OK,
    ButtonId_BACK,
    ButtonIdCount
};

// button state machines
extern struct ButtonSm1 g_button_sms[ButtonIdCount];


void Buttons_setup();
void Buttons_step();


#ifdef __cplusplus
}
#endif