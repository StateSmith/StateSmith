grammar PlantumlGrammar;

/*
@startuml

[*] --> State1
State1 --> [*]
State1 : this is a string
State1 : this is another string

State1 -> State2
State2 --> [*]

@enduml
 */

optional_any_space: (HWS | line_end_with_hs)*;
ohs: HWS? ;
some_ws: (HWS | LINE_ENDER)+ ;
line_end_with_hs: LINE_ENDER ohs;

start_end_state: '[*]';
state_id: IDENTIFIER;

vertex:
    start_end_state | state_id
    ;

edge:
    '->' | '-->'
    ;

transition_event_guard_code:
    rest_of_line
    ;

// State1 -> State2
// State2 --> [*]
transition:
    vertex ohs
    edge ohs
    vertex
    (
        ohs
        ':'
        ohs
        transition_event_guard_code
    )?
    ;


// state NotShooting {
//   [*] --> Idle
//   Idle --> Configuring : EvConfig
//   Configuring --> Idle : EvConfig
// }
state_composite:
    'state'
    HWS+
    IDENTIFIER ohs '{' ohs LINE_ENDER
    diagram_element*
    '}'
;

rest_of_line:
    (
        IDENTIFIER | HWS | DIGIT // fixme add more stuff that's allowed
    )*
    ;

// State1 : this is a string
// State1 : this is another string
state_contents:
    IDENTIFIER
    ohs
    ':'
    ohs
    rest_of_line
    ;

ignore:
    'hide empty description'
    |
    'scale ' rest_of_line
    ;

diagram_element:
    ohs
    (
        ignore
        |
        state_contents
        |
        transition
        |
        state_composite
    )
    ohs
    LINE_ENDER
    ohs
    ;

// @startuml some_name_here
startuml:
    START_UML
    (
        HWS+
        IDENTIFIER
    )?
    ;

diagram:
    optional_any_space
    startuml ohs LINE_ENDER
    
    diagram_element*

    END_UML optional_any_space
    EOF
    ;

fragment IDENTIFIER_NON_DIGIT :  [a-zA-Z_] ;

START_UML: '@startuml';
END_UML: '@enduml';

HWS : [ \t]+ ;
LINE_ENDER: [\r\n]+;
IDENTIFIER  :   IDENTIFIER_NON_DIGIT   (   IDENTIFIER_NON_DIGIT | DIGIT  )*  ;
DIGIT :   [0-9]  ;
