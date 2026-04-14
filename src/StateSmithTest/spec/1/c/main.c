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

// tests for https://github.com/StateSmith/StateSmith/issues/535
static void test_issue_535(void)
{
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_ROOT) == Spec1Sm_StateId_ROOT);
    // While the above is not actually true (ROOT's parent is null and not ROOT), the get parent id function can only return an enum value
    // so it uses ROOT as the default value. If you were looping over enum ID's and getting their parent ID, just skip checking ROOT.

    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_S)    == Spec1Sm_StateId_ROOT);
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_S1)   == Spec1Sm_StateId_S);
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_S11)  == Spec1Sm_StateId_S1);
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_T1)   == Spec1Sm_StateId_S);
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_T11)  == Spec1Sm_StateId_T1);
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateId_T111) == Spec1Sm_StateId_T11);

    // invalid inputs should also give ROOT
    assert(Spec1Sm_get_parent_id(Spec1Sm_StateIdCount) == Spec1Sm_StateId_ROOT);
    assert(Spec1Sm_get_parent_id(-1) == Spec1Sm_StateId_ROOT);
}

// tests for https://github.com/StateSmith/StateSmith/issues/538
static void test_issue_538(void)
{
    assert(Spec1Sm_ROOT_SubtreeEndId == 6);
    assert(Spec1Sm_ROOT_SubtreeEndId == Spec1Sm_StateId_T111);

    assert(Spec1Sm_S_SubtreeEndId == 6);
    assert(Spec1Sm_S_SubtreeEndId == Spec1Sm_StateId_T111);

    assert(Spec1Sm_S1_SubtreeEndId == 3);
    assert(Spec1Sm_S1_SubtreeEndId == Spec1Sm_StateId_S11);
    
    assert(Spec1Sm_S11_SubtreeEndId == 3);
    assert(Spec1Sm_S11_SubtreeEndId == Spec1Sm_StateId_S11);

    assert(Spec1Sm_T1_SubtreeEndId == 6);
    assert(Spec1Sm_T11_SubtreeEndId == 6);
    assert(Spec1Sm_T111_SubtreeEndId == 6);
}

int main(int arg_count, char** args)
{
    test_issue_535();
    test_issue_538();

    Spec1Sm sm;
    Spec1Sm_ctor(&sm);

    print_start();
    Spec1Sm_start(&sm);
    printf("\n");

    for (int i = 1; i < arg_count; i++) // start at 1 to skip 
    {
        char* line = args[i];

        if (strlen(line) == 0)
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
