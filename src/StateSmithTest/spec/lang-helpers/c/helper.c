#include "helper.h"

#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <assert.h>
#include <string.h>

void trace(char const*const str)
{
    printf("%s\n", str);
    fflush(stdout); // useful in case c code fails in someway
}

bool trace_guard(char const*const str, bool guard)
{
    printf("%s ", str);

    if (guard)
    {
        printf("Behavior running.");
    }
    else
    {
        printf("Behavior skipped.");
    }

    printf("\n");

    return guard;
}

void print_divider(void)
{
    printf("===================================================\n");
}

void print_start(void)
{
    printf("Start Statemachine\n");
    print_divider();
}

void print_dispatch_event_name(const char* const event_name)
{
    printf("Dispatch event %s\n", event_name);
    print_divider();
}

size_t find_event_id_from_name(const char * const event_mapping[], const size_t event_mapping_count, const char * const event_name)
{
    for (size_t i = 0; i < event_mapping_count; i++)
    {
        if (event_mapping[i] == NULL)
        {
            printf("`event_mapping[%lu]` is null! Update mapping.\n", i);
            return event_mapping_count;
        }

        if (strcmp(event_mapping[i], event_name) == 0)
        {
            return i;
        }
    }
    
    return event_mapping_count;
}

size_t find_event_id_from_name_or_exit(const char * const event_mapping[], const size_t event_mapping_count, const char * const event_name, const uintmax_t max_value)
{
    size_t event_id = find_event_id_from_name(event_mapping, event_mapping_count, event_name);
    if (event_id >= max_value)
    {
        printf("!!! Invalid input `%s` \n", event_name);
        exit(-1);
    }

    return event_id;
}

bool parse_umax(const char* const line, uintmax_t * const result)
{
    errno = 0;
    char* endptr;
    uintmax_t value = strtoumax(line, &endptr, 10);
    if (endptr == NULL || *endptr != '\0' || errno != 0)
    {
        printf("!!! Invalid input `%s` \n", line);
        return false;
    }

    *result = value;
    return true;
}
