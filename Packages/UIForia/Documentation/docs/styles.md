---
id: styles
title: Styles
---



## Units
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

## Background
````
#### BackgroundColor = Color
Background Color paints the background of the element. It will accept color values in the following forms:
* `rgba(byte, byte, byte, byte)` 
* `rgb(byte, byte, byte)` 
* `#AABBCCDD` (hex form by channel r, g, b, a)
* any of the following color names: `black`, `blue`, `clear`, `cyan`, `gray`, `grey`, `green`, `magenta`, `red`, `white`, `yellow`

#### BackgroundTint = Color
Tints the background image without affecting the background color. Accepts a color value.

#### BackgroundImageOffset = float
Offset the texture coordinates used for the background image
````c#
style MyStyle {
     BackgroundImageOffsetX = 10; // set for x axis
     BackgroundImageOffsetY = 10; // set for y axis
     BackgroundImageOffset = 32; // set for both axes
}
````

#### BackgroundImageScale = float
Scale the texture coordinates used for the background image. Used to tile the image if not part of a sprite sheet
````c#
style MyStyle {
     BackgroundImageScaleX = 2.3; // set for x axis
     BackgroundImageScaleY = 1.4; // set for y axis
     BackgroundImageScale = 0.5; // set for both axes
}
````
#### BackgroundImageRotation = float
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

**`BackgroundImage`** = url("images/background.jpg") sets a background image  
`BackgroundImageOffsetX` offsets the texture coordinates (x-axis) used for the background image
`BackgroundImageOffsetY` offsets the texture coordinates (y-axis) used for the background image
`BackgroundImageRotation` rotates the texture coordinates used for the background image, in degrees                     
`BackgroundImageScaleX` scales the background image by its x-axis  
`BackgroundImageScaleY` scales the background image by its y-axis  
`BackgroundImageTileX` Renders an image on a Tilemap's x-axis (https://docs.unity3d.com/Manual/class-Tilemap.html)  
`BackgroundImageTileY` Renders an image on a Tilemap's y-axis (https://docs.unity3d.com/Manual/class-Tilemap.html)


##Border
Border 
Border = 2px;

`BorderBottom` sets a border on the bottom element  
`BorderColor` sets a color of the border  
`BorderColorBottom` sets the color of the border on the bottom element  
`BorderColorLeft` sets a color of the border on the left  
`BorderColorRight` sets the color of the border on the right element  
`BorderColorTop` sets the color of the border on the top element   
`BorderLeft ` sets the border on the left element  
`BorderRadius` sets a rounded border to an element   
`BorderRadiusBottom` sets a rounded border to the bottom element   
`BorderRadiusLeft` sets a rounded border on the left-side of an element   
`BorderRadiusRight` sets a rounded border on the right-sid of an element  
`BorderRadiusTop` sets a rounded border at the top of an element  
`BorderRight` sets a border on the right-size of an element  
`BorderTop` sets a border at the top of an element  


#### Layout
### FlexBox
To use a flexbox layout, set layout type to `Flex`
##### Properties
* `FlexLayoutDirection` = LayoutDirection -- Which direction to layout elements, `Row` or `Column`
* `FlexLayoutMainAxisAlignment` = MainAxisAlignment -- How to align items on the Main axis. The main axis the axis that is set by `FlexLayoutDirection`
* `FlexLayoutCrossAxisAlignment` = CrossAxisAlignment -- How to align items on the Cross axis. The cross axis is the axis that is not set by `FlexLayoutDirection`
* `FlexItemSelfAlignment` = CrossAxisAlignment -- Elements can override how they are laid out on the cross axis with this property
* `FlexItemGrow` = int -- When there is remaining space, defines what ratio of that extra space the element will get
* `FlexItemShrink` = int -- When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element

