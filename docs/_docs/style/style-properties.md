---
id: StyleProperties
title: Style Properties
layout: page
tags:
  - uiforia
  - style
---

# Style Properties  

## Clipping and Overflow
### `Overflow`, `OverflowX` and `OverflowY` : [Overflow](/docs/types#overflow)
   Overflow denotes how content that spills outside of a containing box is treated.
   The options are
   * `Hidden` Never show overflowing content
   * `Visible` (default) Always show overflowing content
   * `Scroll` Hide overflowing content and create a scroll bar
   
   Overflow can be set independently on the X and Y axes:
  
````C#
 style MyStyle {
    OverflowX = Hidden;  // set x axis to hidden
    OverflowY = Visible; // set y axis to hidden
    Overflow = Scroll; // Set both axes to scroll
} 
````

### `ClipBehavior` : [ClipBehavior](/docs/types#clipbehavior)

### `ClipBounds` : [ClipBounds](/docs/types#clipbounds)

### `PointerEvents` : [PointerEvents](/docs/types#pointerevents)

## Background
### `BackgroundColor` : [Color](/docs/misc#list-of-all-supported-colors)
Background Color paints the background of the element. It will accept color values in the following forms:
* `rgba(byte, byte, byte, byte)` 
* `rgb(byte, byte, byte)` 
* `#AABBCCDD` (hex form by channel r, g, b, a)
* any of the following color names: `black`, `blue`, `clear`, `cyan`, `gray`, `grey`, `green`, `magenta`, `red`, `white`, `yellow`

### `BackgroundTint` : [Color](/docs/misc#list-of-all-supported-colors) 
Tints the background image without affecting the background color. Accepts a color value.

### `BackgroundImageOffset` : [UIFixedLength](style-units#uifixedlength) 
Offset the texture coordinates used for the background image
````c#
style MyStyle {
    BackgroundImageOffsetX = 10; // set for x axis
    BackgroundImageOffsetY = 10; // set for y axis
    BackgroundImageOffset = 32; // set for both axes
}
````  

### `BackgroundImageScale` : [UIFixedLength](style-units#uifixedlength) 
Scale the texture coordinates used for the background image. Used to tile the image if not part of a sprite sheet
```` c#
style MyStyle {
    BackgroundImageScaleX = 2.3; // set for x axis
    BackgroundImageScaleY = 1.4; // set for y axis
    BackgroundImageScale = 0.5; // set for both axes
}
````

### `BackgroundImageRotation` : [UIFixedLength](style-units#uifixedlength) 
Rotate the texture coordinates used for the background image, in degrees.
````c#
style MyStyle {
    BackgroundImageRotation = 45;
}
````
  
### `BackgroundImageTileX` : [UIFixedLength](style-units#uifixedlength)
Renders an image on a Tilemap's x-axis ([see Unity Tilemap](https://docs.unity3d.com/Manual/class-Tilemap.html))  

### `BackgroundImageTileY : Texture` 
Renders an image on a Tilemap's y-axis ([see Unity Tilemap](https://docs.unity3d.com/Manual/class-Tilemap.html))

## Border  

### `Border` : [UIFixedLength](style-units#uifixedlength) 
Sets the border size on an element. Supports the 1-4 parameter shorthand for setting top, right, bottom and left values.

``` Border = 1px; // sets a 1px border on all sides ```  
``` Border = 5px 1px; // sets a 5px border top and bottom and a 1px border left and right ```

### `BorderTop` : [UIFixedLength](style-units#uifixedlength)
Sets a border size at the top of an element.  

### `BorderRight` : [UIFixedLength](style-units#uifixedlength)
Sets a border size on the right-side of an element.
    
### `BorderBottom` : [UIFixedLength](style-units#uifixedlength)
Sets a border size on the bottom of an element.

### `BorderLeft` : [UIFixedLength](style-units#uifixedlength)
Sets the border size on the left side of an element.
    
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

### `BorderRadius` : [UIFixedLength](style-units#uifixedlength) 
Makes the border round on the corners. Supports the 1-4 property shorthand.
```
BorderRadius = 10px 50%; // top-left and bottom-right: 10px; bottom-left and top-right: 50% of the element size.
```

