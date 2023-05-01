#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include "light.h"
#include "Tutorial1Sm.h"

////////////////////////////////////////////////////////////////////////////////
// global vars
////////////////////////////////////////////////////////////////////////////////

static Tutorial1Sm g_state_machine;

////////////////////////////////////////////////////////////////////////////////
// prototypes
////////////////////////////////////////////////////////////////////////////////

static void read_input_run_state_machine(void);
static char read_char_from_line_and_clear_user_input(void);


////////////////////////////////////////////////////////////////////////////////
// functions
////////////////////////////////////////////////////////////////////////////////


int main(void)
{
    printf("---------------------------------------\n\n");
    printf("Tutorial1 C Code Running!\n\n");
    printf("USAGE:\n");
    printf("  type i <enter> to send INCREASE event to state machine\n");
    printf("  type d <enter> to send DIM event to state machine\n");
    printf("  type o <enter> to send OFF event to state machine\n");
    printf("\n");
    printf("hit <enter> to start\n");
    read_char_from_line_and_clear_user_input();

    // setup and start state machine
    Tutorial1Sm_ctor(&g_state_machine);
    Tutorial1Sm_start(&g_state_machine);

    while (true)
    {
        read_input_run_state_machine();
    }
}


static void read_input_run_state_machine(void)
{
    bool valid_input = true;
    enum Tutorial1Sm_EventId event_id = Tutorial1Sm_EventId_OFF;

    char c = read_char_from_line_and_clear_user_input();
    switch (c)
    {
        case 'i': event_id = Tutorial1Sm_EventId_INCREASE; break;
        case 'd': event_id = Tutorial1Sm_EventId_DIM; break;
        case 'o': event_id = Tutorial1Sm_EventId_OFF; break;
        default: valid_input = false; break;
    }

    if (valid_input)
    {
        Tutorial1Sm_dispatch_event(&g_state_machine, event_id);
    }
}


// blocks while waiting for input
static char read_char_from_line_and_clear_user_input(void)
{
    char buf[100];
    char* c_ptr = fgets(buf, sizeof(buf), stdin);

    // erase user input from command line so that lightbulb animation stays at top
    // https://stackoverflow.com/a/35190285/7331858
    printf("\033[A");   // VT100 - move cursor up one line
    printf("\33[2K\r"); // VT100 - erase line cursor is on
    fflush(stdout);

    if (c_ptr == NULL)
    {
        return '\0';
    }

    return *c_ptr;
}