### Grid
To use a grid box layout, set layout type to `Grid`
##### Properties
* `GridLayoutColTemplate` = int --Specifies the number and size of columns in a grid layout
* `GridLayoutRowTemplate` = int --specifies the number and size of rows in a grid layout
* `GridItemColSelfAlignment` = ColAlignment --aligns an in item inside the column of the grid container
* `GridItemRowSelfAlignment` = RowAlignment -- aligns an in item inside the row of the grid container
* `GridItemColSpan` = int --Allows the element to span across multiple columns 
* `GridItemColStart` = int --The line where the column items begin
* `GridItemRowSpan` = int --Allows the element to span across multiple rows
* `GridItemRowStart` = int  --The line where row items begin
* `GridLayoutColAlignment` = ColumnAlignment --the parent grid's column alignment 
* `GridLayoutRowAlignment` = the parent grid's row alignment 
* `GridLayoutColGap` = int --Sets the size of the gap between columns

* `GridLayoutRowGap` = --Specifies the number of rows in a grid layout

* `GridLayoutMainAxisAutoSize` = --main axis auto sizing if the grid doesn't have explicit sizing
* `GridLayoutCrossAxisAutoSize`= --cross axis auto sizing if the grid doesn't have explicit sizing
* `GridLayoutDensity` = place grid items in empty spaces based on their size //dense
* `GridLayoutDirection set the grid's layout to **Horizontal** or **Vertical**







* GridItemColSelfAlignment = Start; // places the item at the start of the column 
* GridItemColSelfAlignment = End; // places the item at the end of the column
* GridItemColSelfAlignment = Center; // places the item at the center of the column 
* GridItemColSelfAlignment = Shrink; // shrinks to fit the content
* GridItemColSelfAlignment = Grow; // grows to fit the content 
* GridItemColSelfAlignment = Fit; // grows or shrinks to fit the content


GridItemColSpan` = 2; // spans across two columns 


GridItemColStart: 1;


GridItemRowSpan = 3; // spans across 3 rows


GridItemRowStart = 2; the element within the row will start on the 2nd row


GridLayoutColTemplate = 1mx, 1fr // length is 1mx (max size of all elements in the column) and the width is 1fr (factional portion of unallocated space in the column)

# Example


AnchorBottom

AnchorLeft

AnchorRight

AnchorTarget

AnchorTop











               



Cursor specifies the cursor displayed when the mouse pointer is over an element
Cursor = url("images/cursor1") 15; sets the image displayed over an element and the size (15 pixels)

FlexItemGrow sets the flex grow factor to a flex item

FlexItemOrder: 

FlexItemSelfAlignment sets alignment of flex items 

FlexItemShrink

FlexLayoutCrossAxisAlignment

FlexLayoutDirection: sets the order of the flexbox container


FlexLayoutMainAxisAlignment: sets alignment of the main axis. 
FlexLayoutMainAxisAlignment: Start;
FlexLayoutMainAxisAlignment: End;
FlexLayoutMainAxisAlignment: Center;
FlexLayoutMainAxisAlignment: SpaceAround;
FlexLayoutMainAxisAlignment: SpaceBetween;

If you've used CSS, this is equivalent to justify-content


FlexLayoutWrap: defines whether flex items area forced on asingle line or multiple lines in the flexbox container

FlexLayoutWrap: Wrap;
The flex items will appear on a single line
FlexLayoutWrap: NoWrap;
The flex items will display on multiple lines. Direction is defined by FlexLayoutDirection
FlexLayoutWrap: WrapReverse;
The flex items will display on multiple lines. Opposite direction defined by FlexLayoutDirection.

* `FlexLayoutDirection` = LayoutDirection -- Which direction to layout elements, `Row` or `Column`
* `FlexLayoutMainAxisAlignment` = MainAxisAlignment -- How to align items on the Main axis. The main axis the axis that is set by `FlexLayoutDirection`
* `FlexLayoutCrossAxisAlignment` = CrossAxisAlignment -- How to align items on the Cross axis. The cross axis is the axis that is not set by `FlexLayoutDirection`
* `FlexItemSelfAlignment` = CrossAxisAlignment -- Elements can override how they are laid out on the cross axis with this property
* `FlexItemGrow` = int -- When there is remaining space, defines what ratio of that extra space the element will get
* `FlexItemShrink` = int -- When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element




