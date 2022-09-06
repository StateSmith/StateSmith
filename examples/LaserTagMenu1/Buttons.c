#include "Buttons.h"
#include "PortApi.h"
#include <stddef.h>
#include <assert.h>

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x)/sizeof(0[x])) / ((size_t)(!(sizeof(x) % sizeof(0[x])))))

// setup input pins for buttons
const uint8_t button_pins[] = { 
    [ButtonId_BACK] = 5,
    [ButtonId_UP] = 4,
    [ButtonId_DOWN] = 3,
    [ButtonId_OK] = 2
};
static_assert(COUNT_OF(button_pins) == ButtonIdCount, "button mapping needs updating");

// button state machines
struct ButtonSm1 g_button_sms[ButtonIdCount];


void Buttons_setup()
{
  for (uint8_t i = 0; i < ButtonIdCount; i++)
  {
    PortApi_enable_pullup(button_pins[i]);
    ButtonSm1_ctor(&g_button_sms[i]);
    ButtonSm1_start(&g_button_sms[i]);
  }
}

void Buttons_step()
{
  for (uint8_t i = 0; i < ButtonIdCount; i++)
  {
    ButtonSm1 * const sm = &g_button_sms[i];
    static_assert(COUNT_OF(g_button_sms) == ButtonIdCount, "required for safe array access");

    sm->vars.input_is_pressed = !PortApi_digital_read(button_pins[i]);
    static_assert(COUNT_OF(button_pins) == ButtonIdCount, "required for safe array access");

    ButtonSm1_dispatch_event(sm, ButtonSm1_EventId_DO);
  }
}

