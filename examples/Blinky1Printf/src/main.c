#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>

#include "led.h"
#include "Blinky1PrintfSm.h"
#include "app_timer.h"

// gcc -Wall -std=c11 main.c led.c app_timer.c Blinky1PrintfSm.c  && ./a.out

int main(void)
{
    Blinky1PrintfSm sm;
    Blinky1PrintfSm_ctor(&sm);

    if (false)
    {
        led_turn_off();
        app_timer_sleep_ms(1000);
        led_turn_on();
        app_timer_sleep_ms(1000);
    }

    Blinky1PrintfSm_start(&sm);

    while (true)
    {
        Blinky1PrintfSm_dispatch_event(&sm, Blinky1PrintfSm_EventId_DO);
        app_timer_sleep_ms(100);
    }
}