#### `BorderRadiusTopLeft` : [UIFixedLength](style-units#uifixedlength) 
#### `BorderRadiusTopRight` : [UIFixedLength](style-units#uifixedlength) 
#### `BorderRadiusBottomRight` : [UIFixedLength](style-units#uifixedlength) 
#### `BorderRadiusBottomLeft` : [UIFixedLength](style-units#uifixedlength) 

### Corner Bevels
Cuts off a corner in a 45 degree angle. The size you define is the length of the cutout from the corner. 
#### `CornerBevelTopLeft` : [UIFixedLength](style-units#uifixedlength) 
#### `CornerBevelTopRight` : [UIFixedLength](style-units#uifixedlength) 
#### `CornerBevelBottomLeft` : [UIFixedLength](style-units#uifixedlength) 
#### `CornerBevelBottomRight` : [UIFixedLength](style-units#uifixedlength) 

## Alignments
There's an [extensive guide around alignments](/docs/alignments) for more details.

### `AlignmentTarget` : [AlignmentTarget](/docs/types#alignmenttarget)
Shorthand to set `AlignmentTargetX` and `AlignmentTargetY` to the same value.
### `AlignmentTargetX` : [AlignmentTarget](/docs/types#alignmenttarget)
### `AlignmentTargetY` : [AlignmentTarget](/docs/types#alignmenttarget)
### `AlignmentDirectionX` : [AlignmentDirection](/docs/types#alignmentdirection)
### `AlignmentDirectionY` : [AlignmentDirection](/docs/types#alignmentdirection)
### `AlignmentOriginX` : [OffsetMeasurement](/docs/style/style-units#offsetmeasurement)
### `AlignmentOriginY` : [OffsetMeasurement](/docs/style/style-units#offsetmeasurement)
### `AlignmentOffsetX` : [OffsetMeasurement](/docs/style/style-units#offsetmeasurement)
### `AlignmentOffsetY` : [OffsetMeasurement](/docs/style/style-units#offsetmeasurement)

### `AlignX`
### `AlignY`

