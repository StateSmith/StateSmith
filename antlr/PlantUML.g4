grammar PlantUML;

/*
# IMPORTANT notes
Wild card and negations are context sensitive.
If you write `(.*?)` in a LEXER rule, it will match any characters lazily.
If you write `(.*?)` in a parser rule, it will match any lexed tokens lazily.

Don't use (.*?) parse rules unless absolutely necessary. They can hide parse errors.
Saw this with multiline comment (when it was a parse rule instead of a lexer rule).
    /' blah1 '/
    invalid parse line
    /' blah2 '/
blah1 stretched over the invalid parse line to join with blah2.
We don't want want. We want to detect invalid diagram elements.

Investigate semantic predicates: https://github.com/antlr/antlr4/blob/master/doc/predicates.md
*/

// line ending with optional white space before and after
line_ending_ows:
    optional_any_space
    LINE_ENDER
    optional_any_space
    ;

optional_any_space: (HWS | LINE_ENDER)*;

// optional  horizontal white space
ohs: HWS* ;

start_end_state: '[*]';
state_id: identifier;

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
    //ex: `-->`
    //ex: `Third -left-> Last`
    //ex: `S1 -right[dotted,#blue]-> S5`
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
    '{'
        diagram_element*
        optional_any_space
    '}'
    ;

stereotype:
    '<<'
    identifier
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
    identifier // ex: long1
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
    identifier
    ohs
    ':'
    ohs
    rest_of_line
    ;

ignore:
    'hide empty description'
    |
    'scale' HWS rest_of_line
    |
    'skinparam' HWS identifier ohs '{' (~'}')* '}'
    |
    'skinparam' HWS rest_of_line
    |
    // 'Line comments use a single apostrophe
    SINGLE_QUOTE rest_of_line
    ;

diagram_element:
    line_ending_ows+
    (
        ignore
        |
        state_contents
        |
        transition
        |
        state_explicit
        |
        note
    )
    ;

// note left of Active : this is a short\nnote
note_short:
    'note'
    HWS+
    (~':')+
    ':'
    ohs
    rest_of_line
    ;


note_multiline_contents:
    .+?
    ;

note_multiline:
    'note'
    HWS+
    (~(':' | LINE_ENDER))+ LINE_ENDER
    (
        note_multiline_contents
        LINE_ENDER
    )?
    ohs
    'endnote'
    ;

// note left of Active : this is a short\nnote
note_floating:
    'note'
    HWS+
    STRING
    rest_of_line
    ;

note:
    note_short
    |
    note_multiline
    |
    note_floating
    ;


// @startuml some_name_here
startuml:
    START_UML
    (
        HWS+
        identifier
    )?
    ;

diagram:
    optional_any_space
    startuml ohs
    
    diagram_element*

    line_ending_ows+
    END_UML optional_any_space
    EOF
    ;

fragment IDENTIFIER_NON_DIGIT :  [a-zA-Z_] ;

START_UML: '@startuml';
END_UML: '@enduml';

HWS : [ \t]+ ;
LINE_ENDER: '\r\n' | '\r' | '\n';
identifier
    : IDENTIFIER
    | 'state'
    | 'State'
    | 'note'
    | 'as'
    | 'scale'
    | 'skinparam'
    | 'note'
    ;
IDENTIFIER  :   IDENTIFIER_NON_DIGIT   (   IDENTIFIER_NON_DIGIT | DIGIT  )*  ;
DIGIT :   [0-9]  ;

fragment BLOCK_COMMENT_START : '/' SINGLE_QUOTE;
fragment BLOCK_COMMENT_END : SINGLE_QUOTE '/';
BLOCK_COMMENT :
    BLOCK_COMMENT_START
    .*?
    BLOCK_COMMENT_END
    -> skip
    ;

SYMBOLS: 
    [-~`!@#$%^&*()_+=\\|{};:",<.>/?] | '[' | ']';
SINGLE_QUOTE: ['];

fragment ESCAPED_CHAR: '\\' . ;
fragment NON_QUOTE_CHAR: ~["] ;
fragment STRING_CHAR: ESCAPED_CHAR | NON_QUOTE_CHAR ;
STRING: '"' STRING_CHAR* '"' ;
