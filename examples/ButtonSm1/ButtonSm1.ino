#include "ButtonSm1.h"

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x)/sizeof(0[x])) / ((size_t)(!(sizeof(x) % sizeof(0[x])))))

const uint8_t buttonPins[] = { 7, 6, 5, 4 };
#define BUTTON_COUNT COUNT_OF(buttonPins)

struct ButtonSm1 buttonSms[BUTTON_COUNT];


void setup() {
  Serial.begin(115200);
  Serial.println("We have booted!");

  for (uint8_t i = 0; i < BUTTON_COUNT; i++) {
    pinMode(buttonPins[i], INPUT_PULLUP);
    ButtonSm1_ctor(&buttonSms[i]);
    ButtonSm1_start(&buttonSms[i]);
  }


}

void loop() {
  for (uint8_t i = 0; i < BUTTON_COUNT; i++) {
    ButtonSm1 & sm = buttonSms[i];
    sm.vars.input_is_pressed = (digitalRead(buttonPins[i]) == LOW);
    ButtonSm1_dispatch_event(&sm, ButtonSm1_EventId_DO);

    if (sm.vars.output_event_press) {
      sm.vars.output_event_press = false;
      Serial.print("PRESS EVENT ");
      Serial.println(buttonPins[i]);
    }

    if (sm.vars.output_event_tap) {
      sm.vars.output_event_tap = false;
      Serial.print("TAP EVENT ");
      Serial.println(buttonPins[i]);
    }

    if (sm.vars.output_event_held) {
      sm.vars.output_event_held = false;
      Serial.print("HELD EVENT ");
      Serial.println(buttonPins[i]);
    }

    if (sm.vars.output_event_release) {
      sm.vars.output_event_release = false;
      Serial.print("RELEASE EVENT ");
      Serial.println(buttonPins[i]);
    }
  }

  delay(10);
}
