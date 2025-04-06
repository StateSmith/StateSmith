# AlgoBalanced1 Potential Alternative - Const Event Handler Table
Instead of dynamically updating the event handler array upon entry/exit, we could have a const event handler table.

I initially had used const state handlers for each state so that they didn't have to be in RAM, but realized that this wouldn't work as desired for AVR/Arduino. It works great for my STM32 work though. The AVR can't put data in flash with just a regular const. It needs special access and use of [PROGMEM](https://www.nongnu.org/avr-libc/user-manual/pgmspace.html). This is a pain, but can be implemented in the future. That said, I'm not sure what the speed cost would be for accessing flash as data.

Given the AVR/Arduino limitation, using regular const arrays would eat way more memory. Multiply the event count by the state count by pointer size. The laser tag example has `29 states` and `8 events`. `29 * 8 * sizeof(int*) == 464`. That's way too much. The current Balanced1 code gen uses only `8 * sizeof(int*) == 16` which isn't bad.

Pros:
* faster (no updating in entry/exit), just simple assignment.
* less RAM on some platforms

Cons:
* WAY more RAM on some platforms.

We may make a user setting to enable this.

