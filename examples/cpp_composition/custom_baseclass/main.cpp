#include <iostream>

using namespace std;

class LightbulbCallback {
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
    LightbulbCallback callback;
    lightbulb<LightbulbCallback> bulb(callback);
    bulb.start();
    bulb.dispatchEvent(lightbulb<LightbulbCallback>::EventId::SWITCH);
    return 0;
}
