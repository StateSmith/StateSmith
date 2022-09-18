#include "App.h"
#include <string.h> // for memset
#include "Buttons.h"
#include "LaserTagMenu1Sm.h"
#include <stdbool.h>
#include "PortApi.h"
#include <assert.h>
#include "Display.h"

////////////////////////////////////////////////////////////////////////////////
// defines
////////////////////////////////////////////////////////////////////////////////

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x)/sizeof(0[x])) / ((size_t)(!(sizeof(x) % sizeof(0[x])))))


////////////////////////////////////////////////////////////////////////////////
// global vars
////////////////////////////////////////////////////////////////////////////////

static struct LaserTagMenu1Sm g_menu_sm;
static enum PlayerClass g_player_class;
static enum LaserTagMenu1Sm_StateId g_sm_last_state_id;


////////////////////////////////////////////////////////////////////////////////
// prototypes
////////////////////////////////////////////////////////////////////////////////

static void dispatch_button_events(void);
static void detect_sm_state_change(void);


////////////////////////////////////////////////////////////////////////////////
// public functions
////////////////////////////////////////////////////////////////////////////////

void App_setup()
{
    g_player_class = PlayerClass_ENGINEER;
    Display_setup();

    Buttons_setup();

    LaserTagMenu1Sm_ctor(&g_menu_sm);
    g_sm_last_state_id = g_menu_sm.state_id;

    LaserTagMenu1Sm_start(&g_menu_sm);
    detect_sm_state_change();
}

void App_step()
{
    Buttons_step();

    dispatch_button_events();
    LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_DO);
    detect_sm_state_change();

    Display_step();
}

void App_save_player_class(uint8_t class_id)
{
    g_player_class = class_id; // assume valid for simple example application
}

enum PlayerClass App_get_player_class(void)
{
    return g_player_class;
}


////////////////////////////////////////////////////////////////////////////////
// private functions
////////////////////////////////////////////////////////////////////////////////

static void dispatch_button_events(void)
{
    // This could be coded more elegantly using a mapping, but I figured
    // it was best to keep it simple for example purposes.

    // down events
    {
        ButtonSm1 * const down_sm = &Buttons_sms[ButtonId_DOWN];
        static_assert(COUNT_OF(Buttons_sms) > ButtonId_DOWN, "required for array access");

        if (down_sm->vars.output_event_press)
        {
            down_sm->vars.output_event_press = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_DOWN_PRESS);
        }

        if (down_sm->vars.output_event_held)
        {
            down_sm->vars.output_event_held = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_DOWN_HELD);
        }
    }

    // up events
    {
        ButtonSm1 * const up_sm = &Buttons_sms[ButtonId_UP];
        static_assert(COUNT_OF(Buttons_sms) > ButtonId_UP, "required for array access");

        if (up_sm->vars.output_event_press)
        {
            up_sm->vars.output_event_press = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_UP_PRESS);
        }

        if (up_sm->vars.output_event_held)
        {
            up_sm->vars.output_event_held = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_UP_HELD);
        }
    }

    // ok events
    {
        ButtonSm1 * const ok_sm = &Buttons_sms[ButtonId_OK];
        static_assert(COUNT_OF(Buttons_sms) > ButtonId_OK, "required for array access");

        if (ok_sm->vars.output_event_press)
        {
            ok_sm->vars.output_event_press = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_OK_PRESS);
        }
    }

    // back events
    {
        ButtonSm1 * const back_sm = &Buttons_sms[ButtonId_BACK];
        static_assert(COUNT_OF(Buttons_sms) > ButtonId_BACK, "required for array access");

        if (back_sm->vars.output_event_press)
        {
            back_sm->vars.output_event_press = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_BACK_PRESS);
        }

        if (back_sm->vars.output_event_held)
        {
            back_sm->vars.output_event_held = false;
            LaserTagMenu1Sm_dispatch_event(&g_menu_sm, LaserTagMenu1Sm_EventId_BACK_HELD);
        }
    }
}

static void detect_sm_state_change(void)
{
    const enum LaserTagMenu1Sm_StateId current_state = g_menu_sm.state_id;
    if (current_state != g_sm_last_state_id)
    {
        PortApi_debug_printf("State changed from %s to %s\n", 
            LaserTagMenu1Sm_state_id_to_string(g_sm_last_state_id),
            LaserTagMenu1Sm_state_id_to_string(current_state));
        g_sm_last_state_id = current_state;
    }
}
