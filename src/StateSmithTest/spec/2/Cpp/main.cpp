#include "Spec2Sm.hpp"
#include "helper.h"

#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <assert.h>
#include <string>
#include <string.h>
#include <unordered_map>

using namespace Spec2;

// https://stackoverflow.com/a/4415646/7331858
#define COUNT_OF(x) ((sizeof(x) / sizeof(0 [x])) / ((size_t)(!(sizeof(x) % sizeof(0 [x])))))

std::unordered_map<std::string, Spec2Sm::EventId> event_map = {
    {"DO", Spec2Sm::EventId::DO},
    {"EV1", Spec2Sm::EventId::EV1},
    {"EV2", Spec2Sm::EventId::EV2},
    {"EV3", Spec2Sm::EventId::EV3},
    {"EV4", Spec2Sm::EventId::EV4},
    {"EV5", Spec2Sm::EventId::EV5},
    {"EV6", Spec2Sm::EventId::EV6},
    {"EV7", Spec2Sm::EventId::EV7},
    {"EV8", Spec2Sm::EventId::EV8},
    {"EV9", Spec2Sm::EventId::EV9},
    {"EV10", Spec2Sm::EventId::EV10},
    {"evBack", Spec2Sm::EventId::EVBACK},
    {"evClose", Spec2Sm::EventId::EVCLOSE},
    {"evOpen", Spec2Sm::EventId::EVOPEN},
    {"evStep", Spec2Sm::EventId::EVSTEP},
};
static_assert(Spec2Sm::EventIdCount == 15, "above mapping outdated");

int main(int arg_count, char** args)
{
    assert(event_map.size() == Spec2Sm::EventIdCount);

    Spec2Sm sm;
    ::print_start();
    sm.start();
    printf("\n");

    for (int i = 1; i < arg_count; i++) // start at 1 to skip program name
    {
        char* line = args[i];
        if (strlen(line) == 0)
        {
            continue;
        }
        std::string str_line(line);

        if (event_map.find(str_line) == event_map.end())
        {
            printf("Event not found: %s\n", line);
            return 1;
        }

        enum Spec2Sm::EventId event_id = event_map.at(str_line);
        print_dispatch_event_name(line);

        sm.dispatchEvent(event_id);
        printf("\n");
    }

    return 0;
}
