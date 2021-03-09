grammar Simple;
@header{
package prz.lang;
}
atom : NUMBER | ID ;                // match identifier or number
expression  :     // match atom and operator and expression
    atom
    | L_BRACKET expression R_BRACKET
    | expression OP_1 expression
    | expression OP_2 expression ;

NUMBER : [1-9][0-9]* ;              // match numbers
ID : [a-z]+ ;                       // match lower-case identifiers
WS : [ \t\r\n]+ -> skip ;           // skip spaces, tabs, newlines
OP_1 : ('*'|'/') ;                  // match /*
OP_2 : ('+'|'-') ;                  // match +-
OP   : ('+'|'-'|'*'|'/') ;          // match +-/*
L_BRACKET : '(' ;                   // match bracket
R_BRACKET : ')' ;                   // match bracket