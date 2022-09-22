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

// State1 -> State2
// State2 --> [*]
transition:
    vertex ohs
    edge ohs
    vertex
    ;

state_contents_string:
    .*?
    ;

// State1 : this is a string
// State1 : this is another string
state_contents:
    IDENTIFIER
    ohs
    ':'
    ohs
    state_contents_string
    ;

ignore:
    'hide empty description'
    |
    'scale ' .*?
    ;

diagram_element:
    (
        ignore
        |
        state_contents
        |
        transition
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
        .*?
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
