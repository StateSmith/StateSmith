State machines are incredibly helpful for certain applications:
* 📱 User Interfaces
* 🦾 Control Systems 🚀
* 👾 Video Games 🕹️
* 🤖 Robotics
* 🌐 Protocols
* 🆎 Parsers/Decoders

<br>

# 🤔 Advice: One State Machine or Two?
Some quick advice: don't try to pack too much functionality into a single state machine if it doesn't fit.

If the design starts to feel "wrong", stop and take a look. Would you be better served by two independent state machines that cooperate? Or maybe one state machine runs the other? [Video games have been doing this forever](https://www.reddit.com/r/gamedev/comments/45nn5i/comment/czzdwp8/?utm_source=share&utm_medium=web2x&context=3).

# 📱 User Interfaces
Really helpful for complex UIs/menus or when a basic menu just won't work. This is how I got really interested in state machines. I had to implement a very large deeply nested LCD user interface (300+ states) that had lots of custom behaviors and frequent client changes. We started with a regular menu implemented in data arrays, but the data approach couldn't keep up with client requirements.

[Examples](https://github.com/StateSmith/StateSmith-examples#-menu--user-interface).

# 🦾 Control Systems 🚀
The company I work for uses StateSmith for many control systems projects.

TODO: example project.

# 👾 Video Games 🕹️
Useful right now for user input, User Interfaces and simple "AI".

More complex "AI" will be easier to implement when we support [sub-statemachines](https://github.com/StateSmith/StateSmith/issues/179), and [orthogonal/concurrent states](https://github.com/StateSmith/StateSmith/issues/75)

[Examples](https://github.com/StateSmith/StateSmith-examples#-video-game).

# 🤖 Robotics
Good for the basics right now.

Will be even more useful when we support [sub-statemachines](https://github.com/StateSmith/StateSmith/issues/179), and [orthogonal/concurrent states](https://github.com/StateSmith/StateSmith/issues/75).

TODO: example webots project.

# 🌐 Protocols
Very helpful for protocols like TCP, DHCP.

[![image](https://user-images.githubusercontent.com/274012/234150679-77d509c7-fe0b-475d-b9d7-081ee06b7eea.png)](http://tcpipguide.com/free/t_TCPOperationalOverviewandtheTCPFiniteStateMachineF-2.htm)
[![image](https://user-images.githubusercontent.com/274012/234150781-b2fa295e-973a-4f7b-92c6-aeb39217ed05.png)](http://www.tcpipguide.com/free/t_DHCPGeneralOperationandClientFiniteStateMachine.htm)

TODO: example project.

# 🆎 Parsers/Decoders
Parsers are essentially state machines.

While StateSmith can help you with this, I would check for a purpose built tool that meets your needs first.

## Reasons you might use StateSmith for a parser
* Embedded developers: many existing tools are too resource heavy, use dynamic memory, or may have unbound recursion.
* Can operate on partial input. You can parse "on the fly" while input is typed or received. Most existing parser tools expect the full input to be available in RAM.
* Useful for learning and experimentation.

You can implement large/more sophisticated parsers in StateSmith, but it can be rather labor intensive without some tricks. In the future, I'll make an example project.

There's a lot of fun possibilities and ideas here. Many parser grammar tools generate a state machine for a particular programming language (maybe a few). It would be awesome to have a parsing tool output PlantUML or something StateSmith could understand. Then the parser tool could use StateSmith to generate code for any language that StateSmith supports (which will hopefully be 10+ eventually).

TODO: example project.