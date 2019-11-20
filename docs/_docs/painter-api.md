---
id: Painters
title: Painters
layout: page
---

UIForia's Painter can be used to draw graphics. If you're familiar with HTML canvas, you will notice many similarities.

You must set an element in the XML to begin drawing within an assigned area.

## Features 
* Procedural Shapes	  
* No extra sprites for cutouts, rounding corners, stars, circles, ellipses, etc.  
* Multiple fill options  
* Hierarchical opacity settings  
* SDF Text rendering  
* infinitely nested clipping  
* Full vector graphics package  
* Easy to define effects  
* Particles   
* Auto Sprite batching  

<br/>

---------------------------------------------------------------
  
## Getting Started 
  
    
** Required namespaces in your C# script:**
```
using Vertigo;
using SVGX;
```
  

## Structure
  
#### 1. Create a `<Div>` element in the XML:
`<Div style = "Box"/>`

#### 2. Use the `Painter` style property and name it:

```
style Box {
    Painter = "MyPainter";
}
```

#### 3. In your C# script, declare the `Paint` method and assign your desired properties:  
```
[CustomPainter("MyPainter")]
public class MyPainter : ISVGXElementPainter {
    public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
        ctx.SetTransform(matrix);
    }
}
```
The `[CustomPainter("MyPainter")]` attribute links the name of your Painter in the `.style` file

The `Paint` method is where you define the properties of your graphical drawing.
  
`SetTransform` overrides the current transform matrix. The default is set to the screen.

<br/>

---------------------------------------------------------------      

## Drawing elements
  
There are a multitude of options we can choose for drawing: 
* lines  
* shapes (rectangles, squares, ellipses)  
* paths  
* images  
* text  

To draw a circle within your element, use the following methods:

```
[CustomPainter("MyPainter")]
public class MyPainter : ISVGXElementPainter {
    public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
        ctx.SetTransform(matrix);
        ctx.SetFill(Color.black);
        ctx.FillCircle(0, 0, 20);
        ctx.ArcTo(20, 20, 20, 0, 360);
        ctx.SetStroke(Color.white);
        ctx.SetStrokeWidth(5);
        ctx.Stroke();
    }
}
```

`SetTransform` overrides the current transform matrix.
    
`SetFill` sets the color of the circle.   
  
`FillCircle` sets the x, y coordinates and the radius.  
  
`Stroke` draws the path you have defined above.


<br/>

---------------------------------------------------------------



### Positioning 
The top-left position denotes (0,0). Moving right increases the value of x and moving down increases the value of y. All elements are set relative to the origin (0,0).

