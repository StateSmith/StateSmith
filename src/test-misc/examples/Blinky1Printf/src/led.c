#include "led.h"
#include <stdio.h>
#include <stdlib.h>

// https://ascii.co.uk/art/led
static const char* off_string = 
"             \n" \
"       .-.   \n" \
"    .'     '.\n" \
"   : '-._.-' :\n" \
"   :   ____  : \n" \
"   : _/_  |_ :\n" \
"   : | /  || :\n" \
"   : ||   || :\n" \
"   :_||___||_:\n" \
"     ||   || \n" \
"     ||   || \n" \
"     ||   || \n" \
"     ||   || \n" \
"          ||\n";

// https://ascii.co.uk/art/led
static const char* on_string = 
"    |||||||||\n" \
"   ||||.-.||||\n" \
"  ||.'|||||'.||\n" \
"   : '-._.-' :\n" \
"   :   ____  : \n" \
"   : _/_  |_ :\n" \
"   : | /  || :\n" \
"   : ||   || :\n" \
"   :_||___||_:\n" \
"     ||   || \n" \
"     ||   || \n" \
"     ||   || \n" \
"     ||   || \n" \
"          ||\n";

static void clear_screen(void)
{
    system("clear");
}

void led_turn_on(void)
{
    clear_screen();
    printf("%s", on_string);
}

void led_turn_off(void)
{
    clear_screen();
    printf("%s", off_string);
}
