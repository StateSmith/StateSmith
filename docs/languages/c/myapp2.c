#include <stdio.h>
#include <stdlib.h>

/* Implement the callbacks that are referenced in lightbulb.puml */
void enter_on() {
  printf("Lightbulb is on\n");
}

void enter_off() {
  printf("Lightbulb is off\n");
}

/* 
 * Include the generated state machine.
 * 
 * NEW NEW NEW
 * 
 * We manually renamed lightbulb.c to lightbulb.inc.h to emphasize
 * that it is an included file and not a standalone source file.
 */
#include "lightbulb.inc.h"

/* Instantiate the state machine struct */
lightbulb sm;

/* 
 * The event loop that will be started by main. 
 * Respond to events and update the state machine. 
 */
void loop() {
  int c = getchar(); // we don't care what the character is, we just need to consume it
  lightbulb_dispatch_event(&sm, lightbulb_EventId_SWITCH);
}

int main() {
  printf("Press <enter> to toggle the light switch.\n");
  printf("Press ^C to quit.\n");

  /* Initialize and start the state machine */
  lightbulb_ctor(&sm);
  lightbulb_start(&sm);

  /* Start an event loop to respond to events and update the state machine. */
  while(1) {
      loop();
  }

  return 0;
}
