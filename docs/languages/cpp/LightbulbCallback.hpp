#include <iostream>
using namespace std;

/* Implement a class for your statemachine that contains your callbacks */
class LightbulbCallback {
protected:
    void enter_on() { cout << "Lightbulb is on." << endl; }
    void enter_off() { cout << "Lightbulb is off." << endl; }
};
