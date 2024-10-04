#include <stdio.h>
#include "helper.h"

class Spec2SmBase {
protected:

    void trace(const char* msg) {
        ::trace(msg);
    }

    bool trace_guard(const char* msg, bool guard) {
        return ::trace_guard(msg, guard);
    }
};
