#include "ButtonSm1Cpp.h"

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x) / sizeof(0 [x])) / ((size_t)(!(sizeof(x) % sizeof(0 [x])))))

const uint8_t buttonPins[] = {7, 6, 5, 4};
#define BUTTON_COUNT COUNT_OF(buttonPins)

struct ButtonSm1Cpp buttonSms[BUTTON_COUNT];

void setup()
{
  Serial.begin(115200);
  Serial.println("We have booted!");

  for (uint8_t i = 0; i < BUTTON_COUNT; i++)
  {
    pinMode(buttonPins[i], INPUT_PULLUP);
    ButtonSm1Cpp_ctor(&buttonSms[i]);
    ButtonSm1Cpp_start(&buttonSms[i]);
  }
}

void loop()
{
  for (uint8_t i = 0; i < BUTTON_COUNT; i++)
  {
    ButtonSm1Cpp &sm = buttonSms[i];
    sm.vars.input_is_pressed = (digitalRead(buttonPins[i]) == LOW);
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