Layer high-level z-indez 



LayoutBehavior
LayoutBehavior = ignored;



LayoutType initializes layout to Grid or Flex

Margin sets space around elements, outside of any defined borders 

Margin = 25px 50px 75px 100px;

Margin = 100px 150px 200px;
top margin is 100px
right and left margins are 150px
bottom margin is 200px



MarginBottom


MarginLeft

MarginRight

MarginTop



MaxHeight sets the maximum height of an element 
MaxHeight = 20px;
If the content is larger than the maximum height, it will overflow 

MaxWidth sets the maximum width of an element 
MaxWidth = 20px;

MinHeight sets the minimum height of an element 
If the content is smaller than the minimum height, the minimum height will be applied 

MinHeight will always override MaxHeight 

MinWidth sets the minimum width of an element 

Opacity sets the opacity of the element. 
The default value is always 1 (100% opacity). Changing the value (0.5) will make the element less transparent.

Opacity will automatically apply to the parent's child elements unless specified.

Overflow sets what happens when the content inside an element is too large (i.e. the content overflows outside its block)
Hidden
Scroll
Auto 
Visible 



OverflowX sets what happens when the content overflows on the right or left

OverflowY sets what happens when the content overflows on the top or bottom



Padding sets the padding of an element 
Padding = 5px 10px 15px 20px;
top padding is 5px
right padding is 5px
bottom padding is 15 px
left padding is 20px

3 values (5px, 25px, 30px) = top, right and left, bottom
2 values (5px, 6px) = top and bottom, right and left


PaddingBottom

PaddingLeft

PaddingRight

PaddingTop


Painter 





PreferredHeight sets the preferred height


PreferredSize sets the preferred size

PreferredSize = 300px, 1cnt;
sets the height to 300px and the width to 1cnt(the size of the element's content)
PreferredSize = 1pca, 1.5em;
sets the height to 1pca(parent's size minus its padding and border) and the width to 1.5em(size of current font applied to element)

PreferredWidth sets the preferred width 



RadialLayoutEndAngle

RadialLayoutRadius

RadialLayoutStartAngle

RenderLayer

RenderLayerOffset




Scrollbar implements a scrollbar within an element. Accepts Painter or a custom class
Or <ScrollView>

ScrollbarColor sets the color of the scrollbar
ScrollbarColor = red;
ScrollbarColor = rgba (0,0,0,0);

ScrollbarSize sets the size of the scrollbar

ShadowIntensity sets the intensity of the shadow

ShadowOffsetX offsets the shadow on the x-axis

ShadowOffsetY offsets the shadow on the y-axis

ShadowSoftnessX
Soft shadow will wrap around the object, whereas a hard shadow will produce a shadow with a sharper edge.

wrap around objects

 Soft light is when a light source is large relative to the subject; hard light is when the light source is small relative to the subject.

ShadowSoftnessY

ShadowType adds an outline effect to an element (such as text or an image)



TextAlignment aligns text within an element 
Left, Center, Right

TextColor  sets the color of the text

TextFontAsset sets the font you specify
TextFontAsset = ()

TextFontSize sets the size of the font
TextFontSize = 10px;


TextFontStyle

TextGlowColor

TextGlowInner

TextGlowOffset

TextGlowOuter

TextGlowPower

TextOutlineColor

TextOutlineWidth

TextShadowColor adds the color of the text's shadow

TextShadowIntensity adjusts the intensity of the text's shadow

TextShadowOffsetX

TextShadowOffsetY

TextShadowSoftness

TextShadowType

TextTransform

TextWhitespaceMode

TransformBehavior

TransformBehaviorX

TransformBehaviorY

TransformPivot

TransformPivotX = 1,1;

TransformPivotY

TransformPosition

TransformPositionX

TransformPositionY

TransformRotation

TransformScale = 

TransformScaleX

TransformScaleY


Visibility shows or hides an element
Visibility = visible;
Visibility = hidden;

ZIndex sets the stack order of an element
An element with a larger z-index is placed infront of an element with a lower z-index