grammar PlantumlGrammar;

/*
# IMPORTANT notes
Wild card and negations are context sensitive.
If you write `(.*?)` in a LEXER rule, it will match any characters lazily.
If you write `(.*?)` in a parser rule, it will match any lexed tokens lazily.


TODO
    - skinparam
    - notes

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
    (
        ohs
        stereotype
    )?
    ;

edge:
    '->'
    |
    //ex: `-->`, `Third -left-> Last`, `S1 -right[dotted,#blue]-> S5`
    '-' IDENTIFIER? 
    (
        '['
            (
                ~(']' | LINE_ENDER)
            )*
        ']'
    )?
    '->' 
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



state_child_states:
    '{' ohs LINE_ENDER   
    diagram_element*
    '}'
    ;

stereotype:
    '<<'
    (IDENTIFIER | 'state' | 'State' | 'as')
    '>>'
    ;

// ex: state "Accumulate Enough Data\nLong State Name" as long1
// ex:
//      state NotShooting {
//        [*] --> Idle
//        Idle --> Configuring : EvConfig
//        Configuring --> Idle : EvConfig
//      }
state_explicit:
    ('state' | 'State')
    HWS+
    (
        STRING HWS+ 'as' HWS+   // ex: "Accumulate Enough Data\nLong State Name" as
    )?
    IDENTIFIER // ex: long1
    (
        ohs
        stereotype
    )?
    ohs
    state_child_states?
    ;

rest_of_line:
    (
        ~LINE_ENDER
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
    optional_any_space
    (
        ignore
        |
        state_contents
        |
        transition
        |
        state_explicit
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

fragment ESCAPED_CHAR: '\\' . ;
fragment NON_QUOTE_CHAR: ~["] ;
fragment STRING_CHAR: ESCAPED_CHAR | NON_QUOTE_CHAR ;
STRING: '"' STRING_CHAR* '"' ;
