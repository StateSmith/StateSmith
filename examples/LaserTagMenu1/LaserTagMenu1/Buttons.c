#include "Buttons.h"
#include "PortApi.h"
#include <stddef.h>
#include <assert.h>

////////////////////////////////////////////////////////////////////////////////
// defines
////////////////////////////////////////////////////////////////////////////////

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x)/sizeof(0[x])) / ((size_t)(!(sizeof(x) % sizeof(0[x])))))


////////////////////////////////////////////////////////////////////////////////
// PUBLIC/EXTERN global vars
////////////////////////////////////////////////////////////////////////////////

struct ButtonSm1 Buttons_sms[ButtonIdCount];


////////////////////////////////////////////////////////////////////////////////
// global vars
////////////////////////////////////////////////////////////////////////////////

// setup input pins for buttons
static const uint8_t g_button_pins[] = { 
  [ButtonId_BACK] = 5,
  [ButtonId_UP] = 4,
  [ButtonId_DOWN] = 3,
  [ButtonId_OK] = 2
};
static_assert(COUNT_OF(g_button_pins) == ButtonIdCount, "button mapping needs updating");


////////////////////////////////////////////////////////////////////////////////
// public functions
////////////////////////////////////////////////////////////////////////////////

void Buttons_setup()
{
  for (uint8_t i = 0; i < ButtonIdCount; i++)
  {
    PortApi_enable_pullup(g_button_pins[i]);
    static_assert(COUNT_OF(g_button_pins) == ButtonIdCount, "required for safe array access");

    ButtonSm1_ctor(&Buttons_sms[i]);
    ButtonSm1_start(&Buttons_sms[i]);
    static_assert(COUNT_OF(Buttons_sms) == ButtonIdCount, "required for safe array access");
  }
}

void Buttons_step()
{
  for (uint8_t i = 0; i < ButtonIdCount; i++)
  {
    ButtonSm1 * const sm = &Buttons_sms[i];
    static_assert(COUNT_OF(Buttons_sms) == ButtonIdCount, "required for safe array access");

    sm->vars.input_is_pressed = !PortApi_digital_read(g_button_pins[i]);
    static_assert(COUNT_OF(g_button_pins) == ButtonIdCount, "required for safe array access");

    ButtonSm1_dispatch_event(sm, ButtonSm1_EventId_DO);
  }
}

