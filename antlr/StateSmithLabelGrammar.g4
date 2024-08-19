grammar StateSmithLabelGrammar;

/*
    todolow allow tags

    todolow - look into using LEXER rules _like_ `COLON` instead of chars like ':' in parsing rules.

    NOTES!
    Lexing rules are very important and should be used with care!

    https://riptutorial.com/antlr/example/11235/priority-rules
    Several lexer rules can match the same input text. In that case, the token type will be chosen as follows:

    - First, select the lexer rule which matches the longest input
    - If the text matches an implicitly defined token (like '{'), use the implicit rule
    - If several lexer rules match the same input length, choose the first one, based on definition order

 */
optional_any_space: (HWS | line_end_with_hs)*;
ohs: HWS? ;

some_ws: (HWS | LINE_ENDER)+ ;

node:
    notes_node
    |
    config_node
    |
    exit_point
    |
    entry_point
    |
    choice
    |
    state_defn
    |
    ortho_defn
    |
    statemachine_defn
    ;

statemachine_name:
    SS_IDENTIFIER
    |
    // {fileName}_1
    // https://github.com/StateSmith/StateSmith/issues/330
    '{' SS_IDENTIFIER '}' SS_IDENTIFIER?
    ;

statemachine_defn:
    optional_any_space
    '$STATEMACHINE'
    ohs
    COLON
    ohs
    statemachine_name
    optional_any_space
    state_behaviors
    EOF
    ;

any_text:
    .*?
    ;

notes_node:
    optional_any_space
    '$NOTES'
    (
        some_ws
        any_text
    )?
    EOF
    ;

// https://github.com/StateSmith/StateSmith/issues/23
config_node:
    optional_any_space
    '$CONFIG'
    ohs
    COLON
    ohs
    SS_IDENTIFIER
    ohs
    (
        LINE_ENDER
        any_text
    )?
    EOF
    ;

state_behaviors:
    ohs
    (
        nl_behaviors
        |
        optional_any_space
    )
    optional_any_space
    ;

//$ORTHO 1 : BASIC
ortho_defn:
    optional_any_space
    '$ORTHO'
    ohs
    ortho_order?
    ohs
    COLON
    ohs
    state_id
    state_behaviors
    EOF
    ;

//examples:
//PRESSED
state_defn:
    optional_any_space
    state_id
    state_behaviors
    EOF
    ;

// todo_low - remove.
global_id:
    '#'
    SS_IDENTIFIER
    ;

state_id:
    global_id
    |
    SS_IDENTIFIER;

ortho_order:
    number
    ;

edge:
    optional_any_space
    edge_behaviors?
    optional_any_space
    EOF
    ;

edge_behaviors:
    behavior
    ohs
    nl_behaviors?
    ;

nl_behaviors:
    nl_behavior+ ;

nl_behavior:
    line_end_with_hs
    optional_any_space
    behavior 
    ;

// https://github.com/StateSmith/StateSmith/issues/3
point_label:
    DIGIT+ SS_IDENTIFIER?
    |
    SS_IDENTIFIER
    ;

// https://github.com/StateSmith/StateSmith/issues/3
entry_point:
    optional_any_space
    ENTRY
    optional_any_space
    COLON
    optional_any_space
    point_label
    optional_any_space
    EOF
    ;


// https://github.com/StateSmith/StateSmith/issues/40
choice:
    optional_any_space
    '$choice'
    (
        optional_any_space
        COLON
        optional_any_space
        point_label
    )?
    optional_any_space
    EOF
    ;

// NOTE! We can't a rule like below because the SS_IDENTIFIER lexer token will match all of example
// input like `$entry_first` and because it matches more than `$entry_` it wins causing this to fail.
// There might be a way to do it, but it is much simpler instead to require `entry` to be
// split from the identifier.
    // entry_point:
    //     optional_any_space
    //     '$entry_'
    //     point_label
    //     optional_any_space
    //     ;

