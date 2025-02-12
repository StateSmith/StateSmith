You can override the StateSmith file writing functionality so that you can easily post process the generated code before it is written to file.

Why would you want to do this?
* can be used to remove state_id_to_string() function if not wanted
* can be used to add custom functions
* can be used to add code coverage annotations
* or anything really

Examples: https://github.com/StateSmith/StateSmith-examples/tree/main/user-post-process
Video: https://www.youtube.com/watch?v=UYL6Qzoyxwo&t=50s&ab_channel=StateSmith

https://github.com/StateSmith/StateSmith/issues/114