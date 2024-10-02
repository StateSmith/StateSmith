#include <stdio.h>

class Spec2SmBase {
protected:

    void trace(const char* msg) {
        printf("%s\n", msg);
    }

    bool trace_guard(const char* msg, bool guard) {
        trace(msg);
        return guard;
    }
};
