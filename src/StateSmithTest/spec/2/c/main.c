#include "Spec2Sm.h"

#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <assert.h>
#include <string.h>

#include "../../lang-helpers/c/helper.h"

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x) / sizeof(0 [x])) / ((size_t)(!(sizeof(x) % sizeof(0 [x])))))

const char * const event_names[] = {
    [Spec2Sm_EventId_DO] = "DO",
    [Spec2Sm_EventId_EV1] = "EV1",
    [Spec2Sm_EventId_EV2] = "EV2",
    [Spec2Sm_EventId_EV3] = "EV3",
    [Spec2Sm_EventId_EV4] = "EV4",
    [Spec2Sm_EventId_EV5] = "EV5",
    [Spec2Sm_EventId_EV6] = "EV6",
    [Spec2Sm_EventId_EV7] = "EV7",
    [Spec2Sm_EventId_EV8] = "EV8",
    [Spec2Sm_EventId_EV9] = "EV9",
    [Spec2Sm_EventId_EV10] = "EV10",
    [Spec2Sm_EventId_EVBACK] = "evBack",
    [Spec2Sm_EventId_EVCLOSE] = "evClose",
    [Spec2Sm_EventId_EVOPEN] = "evOpen",
    [Spec2Sm_EventId_EVSTEP] = "evStep",
};
static_assert(COUNT_OF(event_names) == Spec2Sm_EventIdCount, "above mapping outdated");
static_assert(Spec2Sm_EventIdCount == 15, "above mapping outdated");


int main(int arg_count, char** args)
{
    Spec2Sm sm;
    Spec2Sm_ctor(&sm);

    print_start();
    Spec2Sm_start(&sm);
    printf("\n");

    for (int i = 1; i < arg_count; i++) // start at 1 to skip program name
    {
        char* line = args[i];
        if (strnlen(line, 1000) == 0)
        {
            continue;
        }

        enum Spec2Sm_EventId event_id = (enum Spec2Sm_EventId)find_event_id_from_name_or_exit(event_names, COUNT_OF(event_names), line, Spec2Sm_EventIdCount);
        print_dispatch_event_name(line);

        Spec2Sm_dispatch_event(&sm, event_id);
        printf("\n");
    }

    return 0;
}
