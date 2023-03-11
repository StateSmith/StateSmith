#include "Spec1Sm.h"

#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <assert.h>
#include <string.h>

#include "../../lang-helpers/c/helper.h"

// gcc -g -Wall ../../lang-helpers/c/helper.c main.c Spec1Sm.c

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x) / sizeof(0 [x])) / ((size_t)(!(sizeof(x) % sizeof(0 [x])))))

const char * const event_names[] = {
    [Spec1Sm_EventId_EV1] = "EV1",
    [Spec1Sm_EventId_EV2] = "EV2",
};
static_assert(COUNT_OF(event_names) == Spec1Sm_EventIdCount, "above mapping outdated");


int main(int arg_count, char** args)
{
    Spec1Sm sm;
    Spec1Sm_ctor(&sm);

    print_start();
    Spec1Sm_start(&sm);
    printf("\n");

    for (int i = 1; i < arg_count; i++) // start at 1 to skip 
    {
        char* line = args[i];

        if (strnlen(line, 1000) == 0)
        {
            continue;
        }

        enum Spec1Sm_EventId event_id = (enum Spec1Sm_EventId)find_event_id_from_name_or_exit(event_names, COUNT_OF(event_names), line, Spec1Sm_EventIdCount);
        print_dispatch_event_name(line);

        static_assert(COUNT_OF(event_names) == Spec1Sm_EventIdCount, "required for safe array access above");

        Spec1Sm_dispatch_event(&sm, event_id);
        printf("\n");
    }

    return 0;
}
