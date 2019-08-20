---
id: doc1
title: Welcome to UIForia
---

## UIForia User Guide
UIForia (/juːˈfɔːriə/) is a stylable template based UI system.  
  
  
  
This page is an overview of the UIForia documentation and related resources.


## Add UIForia to Unity

In this section, we will show you how to install the framework in Unity. 



## Structure of UIForia
These three file types make up the entire structure of your UI:  
`.xml` defines the structure of the user interface   
`.style` defines the style properties  
`.cs` links visual elements to user interaction

There is a *one-to-one* relationship between templates and elements. 



## Box Model
Unlike CSS there is only one box model in UIForia, which is the equivalent to the CSS border-box model.
The content width of an element will be reduced by border and padding values. Defining a width of 100% and 
additionally a border and/or padding will not result in a box bigger than 100%. An element with a fixed 
width of 100px on the other hand will "eat up" border and padding, effectively shrinking the content box.

## Syntax

Keyword           | Description
----------------- |:------------------------------------------------------                                    
`export`          | Used in conjunction with `const`; exported constants can be imported by other scripts.
`const`           | Defines a constant that can be referenced throughout the style file.
`import`          | Imports constants or style definitions for other style files.
`as`              | Renames a constant in the local context. More details in the section about imports.
`from`            | Specify the style file location that should be imported. More details in the section about imports.
`style`           | Creates a style definition.
`[attr:attrName]` | Used inside a style definition to create an attribute sub-style-group. See the attribute section below for more.
`not` / `and`     | Used in conjunction with attribute style definitions. See the attribute section below for more.

### Example
```
// place single line comments everywhere

// export theme constants e.g.
const redVal = 200;
export const color0 = rgba(@redVal, 0, 0, 1);
export const baseDirection = Vertical;

// reference UIForia enums 
const anotherDirection = Horizontal;

// import exported constants from other style files
import vars as Constants from "file.style";

// create an element style which will be applied to all elements of that kind 
style <Group> {
    
}

// create a style definition that can be assigned in the template via the style attribute
style MattsStyleSheet {

   // styles not applied in a block implicitly belong to the 'default' group
   // states applies in a block not wrapped in a group block belong to the 'default' group

   // example:
   // [hover] {}

   [attr:attrName] {
       [state-name-here] {
           BackgroundColor = @Constants.color0;
       }
   }

   [attr:attrName="value"] {}

   [attr:attrName="value"] and [attr:other] {}

   not [attr:attrName] {}

   not [$siblingIndex % 3 == 0] {
       MarginTop: @vars.margin1;
   }

   not [$siblingIndex > 2] {
       @use styleNameHere;
   }

   [state-name-here] {
       // block of styles
       StyleName = StyleValue;
   }

}

audio xy {
    
}



<TagName> {
    [attr:other] {
        
    }
    
   
}

```


##Style by Attributes
So you want to add a special style for your element based on its attributes?
```<Input x-disabled="true" text="'some text'" style="button"/>```
Be aware of UIForia's distinction between attributes and properties. To set an attribute you have to use the
`x-` notation. To add a stylable `disabled` attribute you have to write `x-disabled` and assign your value.
UIForia attributes always expect plain strings as values in contrast to properties, which expect expressions (hence 
the double quoting when passing strings).

After defining an attribute in your element go to your style definition and add an attribute style group like this:
```
style button {
    BackgroundColor = rgba(240, 240, 240, 255);             // almost white if not disabled
    [attr:disabled="true"] {
        BackgroundColor = rgba(190, 190, 190, 255);         // a bit grey if the disabled attribute has the string value "true"
        [hover] {
            BackgroundColor = rgba(100, 100, 100, 255);     // hover the disabled attribute and it becomes very dark
        }
    }
}
```
stylepropertymap
# Styles
### Units
UIMeasurement -- used for things that need to measured and can relate to other element sizes.
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `ParentContentArea` in style sheets: `pca` The measure of the parents size minus it's padding and border
 * `ParentSize` in style sheets: `psz` The total measure of the parent's size
 * `Em` in style sheets: `em` the em size of the current font applied to the element
 * `Content` in style sheets: `cnt` the size of the element's content
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `AnchorWidth` in style sheets: `aw` the absolute distance between the element's `AnchorLeft` and `AnchorRight`
 * `AnchorHeight` in stylesheets: `ah` the absolute distance between the element's `AnchorTop` and `AnchorBotom`
 
UIFixedLength -- used for tings that have lengths relative to an element
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `Percent` in style sheets: `%` the percentage of an element's size on the related axis
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `Em` in style sheets: `em` the em size of the current font applied to the element

GridTemplate -- used for defining grid layouts
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `MaxContent` in style sheets: `mx` the max of the sizes of all elements in a given row or column
 * `MinContent` in style sheets: `mn` the min of the sizes of all elements in a given row or column
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `Em` in style sheets: `em` the em size of the current font applied to the element
 * `FractionalRemaining` in style sheets: `fr` a fractional portion of the unallocated space in a row or column
 * `ParentContentArea` in style sheets: `pca` The measure of the parents size minus it's padding and border
 * `ParentSize` in style sheets: `psz` The total measure of the parent's size

