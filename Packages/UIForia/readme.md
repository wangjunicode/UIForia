# Styles
    
#### Overflow
   Overflow denotes how content that spills outside of a containing box is treated.
   The options are
   * `Hidden` Never show overflowing content
   * `Visible` (default) Always show overflowing content
   * `Scroll` Hide overflowing content and create a scroll bar
   
   Overflow can be set independently on the X and Y axes.
  
````C#
 style MyStyle {
    OverflowX = Hidden;  // set x axis to hidden
    OverflowY = Visible; // set y axis to hidden
    Overflow = Scroll; // Set both axes to scroll
}
````
#### Background Color
Background Color paints the background of the element. It will accept color values in the following forms:
* rgba(byte, byte, byte, byte) 
* rgb(byte, byte, byte) 
* #AABBCCDD (hex form by channel r, g, b, a)
* any of the following color names: `black`, `blue`, `clear`, `cyan`, `gray`, `grey`, `green`, `magenta`, `red`, `white`, `yellow`

#### Background Tint : Color
Tints the background image without affecting the background color. Accepts a color value.

#### BackgroundImageOffset : float
Offset the texture coordinates used for the background image
````c#
style MyStyle {
     BackgroundImageOffsetX = 10; // set for x axis
     BackgroundImageOffsetY = 10; // set for y axis
     BackgroundImageOffset = 32; // set for both axes
}
````

#### BackgroundImageScale : float
Scale the texture coordinates used for the background image. Can be used to tile the image if not part of a sprite sheet
````c#
style MyStyle {
     BackgroundImageScaleX = 2.3; // set for x axis
     BackgroundImageScaleY = 1.4; // set for y axis
     BackgroundImageScale = 0.5; // set for both axes
}
````
#### BackgroundImageRotation : float
Rotate the texture coordinates used for the background image, in degrees.
````c#
style MyStyle {
     BackgroundImageRotation = 45;
}
````

#### BackgroundImage : Texture
Sets the background image used.
````c#
style MyStyle {
     BackgroundImage = url("path/to/texture"); // set a texture
     BackgroundImage = url("path/to/sprite/atlas", "spriteName"); // set a texture from a sprte atlas
}
````


# Expressions
Expressions in templates drive the entire data binding system. They can support literal values as well as much more complicated
structures like property access, method calls, comparison operators, math operators, boolean logic and pretty much
any other operator you might use in C#.


#### Literal values
````C#
    <SomeElement literalIntValue="1"/>
    <SomeElement literalFloatValue="1}"/>
    <SomeElement literalDoubleValue="1.0"/>
    <SomeElement literalBooleanValue="true"/>
    <SomeElement literalStringValue="'string value'"/>
    
````

#### Math operators
````C#
     <SomeElement addition="10f + 4"/>
     <SomeElement subtraction="10f - 4"/>
     <SomeElement multiplication="10f * 4"/>
     <SomeElement division="10f / 4"/>
     <SomeElement modulus="10f % 4"/>
     <SomeElement complexOperators="50f * 3 - 4f / 25"/>
     <SomeElement parenOperator="50f * (3 - 4f) / 25"/>
````

#### String concat

````C#
     Any expression used with a + and a string operand will be cast to a string and concatenated
     {'some string' + (50 * 2) }
     {'more strings' + 'some other string'}
````
    
#### Comparisons
````C#
    50 > 14 
    255 < 25
    67 + 25 <= 1
    52 * 5 >= 67 * 3
    67 != 41
    88 == 88
````

#### Ternary
````C#
    <SomeElement someValue="76 > 18 ? 'string one' : 'string two'"/>
````

#### Unaries
````C#
    <SomeElement someBoolValue="!(1 > 2)"/>
    <SomeElement someIntValue="-(1 + 2)"/>
````

#### Property Access
Any property on an element can be accessed. 
````C#
    <SomeElement someValue="someFieldOnTheType"/>
    <SomeElement someValue="someArrayField[6]"/>
    <SomeElement someValue="some.nested.array[6].some.other.property"/>
    <SomeElement someValue="some.nested.array[6 + 1 * 2 / 5].some.other.property"/>
````

#### Method References
Methods can also be referenced directly and their output used. They are special in that their arguments 
can take computed expressions as well as literal values
````C#
    <SomeElement someValue="myMethod(45, true, someField.nested.value + something)"/>
````

#### Aliases
Various aliases can be made available to the expression engine. The most prominent use of these 
are within <Repeat> tags. Aliases are defined by the context in which the expression runs. Anything
can be an alias, the syntax is simply $ + some identifier. Important to note is that aliases are 
type checked just like all expressions, so you won't be able to accidentally mis-use them
````C#
    <SomeElement intValue="$intAlias"/>
````