## Layout  
[`LayoutType`](#layouttype--layouttype)   
[`LayoutBehavior`](#layoutbehavior--layoutbehavior)     
[`Layer`](#layer)  
[`Margin`](#margin--uimeasurement)  
[`MarginTop`](#margintop--uimeasurement)  
[`MarginBottom`](#mmarginbottom--uimeasurement)  
[`MarginLeft`](#marginleft--uimeasurement)  
[`MarginRight`](#marginright--uimeasurement)  

**Size**  
[`PreferredSize`](#preferredsize--uimeasurement)  
[`PreferredHeight`](#preferredheight--uimeasurement)  
[`PreferredWidth`](#preferredwidth--uimeasurement)  
[`MaxHeight`](#maxheight--uimeasurement)  
[`MaxWidth`](#maxwidth--uimeasurement)  
[`RadialLayoutEndAngle`](#radiallayoutendangle--float)  
[`RadialLayoutRadius`](#radiallayoutradius--float)  
[`RadialLayoutStartAngle`](#radiallayoutstartangle--float)  
[`RenderLayer`](#renderlayer--renderlayer)  
[`RenderLayerOffset`](#renderlayeroffset--int)  
[`Opacity`](#opacity--float)  
[`Padding`](#padding--uifixedlength)  
[`PaddingTop`](#paddingtop--uifixedlength)  
[`PaddingBottom`](#paddingbottom--uifixedlength)  
[`PaddingLeft`](#paddingleft--uifixedlength)  
[`PaddingRight`](#paddingright--uifixedlength)
[`Visibility`](#visibility--visibility)
[`ZIndex`](#zindex--int)  

**FlexBox**
[`FlexLayoutDirection`](#flexlayoutdirection--layoutdirection)
[`FlexItemGrow`](#flexitemgrow--int)  
[`FlexItemShrink`](#flexitemshrink--int)  
[`FlexLayoutWrap`](#flexlayoutwrap--layoutwrap)  

**Grid**  
[`GridLayoutColTemplate`](#gridlayoutcoltemplate--gridtracksize)    
[`GridLayoutRowTemplate`](#gridlayoutrowtemplate--gridtracksize)    
[`GridItemColSelfAlignment`](#griditemcolselfalignment--gridaxisalignment)    
[`GridItemRowSelfAlignment`](#griditemrowselfalignment--gridaxisalignment)    
[`GridLayoutDirection`](#gridlayoutdirection--layoutdirection)    
[`GridItemColSpan`](#griditemcolspan--int)    
[`GridItemRowSpan`](#griditemrowspan--int)    
[`GridItemColStart`](#griditemcolstart--int)    
[`GridItemRowStart`](#griditemrowstart--int)    
[`GridLayoutColStart`](#gridlayoutcolstart--int)    
[`GridLayoutRowStart`](#gridlayoutrowstart--int)    
[`GridLayoutColAlignment`](#gridlayoutcolalignment--gridaxisalignment)    
[`GridLayoutRowAlignment`](#gridlayoutrowalignment--gridaxisalignment)    
[`GridLayoutColGap`](#gridlayoutcolgap--float)    
[`GridLayoutRowGap`](#gridlayoutrowgap--float)    
[`GridLayoutMainAxisAutoSize`](#gridlayoutmainaxisautosize--gridtracksize)    
[`GridLayoutDensity`](#gridlayoutdensity--gridlayoutdensity)    
 
[`Cursor`](#cursor--cu)   
  
[`Painter`](#painter--string)    
  
**Shadow**  
[`ShadowType`](#shadowtype--shadowtype)    
[`ShadowIntensity`](#shadowitensity--float)    
[`ShadowOffsetX`](#shadowoffsetx--float)    
[`ShadowOffsetY`](#shadowoffsety--float)    
[`ShadowSoftnessX`](#shadowowsoftnessx--float)    
[`ShadowSoftnessY`](#shadowowsoftnessY--float)    

**Text**  
[`TextAlignment`](#textalignment--texttextalignment)    
[`TextColor`](#textcolor--color)    
[`TextFontAsset`](#textfontasset--tmp_fontasset)    
[`TextFontSize`](#textfontsize--uifixedlength)    
[`TextFontStyle`](#textfontstyle--textfontstyle)    
[`TextGlowColor`](#textglowcolor--color)    
[`TextGlowInner`](#textglowinner--float)    
[`TextGlowOuter`](#textglowouter--float)    
[`TextGlowOffset`](#textglowoffset--float)    
[`TextGlowPower`](#textglowpower--float)    
[`TextOutlineColor`](#textoutlinecolor--color)    
[`TextOutlineWidth`](#textoutlinewidth--float)    
[`TextShadowColor`](#textshadowcolor--color)    
[`TextShadowIntensity`](#textshadowitensity--float)    
[`TextShadowOffsetX`](#textshadowoffsetx--float)    
[`TextShadowOffsetY`](#textshadowoffsety--float)    
[`TextShadowSoftness`](#textshadowsoftness--float)    
[`TextShadowType`](#textshadowtype--shadowtype)    
[`TextTransform`](#texttransform--texttransform)    
[`TextWhitespaceMode`](#textwhitespacemode--uiforiatextwhitespacemode)

**Transform**    
[`TransformBehavior`](#transformbehavior--transformbehavior)  
[`TransformBehaviorX`](#transformbehaviorx--transformbehavior)  
[`TransformBehaviorY`](#transformbehaviory--transformbehavior)  
[`TransformPivot`](#transformpivot--uifixedlength)  
[`TransformPivotX`](#transformpivotx--uifixedlength)  
[`TransformPivotY`](#transformpivoty--uifixedlength)  
[`TransformPositionX`](#transformpositionx--transformoffset)  
[`TransformPosition`](#transformposition--transformoffset)  
[`TransformPositionX`](#transformpositionx--transformoffset)  
[`TransformPositionY`](#transformpositiony--transformoffset)  
[`TransformRotation`](#transformrotation--float)  
[`TransformScale`](#transformscale--float)  
[`TransformScaleX`](#transformscalex--float)  
[`TransformScaleY`](#transformscaley--float)  


  





### Layout

#### `Layer` 

#### `LayoutBehavior` : [LayoutBehavior](structs.md)
Set to `Ignored` to ignore the parent element's style 
 
#### `LayoutType` : [LayoutType](structs.md)
`Grid` or `Flex`  

#### `Margin` : [UIMeasurement](style-units#uimeasurement) 
Sets space around elements, outside of any defined borders.   

#### `MarginTop` : [UIMeasurement](style-units#uimeasurement) 
Sets the margin at the top.  

#### `MarginBottom` : [UIMeasurement](style-units#uimeasurement) 
Sets the margin at the bottom.  

#### `MarginLeft` : [UIMeasurement](style-units#uimeasurement) 
Sets the margin on the left.
  
#### `MarginRight` : [UIMeasurement](style-units#uimeasurement) 
Sets the margin on the right.    



  
### Size
  
#### `PreferredSize` : [UIMeasurement](style-units#uimeasurement) 
Sets the preferred size. 
```
PreferredSize = 300px, 1cnt;  
sets the height to 300px and the width to 1cnt(the size of the element's content)  
PreferredSize = 1pca, 1.5em;  
sets the height to 1pca(parent's size minus its padding and border) and the width to 1.5em(size of current font applied to element)  
```

#### `PreferredHeight` : [UIMeasurement](style-units#uimeasurement) 
Sets the preferred height.  

#### `PreferredWidth` : [UIMeasurement](style-units#uimeasurement) 
Sets the preferred width.
 
#### `MaxHeight` : [UIMeasurement](style-units#uimeasurement) 
Sets the maximum height of an element.   

#### `MaxWidth` : [UIMeasurement](style-units#uimeasurement)  
Sets the maximum width of an element.   
  
If the content is larger than the maximum height, it will overflow. 
   
#### `MinHeight` : [UIMeasurement](style-units#uimeasurement)   
Sets the minimum height of an element. 

#### `MinWidth` : [UIMeasurement](style-units#uimeasurement)  
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


#### `Padding` : [UIFixedLength](style-units#uifixedlength)   
Sets the padding of an element 
```
Padding = 5px 10px 15px 20px; //top padding is 5px, right padding is 5px, bottom padding is 15 px, left padding is 20px
Padding = 5px, 25px, 30px // top padding is 5px, right and left padding are 25px, bottom padding is 30px
Padding = 5px, 6px // top and bottom padding are 5 px, right and left padding are 6px
```
  
#### `PaddingTop` : [UIFixedLength](style-units#uifixedlength)   
Sets the padding at the top of the element.
  
#### `PaddingBottom` : [UIFixedLength](style-units#uifixedlength) 
Sets the padding at the bottom of the element.
  
#### `PaddingLeft` : [UIFixedLength](style-units#uifixedlength)  
Sets the padding on the left side of the element.
  
#### `PaddingRight` : [UIFixedLength](style-units#uifixedlength) 
Sets the padding on the right side of an element.
  
<br/>

#### `Visibility` : [Visibility](/docs/misc#types)
Shows or hides an element 
* `Visible` 
* `Hidden`
#### `ZIndex` : int
Sets the stack order of an element. 
`ZIndex = 1`

-------------------------  

###  FlexBox
Flexbox is a one-dimensional layout mode, which was designed with a flexible and consistent layout for different screen sizes. 

Flexbox is described as one-dimensional since you can only utilize one dimension at a time — either a column or a row. If you wish to use a two-dimensional layout, a grid layout will provide option of using both rows and columns.

#### Flex Container
Although flexbox is set as the default layout, you would set `LayoutType` to `flex` in the parent element which holds the children in your XML.  
  
##### `LayoutType = Flex`

#### `FlexLayoutDirection` : [LayoutDirection](/docs/misc#types)
 `FlexLayoutDirection`  sets the direction in which your elements are laid out. There are four possible values:

* `Row` aligns elements from left to right.
* `Column` aligns elements from top to bottom.
* `Horizontal` aligns elements horizontally. 
* `Vertical` aligns elements vertically. 

#### Main Axis and Cross Axis
The layout will be structured according to the main axis and the cross axis. The main axis will be defined by `FlexLayoutDirection`, and the cross axis is set perpendicular to it. 


#### `FlexMainAxisAlignment` : [MainAxisAlignment](/docs/misc#types)
Defines how to align children within the main axis.   
  
Although flexbox is set as the default layout, you would set `LayoutType` to `flex` in the parent element which holds the children in your XML.

  
* `Start` aligns children at the start of the container's main axis.     
<br/><br/>             
* `End` aligns children at the end of the container's main axis.   
<br/><br/>       
* `Center` aligns children at the center of the container's main axis.  
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's main axis.  
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child across the container's main axis.  

![flex-align](assets/img/flex-alignment.png)

    
#### `FlexCrossAxisAlignment` : [CrossAxisAlignment](/docs/misc#types)  
The cross axis is set perpendicular to the main axis. If your main axis is set to `FlexLayoutDirection` = Column, the cross axis runs along the rows.
  
* `Start` aligns children at the start of the container's cross axis.     
<br/><br/>             
* `End` aligns children at the end of the container's cross axis.
<br/><br/>       
* `Center` aligns children at the center of the container's cross axis.
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's cross axis.
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child, across the container's cross axis.
<br/><br/>  
* `Stretch` distributes children to take up the entire space in the cross axis.
     
#### `FlexItemSelfAlignment` : [CrossAxisAlignment](/docs/misc#types)  
Elements can override how they are laid out on the cross axis with this property. 
* `Start` aligns children at the start of the container's cross axis.     
<br/><br/>             
* `End` aligns children at the end of the container's cross axis.
<br/><br/>       
* `Center` aligns children at the center of the container's cross axis.
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's cross axis.
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child, across the container's cross axis.  
<br/><br/> 
* `Stretch` distributes children to take up the entire space in the cross axis.


  
#### `FlexItemGrow` : int      
When there is remaining space, defines what ratio of that extra space the element will get  
  
#### `FlexItemShrink` : int
When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element  
  
#### `FlexLayoutWrap` : [LayoutWrap](/docs/misc#types) 
Defines whether flex items area forced on a single line or multiple lines in the flexbox container  


<br/>

-------------------------

### Grid
Grid is a 2-dimensional system, which allows for both columns and rows, unlike flexbox which is 1-dimensional (rows or columns). 

#### Grid Container
To use a grid box layout, set `LayoutType` to `Grid` 
  
By default, grid elements are placed in rows and span the full width of the container.

#### Adding Columns and Rows 

#### `GridLayoutColTemplate` : [GridTrackSize](/docs/misc#types)
Specifies the number and size of columns in a grid layout.  
  
#### `GridLayoutRowTemplate` : [GridTrackSize](/docs/misc#types)
Specifies the number and size of rows in a grid layout.  

```
style grid-window {
    LayoutType = Grid;
    GridLayoutColTemplate = 1fr;
    GridLayoutRowTemplate = 100px 1fr 100px;
    PreferredSize = 800px 500px;
}
```

#### `GridItemColSelfAlignment` : [GridAxisAlignment](/docs/misc#types)
Aligns an in item inside the column of the grid container.  

#### `GridItemRowSelfAlignment` : [GridAxisAlignment](/docs/misc#types)
 Aligns in item inside the row of the grid container  

#### `GridLayoutDirection` : [LayoutDirection](/docs/misc#types)
Set to `Horizontal` or `Vertical`

#### `GridItemColSpan` : int
Element can span across multiple columns. 
      
#### `GridItemRowSpan` : int 
Set an element to span across multiple rows.

#### `GridItemColStart` : int 
The line where the column items begin.  

#### `GridItemRowStart` : int
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


<br/>

-------------------------
#### `Cursor` : [CursorStyle](/docs/misc#types)
Cursor specifies the cursor displayed when the mouse pointer is over an element.

`Cursor = url("images/cursor1.png") 15` sets the cursor image displayed over an element and the size (15 pixels)  


#### `Painter` : string
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

#### `TextFontSize` : [UIFixedLength](style-units#uifixedlength)
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

#### `TransformPivot` : [UIFixedLength](style-units#uifixedlength)

#### `TransformPivotX` : [UIFixedLength](style-units#uifixedlength)
     
`TransformPivotX = 1,1`
  
#### `TransformPivotY` : [UIFixedLength](style-units#uifixedlength)

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




