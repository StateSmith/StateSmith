transformation phases:

1. yEd XML to diagram nodes and edges
2. node and edge interpretation into state machines, states, transitions, ...



node label grammar:

`example: 1. (EVENT1, EVENT2) [guard_expression] / { action_expression }`
`via_exit(1) via_entry(my_entry) / action();`


grammar StateStuff;
state_name: \w+  line_ender

trigger_simple: \w+
trigger_list: '('
                (
                    trigger_simple | ( , trigger_simple )*
                )
              ')' ;
triggers: trigger_simple | trigger_list ;
order: \d+ '.' ;

guard: '[' code_expression ']' ;

action: braced_action | naked_action ;
braced_action: '/' '{' code_expression '}'
naked_action:  '/' code_expression

behavior: order? triggers? guard? action?

