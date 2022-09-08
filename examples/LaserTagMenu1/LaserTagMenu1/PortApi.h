#pragma once
#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

void PortApi_debug_printf(const char * format, ...);

void PortApi_debug_msg(const char* const message);

bool PortApi_digital_read(uint8_t input_pin);

void PortApi_enable_pullup(uint8_t input_pin);

uint32_t PortApi_get_time_ms(void);

#ifdef __cplusplus
}
#endif
