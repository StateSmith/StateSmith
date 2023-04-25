#include "app_timer.h"
#include <threads.h>
#include <time.h>

#define NS_IN_MS (1000 * 1000)
#define NS_IN_SECOND (1000 * NS_IN_MS)

static uint32_t approx_ms_counts;

void app_timer_sleep_ms(uint16_t ms)
{
    uint64_t ns_total = ((uint64_t)ms) * NS_IN_MS;
    long seconds_part = ns_total / NS_IN_SECOND;
    long nanoseconds_part = ns_total % NS_IN_SECOND;

    // for simple example, ignore return code and time remaining tracking
    thrd_sleep(&(struct timespec){ .tv_sec = seconds_part, .tv_nsec = nanoseconds_part }, NULL);

    approx_ms_counts += ms;
}


uint32_t app_timer_get_ms(void)
{
    return approx_ms_counts;
}

