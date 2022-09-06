#include "App.h"

void setup() {
  Serial.begin(115200);
  Serial.println("We have booted!");

  App_setup();
}

void loop() {
  App_step();

  delay(10);
}
