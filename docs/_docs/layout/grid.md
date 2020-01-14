---
id: GridLayoutGuide
title: Grid Layout Guide
tags: 
 - layout
layout: page
---
# The Grid LayoutType
With the `Grid` `LayoutType` you can configure a two-dimensional layout structure that consists
of rows and columns. It supports a variety of sizing types, from pixels to fractional remaining space,
has shorthands commands for growing and shrinking tracks, and methods to repeat track configuration 
patterns.

Use grids only if you actually need them, i.e. grids with only one column or one row may be 
more represented more efficiently with a [flex layout](/docs/layout/flex). 

## Auto Size
`auto` in a grid will always default to content size. Use `LayoutFit` on your grid items to fit into the cell space.

## Block providers and Grid
In a grid not only elements can be block providers but also spanned cells. 

```
style grid1 {
    GridLayoutColTemplate = 100px grow(100px, 1mx) 100px;
}
style grid-auto-item {
    PreferredWidth = 300px;
    MinWidth = auto;
}
```

`grink(base, fr grow, fr shrink, shrink-limit, grow-limit)`
`grow(base, max)` -> `clamp(base, fr grow, fr shrink, base, grow-limit)`
`grow(base, fr grow, 400px)` -> `clamp(base, 3fr, 0, base, 400px)`
`shrink(base, shrink-limit)` -> `clamp(base, fr grow, fr shrink, shrink-limit, base)`
`shrink(base, fr shrink, shrink-limit)` -> `clamp(base, fr grow, fr shrink, shrink-limit, base)`
 
 1. The base size is allocated
 2. If there is overflow the elements tries to shrink down to the `shrink-limit`.
 3. If there is underflow the elements tries to grow to the `grow-limit`.
 4. If there is still space left in the whole grid container we allocate the `fr` space.
 
 That means in case the grid is content sized there will never be extra space to grow or shrink
 and `fr` will always be 0. This 
 
 `GridLayoutColTemplate = 100px 1fr 100px;`
 `GridLayoutColTemplate = 100px grink(0, 1fr, 1fr, 0, infinite) 100px;`
 `GridLayoutColTemplate = grink(100px, 0, 0, 100px, 100px)`

Elements may become block providers based on their `PreferredSize` not on `Min-` or `MaxSize`s.



---------------------------------------------------------------
## Grid Container
To use a grid box layout, set `LayoutType` to `Grid`

By default, grid elements are placed in rows and span the full width of the container.



   
```
style container {
    LayoutType = Grid;
}
```

---------------------------------------------------------------

## Adding Columns and Rows
  
#### GridLayoutColTemplate
Specifies the number and size of columns in a grid layout.
  
#### GridLayoutRowTemplate
Specifies the number and size of rows in a grid layout.
```
style container {
    LayoutType = Grid;
    GridLayoutColTemplate = 0.50pca 0.50pca;
    GridLayoutRowTemplate = 0.25pca 0.25pca;
}
```

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Column 1 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp; &nbsp;&nbsp;&nbsp;&nbsp;          Column 2
![grid-layout](/assets/img/grid-layout.png)

