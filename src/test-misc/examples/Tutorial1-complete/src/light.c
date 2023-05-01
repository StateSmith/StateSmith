#include "light.h"
#include <stdio.h>
#include <stdlib.h>


static const char* off_string = 
"    _------_   \n" \
"  -~        ~-   \n" \
" -     _      -   \n" \
"-      ||      -   \n" \
"-      ||      -   \n" \
" -     ||     -   \n" \
"  -    ||    -   \n" \
"   -   ||   -   \n" \
"    -__||__-   \n" \
"    |______|   \n" \
"    <______>   \n" \
"    <______>   \n" \
"       \\/   \n";

static const char* on1_string = 
"    _------_   \n" \
"  -~  .   . ~-   \n" \
" - .   _    . -   \n" \
"-      ||      -   \n" \
"-  .   ||   .  -   \n" \
" -    .||.    -   \n" \
"  -  .<||>.  -   \n" \
"   - .<||>. -   \n" \
"    -__||__-   \n" \
"    |______|   \n" \
"    <______>   \n" \
"    <______>   \n" \
"       \\/   \n";

static const char* on2_string = 
"    _------_   \n" \
"  -~.  .  . ~-   \n" \
" -.  . _ .  . -   \n" \
"- .  .^||^.  . -   \n" \
"-...<<<||>>>.. -   \n" \
" -..<<<||>>>.-   \n" \
"  -..<<||>>..-   \n" \
"   -.<<||>>.-   \n" \
"    -__||__-   \n" \
"    |______|   \n" \
"    <______>   \n" \
"    <______>   \n" \
"       \\/   \n";

static const char* on3_string = 
"    _------_   \n" \
"  -~^^^^^^^^~-   \n" \
" -<<<<<^^>>>>>-   \n" \
"-<<<<<<||>>>>>>-   \n" \
"-<<<<<<||>>>>>>-   \n" \
" -<<<<<|:)>>>-   \n" \
"  -<<<<||>>>>-   \n" \
"   -<<<||>>>-   \n" \
"    -__||__-   \n" \
"    |______|   \n" \
"    <______>   \n" \
"    <______>   \n" \
"       \\/   \n";

static const char* boom_string = 
" `     ^     '   \n" \
"                 \n" \
"`  ! ! ! ! !   '   \n" \
"  ! KA-BOOM! !      \n" \
"   ! ! ! ! !  .    \n" \
"                  \n" \
"  `   `   '  '    \n" \
"                 \n" \
"    -__||__-   \n" \
"    |______|   \n" \
"    <______>   \n" \
"    <______>   \n" \
"       \\/   \n";


static void clear_screen(void)
{
    system("clear");
}

void light_off(void)
{
    clear_screen();
    printf("%s", off_string);
}

void light_on1(void)
{
    clear_screen();
    printf("%s", on1_string);
}

void light_on2(void)
{
    clear_screen();
    printf("%s", on2_string);
}

void light_on3(void)
{
    clear_screen();
    printf("%s", on3_string);
}

void light_boom(void)
{
    clear_screen();
    printf("%s", boom_string);
}
