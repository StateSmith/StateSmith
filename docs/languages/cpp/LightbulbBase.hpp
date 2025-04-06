#include <iostream>
#pragma once

using namespace std;

/* Implement a class for your statemachine that contains your callbacks */
class LightbulbBase {
public:
    void enterOn() { cout << "Lightbulb is on." << endl; }
    void enterOff() { cout << "Lightbulb is off." << endl; }
};
