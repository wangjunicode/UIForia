---
id: StyleProperties
title: Style Properties
layout: page
tags:
  - uiforia
  - style
---

# Style Properties

Here’s a quick cheat sheet for every style property. Some support a shorthand syntax that
will be referred to as the "1-4 parameter shorthand", like `Margin`, `Padding`, `Border`
`BorderColor` or `BorderRadius`. Those properties can take up to four parameters, which 
apply the given value to the four sides (or corners in case of `BorderRadius`) of an element.
This is how the values are mapped to the sides:
- 1 value: top + right + bottom + left
- 2 values: 
    - top + bottom
    - left + right
- 3 values: 
    - top
    - right + left
    - bottom
 - 4 values:
    - top
    - right
    - bottom
    - left

Properties expect different types as values, so throughout this document you'll find quick links
to their type definitions like these: <button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
If there are big differences to their `C#` counterpart both notations will be described in the type definition.
Enum types map 1:1 to style property values, so you'll only find a table that lists all values
like we did for [Overflow](/docs/types#overflow).

Beware of experimental properties! Nothing more to say here really. Go on. 

## Overflow and Clipping
### `Overflow`

<button class="api-button">[Overflow](/docs/types#overflow)</button>
{% include alert.html type="info" title="This is a shorthand for" content="<code>OverflowX</code> and <code>OverflowY</code>" %}

Overflow denotes how elements are treated that are positioned fully or in part outside of their 
ancestor’s bounds. The options are:
* `Hidden` Never show overflowing content
* `Visible` (default) Always show overflowing content
* `Scroll` Only supported by the ScrollView element, which requires it!

Overflow can be set independently on the X and Y axes:
  
```c#
style MyStyle {
    OverflowX = Hidden;  // hides horizontal overflow
    OverflowY = Visible; // set vertical overflow to be visible
    Overflow = Hidden; // equivalent to: OverflowX = Hidden; OverflowY = Hidden;
    Overflow = Hidden Visible; // equivalent to: OverflowX = Hidden; OverflowY = Visible;
}
```

### `ClipBehavior`
<button class="api-button">[ClipBehavior](/docs/types#clipbehavior)</button>

### `ClipBounds`
<button class="api-button">[ClipBounds](/docs/types#clipbounds)</button>

### `PointerEvents`
<button class="api-button">[PointerEvents](/docs/types#pointerevents)</button>

## Background
### `BackgroundColor`
<button class="api-button">[Color](/docs/misc#list-of-all-supported-colors)</button>

Background Color paints the background of the element. It will accept color values in the following forms:
* `rgba(byte, byte, byte, byte)` 
* `rgb(byte, byte, byte)` 
* `#AABBCCDD` (hex form by channel r, g, b, a)
* any of the following color names: `black`, `blue`, `clear`, `cyan`, `gray`, `grey`, `green`, `magenta`, `red`, `white`, `yellow`

### `BackgroundImage`
<button class="api-button">[Texture URL](/docs/types#url)</button>

### `BackgroundTint`
<button class="api-button">[Color](/docs/misc#list-of-all-supported-colors)</button>
 
Tints the background image without affecting the background color. Accepts a color value.
Apply this property to `<Image>` elements to tint your image.

### `BackgroundImageOffset`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Offset the texture coordinates used for the background image
````c#
style MyStyle {
    BackgroundImageOffsetX = 10; // set for x axis
    BackgroundImageOffsetY = 10; // set for y axis
    BackgroundImageOffset = 32; // set for both axes
}
````  

### `BackgroundImageScale`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Scale the texture coordinates used for the background image. Used to tile the image if not part of a sprite sheet
```` c#
style MyStyle {
    BackgroundImageScaleX = 2.3; // set for x axis
    BackgroundImageScaleY = 1.4; // set for y axis
    BackgroundImageScale = 0.5; // set for both axes
}
````

### `BackgroundImageRotation`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Rotate the texture coordinates used for the background image, in degrees.
````c#
style MyStyle {
    BackgroundImageRotation = 45;
}
````
  
### `BackgroundImageTileX` and `BackgroundImageTileY`
<button class="api-button">float</button>

Renders an image on a Tilemap's x- or y-axis ([see Unity Tilemap](https://docs.unity3d.com/Manual/class-Tilemap.html))

## Border  

### `Border`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Sets the border size on an element. Supports the 1-4 parameter shorthand for setting top, right, bottom and left values.

``` Border = 1px; // sets a 1px border on all sides ```  
``` Border = 5px 1px; // sets a 5px border top and bottom and a 1px border left and right ```

### `BorderTop`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

### `BorderRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

### `BorderBottom`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

### `BorderLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

### `BorderColor` : [Color](/docs/misc#list-of-all-supported-colors)
Sets the color of the border for all sides. Also supports the 1-4 value shorthand.
Note: without setting a color there wouldn't be a visible border!
``` c#
Border = red green rgba(200, 200, 0, 255) #facabf; // to set a different color on every side
```

#### `BorderColorTop` : [Color](/docs/misc#list-of-all-supported-colors)
#### `BorderColorRight` : [Color](/docs/misc#list-of-all-supported-colors)
#### `BorderColorBottom` : [Color](/docs/misc#list-of-all-supported-colors)
#### `BorderColorLeft` : [Color](/docs/misc#list-of-all-supported-colors)

### `BorderRadius`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Makes the border round on the corners. Supports the 1-4 property shorthand.
```
BorderRadius = 10px 50%; // top-left and bottom-right: 10px; bottom-left and top-right: 50% of the element size.
```

#### `BorderRadiusTopLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `BorderRadiusTopRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `BorderRadiusBottomRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `BorderRadiusBottomLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
----------------

### Corner Bevels
Cuts off a corner in a 45 degree angle. The size you define is the length of the cutout from the corner.
 
#### `CornerBevelTopLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `CornerBevelTopRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `CornerBevelBottomLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
#### `CornerBevelBottomRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
----------------

## Alignments
There's an [extensive guide around alignments](/docs/alignments) for more details.

### `AlignmentTarget`
<button class="api-button">[AlignmentTarget](/docs/types#alignmenttarget)</button>

Shorthand to set `AlignmentTargetX` and `AlignmentTargetY` to the same value.

### `AlignmentTargetX`
<button class="api-button">[AlignmentTarget](/docs/types#alignmenttarget)</button>

### `AlignmentTargetY`
<button class="api-button">[AlignmentTarget](/docs/types#alignmenttarget)</button>

### `AlignmentDirectionX`
<button class="api-button">[AlignmentDirection](/docs/types#alignmentdirection)</button>

### `AlignmentDirectionY`
<button class="api-button">[AlignmentDirection](/docs/types#alignmentdirection)</button>

### `AlignmentOriginX`
<button class="api-button">[OffsetMeasurement](/docs/types#offsetmeasurement)</button>

### `AlignmentOriginY`
<button class="api-button">[OffsetMeasurement](/docs/types#offsetmeasurement)</button>

### `AlignmentOffsetX`
<button class="api-button">[OffsetMeasurement](/docs/types#offsetmeasurement)</button>

### `AlignmentOffsetY`
<button class="api-button">[OffsetMeasurement](/docs/types#offsetmeasurement)</button>

### `AlignX`
Shorthand to set all the horizontal alignment properties at once:
`AlignX = AlignmentOriginX [AlignmentOffsetX] [AlignmentTargetX] [AlignmentDirectionX]`

### `AlignY`
Shorthand to set all the vertical alignment properties at once:
`AlignY = AlignmentOriginY [AlignmentOffsetY] [AlignmentTargetY] [AlignmentDirectionY]`

----------------

### `LayoutFit`
<button class="api-button">[LayoutFit](/docs/types#layoutfit)</button>

Shorthand for settings `LayoutFitHorizontal` and `LayoutFitVertical` at once.
Accepts one or two values to set both properties to the same or different values respectively. 

Have a look at [the LayoutFit guide](/docs/layout/#layoutfit) for more information.

### `LayoutFitHorizontal`
<button class="api-button">[LayoutFit](/docs/types#layoutfit)</button>

### `LayoutFitVertical`
<button class="api-button">[LayoutFit](/docs/types#layoutfit)</button>

### `LayoutBehavior`
<button class="api-button">[LayoutBehavior](/docs/types#layoutbehavior)</button>

This property lets you opt-out of the normal layout flow.

- `Ignored` is usually used with [alignments](/docs/alignments) or transforms. To
    place an element at a specific place without affecting the layout of siblings.
    In a grid it will *not* count towards any cell in a row or column track. The
    parent's size will not be affected by the Ignored element.
 
- `TranscludeChildren` is automatically applied to all `<Children />` elements but
    can be applied to any user element as well. Useful when a dedicated custom
    UIElement is created to wrap some functionality but some critical style requirements
    apply.
 
### `LayoutType`
- [Flex](/docs/layout/flex)
- [Grid](/docs/layout/grid)
- [Stack](/docs/layout/stack)

### `Margin` : [UIMeasurement](/docs/types#uimeasurement) 
1-4 parameter shorthand. Sets space around elements, outside of any defined borders.   

### `MarginTop` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the margin at the top.  

### `MarginBottom` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the margin at the bottom.  

### `MarginLeft` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the margin on the left.
  
### `MarginRight` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the margin on the right.    
  
### `PreferredSize` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the preferred size. 
```
PreferredSize = 300px, 1cnt;  
sets the height to 300px and the width to 1cnt(the size of the element's content)  
PreferredSize = 1pca, 1.5em;  
sets the height to 1pca(parent's size minus its padding and border) and the width to 1.5em(size of current font applied to element)  
```

#### `PreferredHeight` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the preferred height.  

#### `PreferredWidth` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the preferred width.
 
#### `MaxHeight` : [UIMeasurement](/docs/types#uimeasurement) 
Sets the maximum height of an element.   

#### `MaxWidth` : [UIMeasurement](/docs/types#uimeasurement)  
Sets the maximum width of an element.   
  
If the content is larger than the maximum height, it will overflow. 
   
#### `MinHeight` : [UIMeasurement](/docs/types#uimeasurement)   
Sets the minimum height of an element. 

#### `MinWidth` : [UIMeasurement](/docs/types#uimeasurement)  
Sets the minimum width of an element 
  
If the content is smaller than the minimum height, the minimum height will be applied.
    
MinHeight will always override MaxHeight.  
    
  
#### `RadialLayoutEndAngle` : float
    
#### `RadialLayoutRadius` : float  
  
#### `RadialLayoutStartAngle` : float

#### `RenderLayer` : [RenderLayer](structs.md) 
Renders layer based on selected value: `Unset`, `Default`, `Parent`, `Template`, `Modal`, `View`, or `Screen`

#### `RenderLayerOffset` : int


#### `Opacity` : float
Sets the opacity of the element from 0.0 (transparent) to 1.0 (opaque). 
       
The default value is always 1 (100% opacity). Changing the value (0.5) will make the element less transparent. 
  
Opacity will automatically apply to the parent's child elements unless specified.


#### `Padding`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
   
Sets the padding of an element 
```
Padding = 5px 10px 15px 20px; //top padding is 5px, right padding is 5px, bottom padding is 15 px, left padding is 20px
Padding = 5px, 25px, 30px // top padding is 5px, right and left padding are 25px, bottom padding is 30px
Padding = 5px, 6px // top and bottom padding are 5 px, right and left padding are 6px
```
  
#### `PaddingTop`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
   
Sets the padding at the top of the element.
  
#### `PaddingBottom`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Sets the padding at the bottom of the element.
  
#### `PaddingLeft`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
  
Sets the padding on the left side of the element.
  
#### `PaddingRight`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>
 
Sets the padding on the right side of an element.
  
### `Visibility` : [Visibility](/docs/misc#types)
Shows or hides an element 
* `Visible` 
* `Hidden`

### `ZIndex` : int
Sets the stack order of an element. 
`ZIndex = 1`

### `FlexLayoutDirection` : [LayoutDirection](/docs/misc#types)
 `FlexLayoutDirection`  sets the direction in which your elements are laid out. There are four possible values:

* `Horizontal` aligns elements horizontally. 
* `Vertical` aligns elements vertically. 
  
### `FlexItemGrow` : int      
When there is remaining space, defines what ratio of that extra space the element will get  
  
### `FlexItemShrink` : int
When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element  
  
### `FlexLayoutWrap` : [LayoutWrap](/docs/misc#types) 
Defines whether flex items area forced on a single line or multiple lines in the flexbox container  
 

### `GridLayoutColTemplate`
<button class="api-button">[GridTrackSize](/docs/types#gridtracksize)</button>

Accepts multiple values. Each value is a `GridTrackSize` and defines the width of the column track.
[Grid Cell Functions](/docs/layout/grid#grid-cell-functions) may also be used instead of mixed with
`GridTrackSizes`: `GridLayoutColTemplate = 100px cell(0, 1fr, 1fr, 0, infinite) 100px;`

### `GridLayoutRowTemplate`
<button class="api-button">[GridTrackSize](/docs/types#gridtracksize)</button>

Accepts multiple values. Each value is a `GridTrackSize` and defines the height of the row track.
[Grid Cell Functions](/docs/layout/grid#grid-cell-functions) may also be used instead of mixed with
`GridTrackSizes`: `GridLayoutRowTemplate = 100px cell(0, 1fr, 1fr, 0, infinite) 100px;`
```
style grid-window {
    LayoutType = Grid;
    GridLayoutColTemplate = 1fr;
    GridLayoutRowTemplate = 100px 1fr 100px;
    PreferredSize = 800px 500px;
}
```

#### `GridLayoutDirection` : [LayoutDirection](/docs/misc#types)
Set to `Horizontal` or `Vertical`

#### `GridItemWidth` : int
Element can span across multiple columns. 
      
#### `GridItemHeight` : int 
Set an element to span across multiple rows.

#### `GridItemX` : int 
The line where the column items begin.  

#### `GridItemY` : int
The line where row items begin.  

#### `GridLayoutColAlignment` : [GridAxisAlignment](/docs/misc#types)
The parent grid's column alignment.   

#### `GridLayoutRowAlignment` : [GridAxisAlignment](/docs/misc#types) 
The parent grid's row alignment.   

#### Grid Gaps
The `GridLayoutColGap` and `GridLayoutRowGap` properties create spaces between columns and rows.

Gaps are created in between rows and columns, rather than the edge of the container. 

#### `GridLayoutColGap` : float
Specifies size of gaps between columns.
```
// <length> values
GridLayoutColGap = 5px;
GridLayoutColGap = 1em;

// <decimal> values
GridLayoutColGap = 0.3
```
 
#### `GridLayoutRowGap` : float
Specifies size of gaps between rows.

#### `GridLayoutMainAxisAutoSize` : [GridTrackSize](/docs/misc#types)
Main axis auto-sizing if the grid doesn't have explicit sizing. 

#### `GridLayoutCrossAxisAutoSize` : [GridTrackSize](/docs/misc#types)
Cross axis auto-sizing if the grid doesn't have explicit sizing  

#### `GridLayoutDensity` : [GridLayoutDensity](/docs/misc#types)
Place grid items in empty spaces based on their size 

`GridLayoutDensity` = Dense;


```

```      

-------------------------
#### `Cursor`
<button class="api-button">[CursorStyle](/docs/types#cursorstyle)</button>

Sets the cursor if the mouse pointer is over an element.

`Cursor = url("images/cursor1.png") 15` changes the cursor texture to `cursor1.png` and sets 
the cursor hot spot x and y to be 15px. For more information about cursors in unity 
[check their docs](https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html).  

The cursor mode cannot be changed, we're always using `CursorMode.Auto`.

#### `Painter`
Allows you to draw graphics. More information on Painter can be found [here](Painter.md)


### Shadow 
#### `ShadowType` : [ShadowType](/docs/misc#types)
Adds an outline effect to an element (such as text or an image)  
  
#### `ShadowIntensity` : float
Sets the intensity of the shadow.
  
#### `ShadowOffsetX` : float 
Offsets the shadow on the x-axis.
  
#### `ShadowOffsetY` : float
Offsets the shadow on the y-axis.
  
#### `ShadowSoftnessX` : float
Sets the softness of the shadow on the x-axis.  
  
#### `ShadowSoftnessY` : float
Sets the softness of the shadow on the y-axis.

Soft shadow will wrap around the object, whereas a hard shadow will produce a shadow with a sharper edge.  
  

-------------------------
### Text

#### `TextAlignment` : [Text.TextAlignment](/docs/misc#types)
Aligns text within an element.   
* `Left`
* `Center` 
* `Right`

#### `TextColor` : [Color](/docs/misc#list-of-all-supported-colors) 
Sets the color of the text

#### `TextFontAsset` : [TMP_FontAsset](/docs/misc#types) 
Sets the specified font. 
`TextFontAsset = url("Fonts/Burbank");`

#### `TextFontSize`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

Sets the size of the font  
`TextFontSize = 10px;`

#### `TextFontStyle` : [Text.FontStyle](/docs/misc#types)
Sets the font style of the text.
   
#### `TextGlowColor` : [Color](/docs/misc#list-of-all-supported-colors) 
Sets the color of the text's glow.
  
#### `TextGlowInner` : float 
Sets the inner glow of the text.
  
#### `TextGlowOffset` : float
Sets the offset of the text's glow.

#### `TextGlowOuter` : float 
     
#### `TextGlowPower` : float   
 
#### `TextOutlineColor` : [Color](/docs/misc#list-of-all-supported-colors) 
  
#### `TextOutlineWidth` : float
Sets the outline width of the text.


#### `TextShadowColor` : [Color](/docs/misc#list-of-all-supported-colors) 
Sets the color of the text's shadow.

#### `TextShadowIntensity` : float
Sets the intensity of the text's shadow.

#### `TextShadowOffsetX` : float
Sets the offset of the shadow on the x-axis
  
#### `TextShadowOffsetY` : float
Sets the offset of the shadow on the y-axis

#### `TextShadowSoftness` : float 
Sets the softness of the text's shadow

#### `TextShadowType` : [ShadowType](/docs/misc#types)
Sets the shadow type of the text

#### `TextTransform` : [TextTransform](/docs/misc#types)
 Modifies the capitalization of text
* `UpperCase`
* `LowerCase`
* `SmallCaps`
* `TitleCase`

#### `TextWhitespaceMode` : [UIForia.Text.WhitespaceMode](/docs/misc#types)

-------------------------
### Transform
Transform can be used to rotate, scale, pivot, or position an element by overwriting the coordinates.

#### `TransformBehavior` : [TransformBehavior](/docs/misc#types)
      
* `LayoutOffset`  
* `AnchorMinOffset`  
* `AnchorMaxOffset`  
* `PivotOffset`  
  
#### `TransformBehaviorX` : [TransformBehavior](/docs/misc#types)

#### `TransformBehaviorY` : [TransformBehavior](/docs/misc#types)

#### `TransformPivot`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>


#### `TransformPivotX`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>

     
`TransformPivotX = 1,1`
  
#### `TransformPivotY`
<button class="api-button">[UIFixedLength](/docs/types#uifixedlength)</button>


#### `TransformPosition` : [TransformOffset](/docs/misc#types)

#### `TransformPositionX` : [TransformOffset](/docs/misc#types)

#### `TransformPositionY` : [TransformOffset](/docs/misc#types)

#### `TransformRotation` : float

#### `TransformScale` : float
Scales element size.

 
   
![transformscale](/assets/img/transformscale.gif)

```
style box {
    PreferredSize = 200px;
    BackgroundColor = blue;
    
    [hover] {
        TransformScale = 1.1;
}
```

#### `TransformScaleX` : float

#### `TransformScaleY` : float