// https://github.com/StateSmith/StateSmith/issues/3
exit_point:
    optional_any_space
    EXIT
    optional_any_space
    COLON
    optional_any_space
    point_label
    optional_any_space
    EOF
    ;

via_entry_type: ENTRY;
via_exit_type: EXIT;

// supports entry and exit points
// https://github.com/StateSmith/StateSmith/issues/3
transition_via:
    optional_any_space 
    VIA
    some_ws
    ( via_entry_type | via_exit_type )
    some_ws
    point_label
    ;

// plural because it might have an entry and exit via in some rare cases
transition_vias:
    transition_via
    ( some_ws transition_via)*
    ;

behavior:
    order?
    ( 
        triggers guard action? transition_vias?
        |
        triggers action? transition_vias?
        |
        guard action? transition_vias?
        |
        action transition_vias?
        |
        transition_vias
    )
    ;


order: 
    ohs
    number  //should only be DIGIT here? Right now allows `1.2`
    ohs
    '.';


triggers: 
    trigger_id 
    |
    trigger_list
    ;

trigger_id:
    ohs
    expandable_identifier
    ;

/*
samples:
    (MY_TRIGGER)
    ( MY_TRIGGER )
    (TRIG1,TRIG2)
    ( TRIG1 , TRIG2 )
 */
trigger_list:
    ohs
    '('
        optional_any_space
        trigger_id
        (
            optional_any_space
            ','
            optional_any_space
            trigger_id
        )*
        ohs
    ')' ;


guard:
    ohs
    '['
        guard_code
    ']'
    ;


guard_code:
    ohs
    any_code  //allow to be empty in the parsing phase. Maybe warn later.
    ;


action:
    ohs
    FORWARD_SLASH
    ohs
    action_code? // optional to support consuming events with `SOME_EVENT /` https://github.com/StateSmith/StateSmith/issues/43
    ;

/*
 Note: we cannot just use `any_code` because then it would be legal to do things like:
    MY_STATE
    [true] / func(); { code...} { code... }

That just looks too weird.
 */
action_code:
    naked_action_code // event / do_something();
    |
    braced_expression // event / { do_something(); }
    ;

naked_action_code:
    naked_action_code_elements+? // we want it to be non-greedy here so that it doesn't consume via statements like `EVENT [guard] / action_code(); via entry MY_ENTRY_POINT`
    ;

/*
For expansions, we need to ensure not part of a member access.
Assume `time` is an expansion to `vars->time_ms`

Should match here:
    `if (time>10)`, `if (time > 10)`
    `time++`

Should NOT match here:
    `if (Class::time>10)`, `if (Class::time > 10)`
    `obj.time++`
    `obj->time++`
 */


member_access_operator:
    (
        PERIOD // foo.bar
        |
        COLON COLON //foo::bar
        |
        (DASH GT) //foo->bar. todolow this can conflict with Java lambda.
    )
    ;

member_access:
    optional_any_space
    member_access_operator

    optional_any_space
    (
        //no expansion checking here because this belongs to something else. `foo->bar`. `foo` should be checked for expansion though.
        permissive_identifier
        |
        member_function_call
    )
    ;

//checked for expansions
expandable_identifier:
    ohs 
    permissive_identifier
    ;

// allows state machine grammar keywords to be used as identifiers
permissive_identifier:
    (SS_IDENTIFIER | EXIT | ENTRY | VIA) // exit/entry lexer tokens required because of exit/entry point rules
    ;

group_expression: 
    ohs '(' any_code ')' 
    ;

square_brace_expression: '[' any_code ']' ;
braced_expression: '{' ohs any_code '}' ;   //ohs here to help with indent formatting

line_comment: 
    LINE_COMMENT 
    ( line_end_with_hs | EOF )
    ;
star_comment: STAR_COMMENT;

function_args:
    code_element+
    ;

leading_optional_any_space:
    optional_any_space
    ;

trailing_optional_any_space:
    optional_any_space
    ;


