#include "App.h"

/*
APP
├── HOME
│   ├── HOME1
│   ├── HOME2
│   └── HOME3
└── MAIN MENU
        ├── SELECT CLASS
        │   ├── ENGINEER
        │   ├── HEAVY
        │   ├── ARCHER
        │   ├── WIZARD
        │   └── SPY
        │── SHOW INFO
        │   ├── INFO 1
        │   ├── INFO 2
        │   └── INFO 3
        └── EAT BACK PRESSES
            └── <stuff to show event handling>
*/

void setup() {
  Serial.begin(115200);
  Serial.println("Laser time!");
  Serial.println("See .ino file for menu layout. Explore!");
  Serial.println("Keyboard keys left, up, down, right have a binding to switches above when simulation pane has focus.");
  Serial.println("");

  App_setup();
}

void loop() {
  App_step();

  delay(10);
}
