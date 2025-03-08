#include <stdio.h>

void powerOff() {
    printf("powerOff\n");
}

void powerOn() {
    printf("powerOn\n");
}


#include "lightbulb.hpp"

// same as main.cpp but no base class
// You can still use functions and globals that are defined before the include,
// but it's not as clean as customizing the base class.

int main() {
    lightbulb<> bulb;
    bulb.start();
    bulb.dispatchEvent(lightbulb<>::EventId::SWITCH);
    return 0;
}
