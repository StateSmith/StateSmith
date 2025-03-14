/* Include the generated state machine */
#include "Lightbulb.hpp"
#include "LightbulbCallback.hpp"

using namespace std;

/* 
 * Instantiate the state machine with an instance of your callback.
 */
LightbulbCallback callback;
Lightbulb bulb(callback);

/* 
 * The event loop that will be started by main. 
 * Respond to events and update the state machine. 
 */
void loop() {
  int c = getchar(); // we don't care what the character is, we just need to consume it
  bulb.dispatchEvent(Lightbulb::EventId::SWITCH);
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
