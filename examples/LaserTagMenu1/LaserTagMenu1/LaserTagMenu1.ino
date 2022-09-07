#include "App.h"

void setup() {
  Serial.begin(115200);
  Serial.println("Laser time!");
  Serial.println("Keyboard keys left, up, down, right have a binding to switches above when simulation pane has focus.");
  Serial.println("");

  App_setup();
}

void loop() {
  App_step();

  delay(10);
}
