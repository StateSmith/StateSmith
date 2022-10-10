#pragma once
#include <stdbool.h>
#include <stddef.h>
#include <inttypes.h>

void trace(char const*const str);
bool trace_guard(char const*const str, bool guard);
void print_divider(void);
void print_start(void);
void print_dispatch_event_name(const char* const event_name);
size_t find_event_id_from_name(const char * const event_mapping[], const size_t event_mapping_count, const char * const event_name);
bool parse_umax(const char* const line, uintmax_t * const result);
size_t find_event_id_from_name_or_exit(const char * const event_mapping[], const size_t event_mapping_count, const char * const event_name, const uintmax_t max_value);
