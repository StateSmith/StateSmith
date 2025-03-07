/* Include the generated state machine */
#include "lightbulb.hpp"

using namespace std;

/* Implement a base class for your statemachine that contains your callbacks */
class MyBase {
protected:
    void enter_on() { cout << "Lightbulb is on" << endl; }
    void enter_off() { cout << "Lightbulb is off" << endl; }
}


/* 
 * Instantiate the state machine using your base.
 * This will make lightbulb inherit from MyBase.
 */
lightbulb<MyBase> bulb;

/* 
 * The event loop that will be started by main. 
 * Respond to events and update the state machine. 
 */
void loop() {
  int c = getchar(); // we don't care what the character is, we just need to consume it
  bulb.dispatchEvent(lightbulb::EventId::SWITCH);
}

int main() {
  printf("Press <enter> to toggle the light switch.\n");
  printf("Press ^C to quit.\n");

  /* Start the state machine */
  bulb.start();

  /* Start an event loop to respond to events and update the state machine. */
  while(1) {
      loop();
  }

  return 0;
}
