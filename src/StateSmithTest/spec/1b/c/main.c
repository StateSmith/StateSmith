#include "Spec1bSm.h"

#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <assert.h>
#include <string.h>


int main(int arg_count, char** args)
{
    Spec1bSm sm;
    Spec1bSm_ctor(&sm);

    Spec1bSm_start(&sm);
    Spec1bSm_dispatch_event(&sm, Spec1bSm_EventId_T1);
    return 0;
}
