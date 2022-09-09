#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>

#include "led.h"
#include "blinky1_printf_sm.h"
#include "app_timer.h"

// gcc -Wall -std=c11 main.c led.c app_timer.c blinky1_printf_sm.c  && ./a.out

int main(void)
{
    blinky1_printf_sm sm;
    blinky1_printf_sm_ctor(&sm);

    if (false)
    {
        led_turn_off();
        app_timer_sleep_ms(1000);
        led_turn_on();
        app_timer_sleep_ms(1000);
    }

    blinky1_printf_sm_start(&sm);

    while (true)
    {
        blinky1_printf_sm_dispatch_event(&sm, BLINKY1_PRINTF_SM_EVENT_ID_DO);
        app_timer_sleep_ms(100);
    }
}