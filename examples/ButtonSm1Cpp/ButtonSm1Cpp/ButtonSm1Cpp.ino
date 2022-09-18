#include "ButtonSm1Cpp.h"
#include <assert.h>

////////////////////////////////////////////////////////////////////////////////
// defines
////////////////////////////////////////////////////////////////////////////////

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x) / sizeof(0 [x])) / ((size_t)(!(sizeof(x) % sizeof(0 [x])))))

#define BUTTON_COUNT 4


////////////////////////////////////////////////////////////////////////////////
// global vars
////////////////////////////////////////////////////////////////////////////////

// setup input pins for buttons
static const uint8_t g_button_pins[] = {7, 6, 5, 4};
static_assert(COUNT_OF(g_button_pins) == BUTTON_COUNT, "button mapping needs updating");

// button state machines
static struct ButtonSm1Cpp g_button_sms[BUTTON_COUNT];


////////////////////////////////////////////////////////////////////////////////
// functions
////////////////////////////////////////////////////////////////////////////////

void setup()
{
  Serial.begin(115200);
  Serial.println("Keyboard keys 1,2,3,4 have a binding to switches above when simulation pane has focus.");
  Serial.println();

  for (uint8_t i = 0; i < BUTTON_COUNT; i++)
  {
    pinMode(g_button_pins[i], INPUT_PULLUP);
    static_assert(COUNT_OF(g_button_pins) == BUTTON_COUNT, "required for safe array access");

    ButtonSm1Cpp_ctor(&g_button_sms[i]);
    ButtonSm1Cpp_start(&g_button_sms[i]);
    static_assert(COUNT_OF(g_button_sms) == BUTTON_COUNT, "required for safe array access");
  }
}

void loop()
{
  for (uint8_t i = 0; i < BUTTON_COUNT; i++)
  {
    ButtonSm1Cpp &sm = g_button_sms[i];
    sm.vars.input_is_pressed = (digitalRead(g_button_pins[i]) == LOW);
    static_assert(COUNT_OF(g_button_pins) == BUTTON_COUNT, "required for safe array access");

    ButtonSm1Cpp_dispatch_event(&sm, ButtonSm1Cpp_EventId_DO);

    const uint8_t button_label = i + 1;

    if (sm.vars.output_event_press)
    {
      sm.vars.output_event_press = false;
      Serial.print("PRESS EVENT ");
      Serial.println(button_label);
    }

    if (sm.vars.output_event_tap)
    {
      sm.vars.output_event_tap = false;
      Serial.print("TAP EVENT ");
      Serial.println(button_label);
    }

    if (sm.vars.output_event_held)
    {
      sm.vars.output_event_held = false;
      Serial.print("HELD EVENT ");
      Serial.println(button_label);
    }

    if (sm.vars.output_event_release)
    {
      sm.vars.output_event_release = false;
      Serial.print("RELEASE EVENT ");
      Serial.println(button_label);
      Serial.println();
    }
  }

  delay(10);
}
