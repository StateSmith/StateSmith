#include <stdio.h>
#include <stdbool.h>

static bool print(const char * const str)
{
    printf("%s", str);
    return true;    // useful for printing behavior's guard
}
