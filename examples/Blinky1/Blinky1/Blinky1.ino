#include "Blinky1Sm.h"

static Blinky1Sm state_machine;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  Blinky1Sm_ctor(&state_machine);
  Blinky1Sm_start(&state_machine);
}

void loop() {
  Blinky1Sm_dispatch_event(&state_machine, Blinky1Sm_EventId_DO);

  delay(10);
}
