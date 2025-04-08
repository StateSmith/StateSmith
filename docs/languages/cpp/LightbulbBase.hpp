#pragma once

#include <stdio.h>

/* Implement a class for your statemachine that contains your callbacks */
class LightbulbBase {
public:
    void enterOn() { printf("Lightbulb is on.\n"); }
    void enterOff() { printf("Lightbulb is off.\n"); }
};
