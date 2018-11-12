### Expressions
Expressions in templates drive the entire data binding system. They can support literal values as well as much more complicated
structures like property access, method calls, comparison operators, math operators, boolean logic and pretty much
any other operator you might use in C#.


#### Literal values
````C#
    <SomeElement literalIntValue="{1}"/>
    <SomeElement literalFloatValue="{1f}"/>
    <SomeElement literalDoubleValue="{1.0}"/>
    <SomeElement literalBooleanValue="{true}"/>
    <SomeElement literalStringValue="{'string value'}"/>
    
````

#### Math operators
````C#
     <SomeElement addition="{10f + 4}"/>
     <SomeElement subtraction="{10f - 4}"/>
     <SomeElement multiplication="{10f * 4}"/>
     <SomeElement division="{10f / 4}"/>
     <SomeElement modulus="{10f % 4}"/>
     <SomeElement complexOperators="{50f * 3 - 4f / 25}"/>
     <SomeElement parenOperator="{50f * (3 - 4f) / 25}"/>
````

#### String concat

````C#
     Any expression used with a + and a string operand will be cast to a string and concatenated
     {'some string' + (50 * 2) }
     {'more strings' + 'some other string'}
````
    
#### Comparisons
````C#
    {50 > 14} 
    {255 < 25}
    {67 + 25 <= 1}
    {52 * 5 >= 67 * 3}
    {67 != 41}
    {88 == 88}
````

#### Ternary
````C#
    <SomeElement someValue="{76 > 18 ? 'string one' : 'string two'}"/>
````

#### Unaries
````C#
    <SomeElement someBoolValue="{!(1 > 2)}"/>
    <SomeElement someIntValue="{-(1 + 2)}"/>
````

#### Property Access
Any property on an element can be accessed. 
````C#
    <SomeElement someValue="{someFieldOnTheType}"/>
    <SomeElement someValue="{someArrayField[6]}"/>
    <SomeElement someValue="{some.nested.array[6].some.other.property}"/>
    <SomeElement someValue="{some.nested.array[6 + 1 * 2 / 5].some.other.property}"/>
````

#### Method References
Methods can also be referenced directly and their output used. They are special in that their arguments 
can take computed expressions as well as literal values
````C#
    <SomeElement someValue="{myMethod(45, true, someField.nested.value + something)}"/>
````

#### Aliases
Various aliases can be made available to the expression engine. The most prominent use of these 
are within <Repeat> tags. Aliases are defined by the context in which the expression runs. Anything
can be an alias, the syntax is simply $ + some identifier. Important to note is that aliases are 
type checked just like all expressions, so you won't be able to accidentally mis-use them
````C#
    <SomeElement intValue="{$intAlias}"/>
````