#include <iostream>

using namespace std;

class MyBaseClass {
public:
    void powerOn() {
        std::cout << "Powering on" << std::endl;
    }
    void powerOff() {
        std::cout << "Powering off" << std::endl;
    }
};

#include "lightbulb.hpp"

int main() {
    lightbulb<MyBaseClass> bulb;
    bulb.start();
    bulb.dispatchEvent(lightbulb<MyBaseClass>::EventId::SWITCH);
    return 0;
}
