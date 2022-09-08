#pragma once
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// must be called before any other Display functions
void Display_setup(void);

// call periodically
void Display_step(void);

void Display_top_line(const char* const str);
void Display_bot_line(const char* const str);

void Display_menu_header(const char* const str);
void Display_menu_option(const char* const str);

void Display_menu_at_top(void);
void Display_menu_at_mid(void);
void Display_menu_at_bottom(void);

void Display_show_home_screen_1(void);
void Display_show_home_screen_2(void);
void Display_show_home_screen_3(void);

void Display_class_saved(void);

void Display_show_back_press_taunt(const char *taunt);
void Display_show_random_back_press_taunt(void);
void Display_show_back_press_count(uint8_t count);

#ifdef __cplusplus
}
#endif
