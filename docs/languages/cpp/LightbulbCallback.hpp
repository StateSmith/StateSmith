#include <iostream>
#pragma once

using namespace std;

/* Implement a class for your statemachine that contains your callbacks */
class LightbulbCallback {
public:
    void enter_on() { cout << "Lightbulb is on." << endl; }
    void enter_off() { cout << "Lightbulb is off." << endl; }
};
