StateSmith has zero dependencies and can run anywhere. This is great, but also means that you need to consider how to handle concurrency.

```c
// Dispatches an event to the state machine. Not thread safe.
void MySm_dispatch_event(MySm* sm, MySm_EventId event_id)
{
    //...
}
```

If your state machine will only be accessed by a single thread/task, no worries! Party on!

# Concurrency Solutions
There are a lot of ways to handle concurrency depending on your programming language and framework/platform.

One of my personal favorites is to use a thread safe queue. In the below image, Thread 1 and 3 put events into the queue. The state machine (Thread 2) reads the events from the queue and processes them. The state machine can also push events into the queue for itself to process later as well.

![image](https://user-images.githubusercontent.com/274012/233886021-1d396612-16d0-4a3d-a50b-6f6917f4d7fb.png)

## Embedded systems variations
If you are on an embedded system and can't use a queue, you may be able to use variables specific to the event type. This is like a queue of size one for each event type: `bool reset_requested`, `bool communication_lost`.

You can then read and clear those variables in a critical section (disable interrupts or mutex)...

# Examples
Care to share a simple repo with an example of safe concurrency for a specific language/framework? Would be helpful as there are too many combinations I can't cover alone.

I currently only have a [few examples](https://github.com/StateSmith/StateSmith-examples/blob/main/README.md#-concurrency).