![](http://www.andrewsmyk.com/mobiledev/wp-content/uploads/2011/09/canvas1.jpg)

<br/>

---------------------------------------------------------------


### Stroke and Fill
Your drawing will not be visible until you call `Stroke()` 
  
Stroke sets the outline of the shape based on the stroke style.  

**For every additional stroke, call `BeginPath()`, otherwise the current path will be based on the previous path.** 

`Fill` fills subpaths by the `FillStyle`, which is set to yellow below: 

```
ctx.Rect(50, 50, 100, 200);
ctx.FillStyle(Color.yellow);
ctx.Fill();
```
<br/>

  
#### Using Stroke() and Fill 
If you use `Stroke()` and `Fill()`, the order in which you declare them will affect how they are rendered. In the example below, the first circle is drawn with the stroke on top of the fill, whereas the second circle is drawn with the fill on top of the stroke.

  
![strokeAndFill](assets/strokeAndFill.png)


  
```
ctx.SetTransform(matrix);
ctx.SetStroke(new Color32(0x15, 0x65, 0xc0, 0xff));
ctx.SetStrokeWidth(50);
                
ctx.BeginPath();
ctx.Circle(0, 50, 100);
ctx.Fill();
ctx.Stroke();
                
ctx.BeginPath();
ctx.Circle(300, 50, 100);
ctx.Stroke();
ctx.Fill();
```

<br/>

---------------------------------------------------------------

### Path
Setting the path is like sketching your drawing in pencil before adding paint.

Your path consists of points with instructions on how to connect those points to construct any type of shape.  

1. `MoveTo(x, y)` sets the starting coordinates
2. `LineTo(x, y)` draws a line to the ending coordinates

Step 1. and 2. are like sketching in pencil. However, nothing will appear within your element until you set `Stroke()` 

`BeginPath` and `Closepath` are necessary for starting the path and closing the current subpath. 


<br/>

---------------------------------------------------------------

  
### Lines
Drawing lines is simple with Painter. Simply define your path and then fill the path.

Call `BeginPath()` to reset the current path and then provide the starting point coordinates with the `MoveTo(x,y)` method.   
  
Then call `LineTo(x,y)` to draw the lines to the coordinate you set with `MoveTo()`.  
  
The line will be colored according to the `ctx.SetStroke()` property value and its width set by `ctx.SetStrokeWidth()`.

At the end, always call `Stroke` to draw the paths you have defined.  

![Lines](assets/Lines.png)


```
 public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
    ctx.SetTransform(matrix);
    ctx.BeginPath();
    ctx.SetStrokeWidth(4);
    ctx.SetStroke(Color.blue);
    ctx.MoveTo(0,75);
    ctx.LineTo(250,75);
    ctx.Stroke();
    ctx.Text();
                
    ctx.BeginPath();
    ctx.SetStroke(Color.yellow);
    ctx.MoveTo(50,0);
    ctx.LineTo(150, 130);
    ctx.Stroke();
}
```

<br/>

---------------------------------------------------------------

### Text
To draw text within your element, use `ctx.Text()`

        public float fontSize;
        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;
        public Color32 outlineColor;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public TMP_FontAsset fontAsset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        public float faceDilate;
  
`ctx.Text(contentRect.x - textScroll.x, contentRect.y, textInfo);`
  
<br/>

---------------------------------------------------------------  


### Styles and Colors
In order to apply colors to a shape, there are a few methods you can use: `SetFill()` and `SetStroke()`

**`SetFill(color)`**   
Sets the style when filling shapes. You can use a color, texture, gradient, or tint as property values.

The valid property values you can enter are `SetFill(Color.blue)` or RGB and RGBA values `SetFill(new Color(29, 100, 206)`

  
**`SetStroke(color)`**  
Sets the stroke color, gradient, or texture.

  
*The default stroke and fill colors are set to black.*

  
In this example, we draw three rectangles using three different colors. 
  
![rectangles](assets/rectangles.png)

```html ca
 public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
    ctx.SetTransform(matrix);

    ctx.SetFill(Color.red);
    ctx.FillRect(10, 10, 300, 150);

    ctx.SetFill(Color.yellow);
    ctx.FillRect(50, 50, 300, 150);

    ctx.SetFill(Color.blue);
    ctx.FillRect(100, 100, 300, 150);

    ctx.Stroke();
}
```
     
    
The colors are defined by CSS color values:  

## Colors
The colors are defined by CSS color values.
  
[Full list of colors supported by UIForia](Colors)
  

**Color Names**  
You can declare the name of the color. For example, `Color = blue` or `Color = red`


**RGB**  
UIForia supports RGB color values. The parameters (red, green, blue) specify the intensity of color as an integer between 0 and 255.

**RGBA**  
RGBA extends upon RGB by adding an alpha channel, which sets the opacity.
RGBA(red, green, blue, alpha)

**Hex Code**  
The CSS 6-digit hex color notation specifies RGB colors using hexadecimal values. 



<br/>

---------------------------------------------------------------


### Gradients
Gradients allow you to specify a range of colors to transition along a vector. UIForia supports two kinds of gradient: linear and radial.

#### Linear Gradient
A linear gradient specifies the color change along a straight line, in the direction you have specified as the `SVGXLinearGradient` parameter  (**Vertical** or **Horizontal**). 

**`GradientDirection.Horizontal`**  
**`GradientDirection.Vertical`**

After setting the direction, you must define a minimum of two color stops.

`ColorStop` takes in two parameters: `time` and `color`

**`time`**
A float between `0.0` and `1.0`, indicating the start and end of the gradient.

**`color`**
CSS color indicates the selected color of the stop.

```
public SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Horizontal, new [] {
    new ColorStop(0, Color.red), 
    new ColorStop(0.5f, Color.blue), 
    new ColorStop(1f, Color.white),
});
```

#### Radial Gradient 


<br/>

---------------------------------------------------------------


### Transformations



<br/>

---------------------------------------------------------------

### Shadows



<br/>

---------------------------------------------------------------

SetFillOpacity
SetStrokeOpacity




  
## Methods
 
### Paths
* `Fill` fills the current drawing path
* `SetFill` sets the color, texture, gradient, tint, or stroke. `SetFill()`
* `ArcTo` sets an arc between two tangents, `ArcTo(cx, cy, radius, startAngle, endAngle)`





```
ctx.ArcTo(50,50, 50, 0, 180);
```
   
#### `FillRect()` : float x, float y, float width, float height 
  
Draws a filled rectangle.
            
#### `FillCircle()` : float x, float y, float radius

Draws a filled circle
      
#### `FillEllipse()`: float x, float y, float dx, float dy

Draws a filled ellipse.
 
#### `CircleFromCenter()` : float cx, float cy, float radius

`CircleFromCenter(float cx, float cy, float radius) || CircleFromCenter(Vector2 position, float radius) `    

Draw a circle with the x-coordinate as the center

#### `ClosePath()`   
  
Draws a path from the current point to the starting point.

#### `CubicCurveTo()` : Vector2 ctrl0, Vector2 ctrl1, Vector2 end
  
Draws a cubic curve between vectors.
  
#### `QuadraticCurveTo()` : Vector2 ctrl, Vector2 end
  
Sets a quadratic curve between vectors.    
    
#### `RoundedRect()` : Rect rect, float rtl, float rtr, float rbl, float rbr  
  
Draws a rectangle with rounded edges.
  
#### `LineTo()` : float x, float y  
  
Draws a line by the specified x,y coordinates. 
     
#### `HorizontalLineTo()` : float x  
Draws a horizontal line by the specified x coordinate.  
    
#### `SetStroke()` : color || gradient || texture  
  
Sets the color, gradient, or texture to fill of your stroke.
`SetStroke(red)`
  
#### `Text()` : float x, float y, TextInfo text

Draws text on the canvas at x, y coordinates. 
  
#### `MoveTo()` 
Moves the path to specified coordinates 

#### `VerticalLineTo()` : float y
Draws a vertical line to the Y coordinate. 
  
#### `CubicCurveTo()` : Vector2 ctrl0, Vector2 ctrl1, Vector2 end
  
Draws a cubic curve based on the coordinates 
  
#### `QuadraticCurveTo()` : Vector2 ctrl, Vector2 end

Draws a quadratic curve based on the coordinates.
  
#### `UpdateShape()` : int pointStart, int pointCount

Updates...

#### `Clear()` 

Clears drawing.
  
#### `Save()` 
  
Saves the state 
  
#### `Restore()` 
Returns the previously saved path state
  
#### `PushClip()` 
  
Clips an assigned area from the original element  

#### `PopClip()`
   
Pops the last pushed element

#### `EnableScissorRect()` 

Limits drawing to rectangle. 

#### `DisableScissorRect()` 


#### `BeginPath()` 

Begins the path

#### `Shadow()` 
Sets the shadow.

#### `Stroke()` 
Draws the path you defined.

#### `SetStrokeOpacity` : float 
  
Sets the opacity from 0.0 to 1.0

#### `SetStrokeWidth` : float

Sets the  width

#### `SetTransform` : SVGXMatrix trs

Overrides the current transform matrix

#### `SetFillOpacity` : float

Sets the fill opacity.
  
#### `SetStrokePlacement` : StrokePlacement strokePlacement 

Sets the stroke's placement.
  
#### `SetShadowColor` : color

Sets the shadow's color.   
 
#### `SetShadowOffsetX` : float 

Sets the shadow's x-axis offset.  
  
#### `SetShadowOffsetY` : float

Sets the shadow's y-axis offset.
    
#### `SetShadowSoftnessX` : float

Sets the shadow's softness.  
   
#### `SetShadowSoftnessY` : float

Sets the shadow's softness on the y-axis.  
  
#### `SetShadowIntensity` : float

Sets the shadow's intensity.  
  
#### `SetShadowTint` : color

Sets the shadow's tint.



