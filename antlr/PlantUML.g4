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
We don't want that. We want to detect invalid diagram elements.

Investigate semantic predicates: https://github.com/antlr/antlr4/blob/master/doc/predicates.md
*/

// line ending with optional white space before and after
line_ending_ows:
    optional_any_space
    LINE_ENDER
    optional_any_space
    ;

optional_any_space: (HWS | LINE_ENDER)*;

// optional horizontal white space
ohs: HWS* ;

// symbol for an initial/start or terminate/end state. termination states are not supported
start_end_state: '[*]';
history_state: '[' ('h'|'H') ']';
state_id: identifier history_state?; // like `State3` or `State3[H]`

vertex:
    start_end_state | history_state | state_id
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
                ~('[' | ']' | LINE_ENDER)
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
    'skin' HWS rest_of_line
    |
    'mainframe' HWS rest_of_line
    |
    'title' HWS rest_of_line
    |
    'skinparam' HWS identifier optional_any_space
    '{' ohs LINE_ENDER
        (
            ohs 
            ( 
                identifier
                stereotype?
                HWS+
                (
                    identifier | DIGIT | '#'
                )+
            )?
            LINE_ENDER
        )*
    ohs
    '}'
    |
    'skinparam' HWS rest_of_line
    ;

diagram_element:
    line_ending_ows+
    (
        state_contents
        |
        transition
        |
        state_explicit
        |
        note
        |
        ignore
        |
        kept_block_comment
    )
    ;

// note left of Active : this is a short\nnote
note_short:
    'note'
    (
        HWS+
        identifier
    )+
    ohs
    ':'
    ohs
    rest_of_line
    ;

// Kept comment blocks are used for comments that need to be kept for further processing for things like
// https://github.com/StateSmith/StateSmith/issues/335
kept_block_comment : KEPT_BLOCK_COMMENT;

// starts with line ender
note_multiline_contents_line:
    LINE_ENDER ~(ENDNOTE | LINE_ENDER)*
    ;

// note right of Inactive
//   A note can also
//   be defined on
//   several lines
// end note
note_multiline:
    'note'
    HWS+
    identifier // ex: right
    HWS+
    identifier // ex: of
    HWS+
    identifier // ex: Inactive
    ohs
    (note_multiline_contents_line)*
    ENDNOTE
    ;

// See https://github.com/StateSmith/StateSmith/issues/343
//    @startuml
//    [*] -> State1
//    State1 --> State2
//    note on link 
//      this is a state-transition note 
//    end note
//    @enduml
note_on_link:
    'note'
    HWS+
    'on'
    HWS+
    'link'
    ohs
    (note_multiline_contents_line)*
    ENDNOTE
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
    note_on_link // must come before note_multiline
    |
    note_multiline
    |
    note_floating
    ;

statemachine_name:
    identifier
    |
    // {fileName}_1
    // https://github.com/StateSmith/StateSmith/issues/330
    '{' identifier '}' identifier?
    ;


// @startuml some_name_here
startuml:
    START_UML
    (
        HWS+
        statemachine_name
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

// Horizontal white space
HWS : [ \t]+ ;
LINE_ENDER: '\r\n' | '\r' | '\n';
identifier
    : IDENTIFIER
    | 'state'
    | 'State'
    | 'note'
    | 'end'
    | 'as'
    | 'scale'
    | 'skin'
    | 'mainframe'
    | 'title'
    | 'skinparam'
    | 'on'
    | 'link'
    ;
IDENTIFIER  :   IDENTIFIER_NON_DIGIT   ( IDENTIFIER_NON_DIGIT | DIGIT )*  ;
DIGIT :   [0-9]  ;

// Kept comment blocks are used for comments that need to be kept for further processing for things like
// https://github.com/StateSmith/StateSmith/issues/335
// We use a separate lexter rule that comes BEFORE the BLOCK_COMMENT rule so that it is matched first and
// isn't skipped. This is much easier than not skipping the BLOCK_COMMENT rule as comments can appear
// almost anywhere in plantuml syntax and our parsing rules would become a real mess.
KEPT_BLOCK_COMMENT :
    BLOCK_COMMENT_START
    '!'
    .*?
    BLOCK_COMMENT_END
    ;

fragment BLOCK_COMMENT_START : '/' SINGLE_QUOTE;
fragment BLOCK_COMMENT_END : SINGLE_QUOTE '/';
BLOCK_COMMENT :
    BLOCK_COMMENT_START
    .*?
    BLOCK_COMMENT_END
    -> skip
    ;

// 'Line comments use a single apostrophe.
// They can't be used anywhere. They must be at the start of a line.
LINE_COMMENT:
    (START_OF_INPUT | LINE_ENDER)
    HWS*
    SINGLE_QUOTE ~[\r\n]*
    -> skip
    ;

// This token MUST occur AFTER the LINE_COMMENT token as it is a subset of the LINE_COMMENT token.
// See https://github.com/StateSmith/StateSmith/issues/352
START_OF_INPUT : '\u0001' -> skip;

SYMBOLS: 
    [-~`!@#$%^&*()_+=\\|{};:",<.>/?] | '[' | ']';
SINGLE_QUOTE: ['];

fragment ESCAPED_CHAR: '\\' . ;
fragment NON_QUOTE_CHAR: ~["] ;
fragment STRING_CHAR: ESCAPED_CHAR | NON_QUOTE_CHAR ;
STRING: '"' STRING_CHAR* '"' ;

// starts with line ender
ENDNOTE: LINE_ENDER HWS* ('end' HWS* 'note');
