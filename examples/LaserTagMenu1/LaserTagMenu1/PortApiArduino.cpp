#include "PortApi.h"
#include "Arduino.h"
#include <stdarg.h>

void PortApi_debug_msg(const char* const message)
{
    Serial.println(message);
}

void PortApi_debug_printf(const char * format, ...)
{
  char buffer[128];
  va_list args;
  va_start(args, format);
  vsnprintf(buffer, sizeof(buffer), format, args); // ignore return code for example application
  va_end (args);

  Serial.print(buffer);
}

bool PortApi_digital_read(uint8_t input_pin)
{
    return digitalRead(input_pin);
}

void PortApi_enable_pullup(uint8_t input_pin)
{
    pinMode(input_pin, INPUT_PULLUP);
}

uint32_t PortApi_get_time_ms(void)
{
    return millis();
}