braced_function_args:
    '('
    leading_optional_any_space
    function_args?
    trailing_optional_any_space
    ')'
    ;

//be a bit strict on whitespace as there is a lot of state machine specific syntax here already
// `foo (123)` will not be allowed, but `foo(123)` will be.
expandable_function_call:
    ohs 
    permissive_identifier
    braced_function_args
    ;

//NON expandable
member_function_call:
    ohs 
    permissive_identifier
    braced_function_args
    ;

any_code:
    ohs
    code_element* ;

code_element: 
    code_line_element
    |
    line_end_with_hs
    ;


naked_action_code_elements:
    star_comment |
    string_literal |
    expandable_function_call |
    member_access | //must come before identifier to prevent bad expansions: `obj.no_expand_here`
    expandable_identifier |
    number |
    code_symbol |
    group_expression |
    HWS
    ;

//does not include newlines except when inside other expressions
code_line_element:
    line_comment |
    star_comment |
    string_literal |
    expandable_function_call |
    member_access | //must come before identifier to prevent bad expansions: `obj.no_expand_here`
    expandable_identifier |
    number |
    code_symbol |
    group_expression |
    square_brace_expression |
    braced_expression |
    HWS
    ;

code_line: 
    ohs;


line_end_with_hs: LINE_ENDER ohs;

number :
    (DASH | PLUS)?
    DIGIT+
    (
        PERIOD
        DIGIT+
    )?
    (
       LITTLE_E DIGIT+
    )?
    ;

string_literal: STRING | TICK_STRING | BACKTICK_STRING; // don't name as `string` because then antlr generated code has warnings in it

code_symbol:
    PERIOD |
    COMMA |
    PLUS |
    DASH |
    COLON |
    GT |
    LT |
    OTHER_SYMBOLS |
    FORWARD_SLASH
    ;

/////////////////////////////////// LEXER RULES - ORDER MATTERS! ///////////////////////////////////

LINE_ENDER: [\r\n]+;

// StateSmith keywords
// These should be added to permissive_identifier
// must come before SS_IDENTIFIER
EXIT: 'exit';
ENTRY: 'entry';
VIA: 'via';


fragment IDENTIFIER_NON_DIGIT :  [$a-zA-Z_] ;

// StateSmith identifier. Can't be other keywords like `exit`, `entry`, `via`, etc.
SS_IDENTIFIER  :  (LITTLE_E | IDENTIFIER_NON_DIGIT)   (   IDENTIFIER_NON_DIGIT | DIGIT  )*  ;
LITTLE_E: 'e';

fragment NOT_NL_CR: ~[\n\r];
LINE_COMMENT: '//' NOT_NL_CR*;
STAR_COMMENT: '/*' .*? '*/' ;

// CHAR_LITERAL: [']
//       ( ESCAPED_CHAR | ~['] )*
//       ['] ;


fragment ESCAPED_CHAR: '\\' . ;
fragment NON_QUOTE_CHAR: ~["] ;
fragment STRING_CHAR: ESCAPED_CHAR | NON_QUOTE_CHAR ;
STRING: DOUBLE_QUOTE STRING_CHAR* DOUBLE_QUOTE ;
TICK_STRING: SINGLE_QUOTE STRING_CHAR* SINGLE_QUOTE ;
BACKTICK_STRING: BACKTICK STRING_CHAR* BACKTICK ;

DIGIT :   [0-9];

DOUBLE_QUOTE: '"';
SINGLE_QUOTE: '\'';
BACKTICK: '`';
PERIOD: '.' ;
COMMA: ',' ;
PLUS : '+' ;
DASH : '-' ;
COLON : ':';
GT : '>' ;
LT : '<' ;
FORWARD_SLASH: '/' ; // fix for https://github.com/StateSmith/StateSmith/issues/230
OTHER_SYMBOLS: 
    [~!%^&*=:;?|\\@#$];  //don't include braces/parenthesis as those need to be grouped

HWS : [ \t]+ ;
