// NOTE! This code is just for a quick proof of concept! Not high quality code.

#include "Display.h"
#include "App.h"
#include <stddef.h>

#include <LiquidCrystal.h>
#include "Arduino.h"


////////////////////////////////////////////////////////////////////////////////
// defines
////////////////////////////////////////////////////////////////////////////////

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x)/sizeof(0[x])) / ((size_t)(!(sizeof(x) % sizeof(0[x])))))

#define BLANK_LINE "                " // long enough to blank LCD display

#define CHAR_CODE_UP_ARROW    ((char)1)
#define CHAR_CODE_DOWN_ARROW  ((char)2)
#define CHAR_CODE_DOTS        ((char)3)
#define CHAR_CODE_SMILEY      ((char)4)


////////////////////////////////////////////////////////////////////////////////
// global vars
////////////////////////////////////////////////////////////////////////////////

static LiquidCrystal g_lcd(12, 11, 10, 9, 8, 7);


////////////////////////////////////////////////////////////////////////////////
// c++ prototypes
////////////////////////////////////////////////////////////////////////////////

static const char * player_class_to_string(enum PlayerClass player_class);


////////////////////////////////////////////////////////////////////////////////
// c public functions
////////////////////////////////////////////////////////////////////////////////

extern "C" {

void Display_setup(void)
{
    // required to be non-const
    uint8_t smiley[8] = {
        B00000,
        B10001,
        B00000,
        B00000,
        B10001,
        B01110,
        B00000,
    };
    g_lcd.createChar(CHAR_CODE_SMILEY, smiley);

    uint8_t up[8] = {
        B00100,
        B01110,
        B11111,
        B00100,
        B00100,
        B00100,
        B00100,
    };
    g_lcd.createChar(CHAR_CODE_UP_ARROW, up);

    uint8_t down[8] = {
        B00100,
        B00100,
        B00100,
        B00100,
        B11111,
        B01110,
        B00100,
    };
    g_lcd.createChar(CHAR_CODE_DOWN_ARROW, down);

    uint8_t dots[8] = {
        B10101,
        B01010,
        B10101,
        B01010,
        B10101,
        B01010,
        B10101,
    };
    g_lcd.createChar(CHAR_CODE_DOTS, dots);


    g_lcd.begin(16, 2);
}

void Display_step(void)
{

}

void Display_top_line(const char *const str)
{
    g_lcd.setCursor(0, 0);
    g_lcd.print(str);
    g_lcd.print(BLANK_LINE);
}

void Display_bot_line(const char *const str)
{
    g_lcd.setCursor(0, 1); // column, row
    g_lcd.print(str);
    g_lcd.print(BLANK_LINE);
}

void Display_menu_header(const char *const str)
{
    g_lcd.setCursor(1, 0);
    g_lcd.print(str);
    g_lcd.print(BLANK_LINE);
}

void Display_menu_option(const char *const str)
{
    g_lcd.setCursor(1, 1);
    g_lcd.print(" "); // slight indent
    g_lcd.print(str);
    g_lcd.print(BLANK_LINE);
}

void Display_set_arrows(bool up, bool down)
{
    g_lcd.setCursor(0, 0);
    g_lcd.print(up ? CHAR_CODE_UP_ARROW : CHAR_CODE_DOTS);

    g_lcd.setCursor(0, 1);
    g_lcd.print(down ? CHAR_CODE_DOWN_ARROW : CHAR_CODE_DOTS);
}

void Display_menu_at_top(void)
{
    Display_set_arrows(false, true);
}

void Display_menu_at_mid(void)
{
    Display_set_arrows(true, true);
}

void Display_menu_at_bottom(void)
{
    Display_set_arrows(true, false);
}

void Display_show_home_screen_1(void)
{
    g_lcd.clear();
    g_lcd.print("GAME: LAZER CATZ");
    g_lcd.setCursor(0, 1);
    g_lcd.print("CLASS: ");
    g_lcd.print(player_class_to_string(App_get_player_class()));
}

void Display_show_home_screen_2(void)
{
    g_lcd.clear();
    g_lcd.print("WEAPON 1:");
    g_lcd.setCursor(0, 1);
    g_lcd.print("LITTLE PEW-PEW");
}

void Display_show_home_screen_3(void)
{
    g_lcd.clear();
    g_lcd.print("WEAPON 2:");
    g_lcd.setCursor(0, 1);
    g_lcd.print("FIXIN WRENCH");
}

void Display_class_saved(void)
{
    g_lcd.clear();
    g_lcd.print("YOUR CLASS SAVED");
    g_lcd.setCursor(0, 1);
    g_lcd.print("YOU BE ");
    g_lcd.print(player_class_to_string(App_get_player_class()));
    g_lcd.print(CHAR_CODE_SMILEY);
}

void Display_show_back_press_count(uint8_t count)
{
    g_lcd.setCursor(0, 0);
    g_lcd.print("BACK COUNT:");
    g_lcd.print(count);
    g_lcd.print(BLANK_LINE);
}

void Display_show_back_press_taunt(const char *taunt)
{
    g_lcd.setCursor(0, 1);
    g_lcd.print(taunt);
    g_lcd.print(BLANK_LINE);
}

void Display_show_random_back_press_taunt(void)
{
    const char* const taunts[] = {
        "YOU STILL HERE!?",
        "FOR THE LOVE OF",
        "DO NO UI EVIL?",
        "SEGFAULT DREAMS",
        "TRASH PANDA!",
    };

    uint8_t index = random();
    index %= COUNT_OF(taunts);
    Display_show_back_press_taunt(taunts[index]);
}

} // extern c


////////////////////////////////////////////////////////////////////////////////
// c++ private functions
////////////////////////////////////////////////////////////////////////////////

static const char * player_class_to_string(enum PlayerClass player_class)
{
    switch (player_class)
    {
    case PlayerClass_ENGINEER:
        return "ENGINEER";
    case PlayerClass_HEAVY:
        return "HEAVY";
    case PlayerClass_ARCHER:
        return "ARCHER";
    case PlayerClass_WIZARD:
        return "WIZARD";
    case PlayerClass_SPY:
        return "SPY";
    default:
        return "???";
    }
}