Transform -- units for defining how an element's position can be transformed
 * `Pixel` in style sheets: `px` -- one device pixel
 * `ActualWidth` in style sheets: `w` the actual width of the element, regardless of space allocated for the element
 * `ActualHeight` in style sheets: `h` the actual height of the element, regardless of space allocated for the element
 * `AllocatedWidth` in style sheets: `alw` the space the element's parent allocates to this element on the x axis
 * `AllocatedHeight` in style sheets: `alh` the space the element's parent allocates to this element on the y axis
 * `ContentWidth` in style sheets: `cw` the width of the element's content
 * `ContentHeight` in style sheets: `ch` the height of the element's content
 * `ContentAreaWidth` in style sheets: `caw` the width of the element's content minus the elements padding and border on the x axis
 * `ContentAreaHeight` in style sheets: `cah` the height of the element's content minus the elements padding and border on the y axis
 * `AnchorWidth` in style sheets: `aw` the absolute distance between the element's `AnchorLeft` and `AnchorRight`
 * `AnchorHeight` in stylesheets: `ah` the absolute distance between the element's `AnchorTop` and `AnchorBotom`
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `ParentWidth` in style sheets: `pw` the width of the parent including border and padding on x axis
 * `ParentHeight` in style sheets: `ph` the height of the parent including border and padding on y axis
 * `ParentContentAreaWidth` in style sheets: `pcaw` the width of the parent excluding border and padding on x axis 
 * `ParentContentAreaHeight` in style sheets: `pcah` the height of the parent excluding border and padding on y axis 
 * `ScreenWidth` in style sheets: `sw` the width of the screen
 * `ScreenHeight` in style sheets: `sh` the height of the screen

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
#### Background Color : Color
Background Color paints the background of the element. It will accept color values in the following forms:
* `rgba(byte, byte, byte, byte)` 
* `rgb(byte, byte, byte)` 
* `#AABBCCDD` (hex form by channel r, g, b, a)
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

#### Layout
### Flex Box
To use a flex box layout, set layout type to `Flex`
##### Properties
* `FlexLayoutDirection` : LayoutDirection -- Which direction to layout elements, `Row` or `Column`
* `FlexLayoutMainAxisAlignment` : MainAxisAlignment -- How to align items on the Main axis. The main axis the axis that is set by `FlexLayoutDirection`
* `FlexLayoutCrossAxisAlignment` : CrossAxisAlignment -- How to align items on the Cross axis. The cross axis is the axis that is not set by `FlexLayoutDirection`
* `FlexItemSelfAlignment` : CrossAxisAlignment -- Elements can override how they are laid out on the cross axis with this property
* `FlexItemGrow` : int -- When there is remaining space, defines what ratio of that extra space the element will get
* `FlexItemShrink` : int -- When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element

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

#### Input Event Bindings

 Attribute      | Template Attribute | $event          | Description
----------------|--------------------|-----------------|--------------------------------------------------
 OnMouseMove    | onMouseMove        | MouseInputEvent | Fires if the mouse position changed
 OnMouseEnter   | onMouseEnter       | MouseInputEvent | The mouse enters the bounds of the element
 OnMouseHover   | onMouseHover       | MouseInputEvent | The mouse wanders within the bounds of the element
 OnMouseExit    | onMouseExit        | MouseInputEvent | The mouse left the bounds of the element
 OnMouseDown    | onMouseDown        | MouseInputEvent | Any mouse button is down
 OnMouseUp      | onMouseUp          | MouseInputEvent | Any mouse button is up
 OnMouseClick   | onMouseClick       | MouseInputEvent | The mouse clicked the element with the left button. Need double or triple click? Use the MouseInputEvent's IsDoubleClick and IsTripleClick properties.
 OnMouseContext | onMouseContext     | MouseInputEvent | Use this for right click behavior
 OnMouseWheel   | onMouseScroll      | MouseInputEvent | Fires when you roll your mouse wheel
 
 Attribute      | Template Attribute | $event           | Description
----------------|--------------------|------------------|--------------------------------------------------
 OnDragCreate   | onDragCreate       | MouseInputEvent  | Fires when dragging starts. All mouse buttons can drag.
 OnDragMove     | onDragMove         | DragEvent [^1]   | Fires when the mouse moves and still drags.
 OnDragEnter    | onDragEnter        | DragEvent [^1]   | Fires when this element is entered while dragging. Activate a drop area with this event maybe?
 OnDragHover    | onDragHover        | DragEvent [^1]   | Fires when your mouse hovers an element while dragging.
 OnDragExit     | onDragExit         | DragEvent [^1]   | Fires when this element is exited while dragging.
 OnDragDrop     | onDragDrop         | DragEvent [^1]   | You just dropped whatever you were dragging. Right here. 
 OnDragCancel   | onDragCancel       | DragEvent [^1]   | Hit escape while dragging? Use this if you want to do something if dragging stopped unnaturally.
 
[^1] When implementing a custom OnDragCreate handler you have to return a custom DragEvent. You can cast $event to 
your custom DragEvent type in those event handlers.
                                                                                                                                                    

 Attribute      | Template Attribute | $event           | Description
----------------|--------------------|------------------|--------------------------------------------------
 OnDragCreate   | onDragCreate       | MouseInputEvent  | Fires when dragging starts. All mouse buttons can drag.   

    