We can examine the grid layout created above within the [UIForia Hierarchy](/docs/tools#uiforia-inspector). We have created 2 columns and 2 rows.

The columns and rows use `pca` for measurement, which stands for Parent Content Area: the measure of the parents size minus its padding and border.

**Column:** 0.50pca assigned to `GridLayoutColTemplate` will extend to half the parent size minus its padding and border. 
  
**Row:** 0.25pca assigned to `GridLayoutRowTemplate` will extend to a quarter of the parent size minus its padding and border. 
    
Additional measurement types can be found in the [Style Units](/docs/style/style-units)
    


---------------------------------------------------------------

## Styling Elements
Now that we have created the container, we can create elements in our XML and assign style properties. 

In addition to our container element, we will create and style three separate `<Div>` elements called **grid-one**, **grid-two**, **grid-three**


```
style container {
    LayoutType = Grid;
    GridLayoutColTemplate = 0.50pca 0.50pca;
    GridLayoutRowTemplate = 0.25pca 0.25pca;
}

style grid-one {
   BackgroundColor = Red;
}

style grid-two {
    BackgroundColor = Yellow;
}

style grid-three {
    BackgroundColor = Purple;
}
```

![grid-colandrow](/assets/img/grid-styling.png)  

<br/>

---------------------------------------------------------------


## Gap ##
`GridLayoutColGap` and `GridLayoutRowGap` properties create spaces between columns and rows.

Gaps are created in between rows and columns, rather than the edge of the container.
```
style container {
    LayoutType = Grid;
    GridLayoutColTemplate = 0.50pca 0.50pca;
    GridLayoutRowTemplate = 0.25pca 0.25pca;
 +  GridLayoutColGap = 8px;
 +  GridLayoutRowGap = 8px;
}

```
![grid-gap](/assets/img/grid-gap.png)  

<br/>

---------------------------------------------------------------
  
## Nested Grid Items
We'll split the first column into two separate elements and the second column will be a nested grid with two columns and six rows:
  
```

style grid-three {
    LayoutType = Grid;
    GridLayoutColTemplate = 1fr 1fr;
    GridLayoutRowTemplate = 1fr 1fr 1fr 1fr 1fr 1fr;
}
```
![grid-colandrow](/assets/img/grid-colandrow0.png)  
*(As mentioned above, we are hoovering over the element to view the nested grid items in column 1, row 1. Check out the [UIForia Hierarchy](/docs/tools#uiforia-inspector) for more details.)*


<br/>

---------------------------------------------------------------

## Placement of Elements
Elements will automatically be placed in the nearest available location. In this instance, `grid-two` is placed in the second column and the first row. 
   

However, if we want to swap the location of `grid-two` and `grid-three`, we can set `GridItemRowStart = 1` within **style grid-two**
  
![grid-colandrow](/assets/img/grid-colandrow1.png)  

We have access to both `GridItemColStart` and `GridItemRowStart`, which define the line where the row or column begin.

<br/>

---------------------------------------------------------------  
## Span 
You can use `GridItemColSpan` or `GridItemRowSpan` to scale items across more than one column or row. 

#### `GridItemColSpan` 
Element can span across multiple columns. 
      
#### `GridItemRowSpan` 
Set an element to span across multiple rows.

<br/><br/>

We will span **grid_three** across two rows below:

```
style grid_three {
    LayoutType = Grid;
    GridLayoutColTemplate = 1fr 1fr;
    GridLayoutRowTemplate = 1fr 1fr 1fr 1fr 1fr 1fr;
 +  GridItemRowSpan = 2; 
}
```

![grid-span](/assets/img/grid-span.png)  

<br/>

---------------------------------------------------------------  

## Adding Elements in a Nested Grid
Since **grid_three** now has 2 columns and 6 rows, we can add `<Div>` elements to style each item in our nested grid.

Add `<Div style = "cell-1">`, `<Div style = "cell-2">`, and `<Div style = "cell-3">` to the XML.

Next, we'll style each **cell** with `BackgroundColor` and span columns and rows:
  
```
style cell-1 {
    GridItemColSpan = 2;
    GridItemRowSpan = 2;
    BackgroundColor = Blue;
}

style cell-2 {
    GridItemColSpan = 1;
    GridItemRowSpan = 2;
    BackgroundColor = orange;
}

style cell-3 {
    GridItemColSpan = 1;
    GridItemRowSpan = 1;
    BackgroundColor = teal;
}
```
![grid-nested](/assets/img/grid-nested.png)  

We'll clean it up now by adding gaps:
```
style grid_three {
    LayoutType = Grid;
    GridLayoutColTemplate = 1fr 1fr;
    GridLayoutRowTemplate = 1fr 1fr 1fr 1fr 1fr 1fr;
 +  GridLayoutColGap = 5;
 +  GridLayoutRowGap = 5;
    GridItemRowSpan = 2; 
}
```

![grid-gaps](/assets/img/grid-gap2.png) 

<br/>

---------------------------------------------------------------  

## Text 
Within our elements, we can add text. We can achieve this by creating another `<Div>` element and styling it.

For example, we can add text in the top-left corner:

`<Div style="title">Title</Div>`

```
style title {
    Padding = 5px;
    TextFontAsset = url("Fonts/Burbank");
    TextFontSize = 1.1em;
    TextTransform = UpperCase;
}
```

![grid-gaps](/assets/img/grid-text.png) 

`TextFontAsset` sets the text font to Burbank, located in the **Fonts** folder inside **Resources**
  
`TextFontSize` sets the font size to 1.1em. Em is the size of the current font applied to the element.
   
`TextTransform` modifies the capitalization of text.

The full list of style properties for text can be found [here](StyleProperties.md)

<br/>

---------------------------------------------------------------  
## Grow
How about setting text in the bottom-left corner?


We could use `FlexItemGrow`, which defines the ratio of extra space an element will get.

Assign `FlexItemGrow = 1` to the `title` element, which will grow its size until it hits the element we create called `<Div style="description">`
 

![grid-grow](/assets/img/grid-grow.png) 

