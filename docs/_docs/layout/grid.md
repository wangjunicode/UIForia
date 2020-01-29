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

## Adding Columns and Rows
Let's try that and make a 2x2 grid with a fixed column and row size:
```xml   
<Group style="simple-grid">
    <Div style="simple-cell" />
    <Div style="simple-cell" />
    <Div style="simple-cell" />
    <Div style="simple-cell" />
</Group>
```

```
style simple-grid {
    PreferredSize = 200px;
    BackgroundColor = seagreen;
    LayoutType = Grid;
    GridLayoutColTemplate = 100px 100px;
    GridLayoutRowTemplate = 100px 100px;
}

style simple-cell {
    BackgroundColor = white;
    Border = 1px;
    BorderColor = black;
}
```
![](/assets/img/grid-1.png) ![](/assets/img/grid-1.1.png)

The left image shows the grid, the right shows the same but with the inspector's highlighting enabled.
What we see is that our simple cells don't have a size, since they are of course content sized by default.
The grid, however, allocates space for the cells according to the column and row template that we defined.

See also:
- [`GridLayoutColTemplate`](/docs/style/style-properties#gridlayoutcoltemplate) 
- [`GridLayoutRowTemplate`](/docs/style/style-properties#gridlayoutrowtemplate) 

Building on what is known about [LayoutFit](/docs/layout/#layoutfit) we could add the `LayoutFit` property
to the `simple-cell` style group and make the cells fill up all the allocated space:

```
style simple-cell {
    BackgroundColor = white;
    Border = 1px;
    BorderColor = black;
    LayoutFit = Fill; // or Grow would work too
}
```
![](/assets/img/grid-2.png)

Nice! But what if there are a lot of different cells with different style groups where you'd have to add
the same `LayoutFit` to all of them? To avoid this kind of repetition there is another style property:
`FitItemsHorizontal` and `FitItemsVertical`.

They work the same as `LayoutFit` but are defined on the grid container. The fit property is the applied to
all items of the grid. To get the same result as before you could therefore also write this:

```
style simple-grid {
    PreferredSize = 200px;
    BackgroundColor = seagreen;
    LayoutType = Grid;
    GridLayoutColTemplate = 100px 100px;
    GridLayoutRowTemplate = 100px 100px;
    FitItemsHorizontal = Fill;
    FitItemsVertical = Fill;
}

style simple-cell {
    BackgroundColor = white;
    Border = 1px;
    BorderColor = black;
}
```

### Column and Row AutoSize 
In the example above we added exactly four cells to our 2x2 grid. But what happens if you don't know the 
exact number of cells?

First, there's the `GridLayoutDirection` that you need to know about. The default is `Horizontal`, meaning
all cells will be laid out on the horizontal axis first until the row runs out of columns, then add the next
cell to the first column of the next row. Adding numbers to our simple grid example makes this a bit more obvious:

```xml
<Group style="simple-grid">
    <Div style="simple-cell">1</Div>
    <Div style="simple-cell">2</Div>
    <Div style="simple-cell">3</Div>
    <Div style="simple-cell">4</Div>
</Group>
```
![](/assets/img/grid-3.png)

Adding a fifth column will follow that algorithm and place the cell in the first column of the third row.
But we defined only two rows! Also, `simple-grid` has a `PreferredSize` of `200px`. The fifth column will 
therefore overflow. There is, however, a third row track. Column and row tracks that are automatically 
created by adding more cells than defined in the template can be configured using `GridLayoutColAutoSize`
and `GridLayoutRowAutoSize`. The default auto size is `1mx`, which is the content size of the element in
the cell.

```xml
<Group style="simple-grid">
    <Div style="simple-cell">1</Div>
    <Div style="simple-cell">2</Div>
    <Div style="simple-cell">3</Div>
    <Div style="simple-cell">4</Div>
    <Div style="simple-cell">5</Div>
</Group>
```

![](/assets/img/grid-4.2.png) ![](/assets/img/grid-4.3.png)

If we remove the `PreferredSize` property from the simple-grid style we'd no longer have overflow but a
naturally growing grid element:

![](/assets/img/grid-4.png) ![](/assets/img/grid-4.1.png)

Now let's add `GridLayoutRowAutoSize = 100px;` to the `simple-grid` style group. The automatically created row
will now have a height of `100px`.

![](/assets/img/grid-5.png) 

Just like `GridLayoutRowTemplate` the `GridLayoutRowAutoSize` property also accepts multiple values. Each
additional row will alternate between the values. 

```xml
<Group style="simple-grid">
    <Div style="simple-cell">1</Div>
    <Div style="simple-cell">2</Div>
    <Div style="simple-cell">3</Div>
    <Div style="simple-cell">4</Div>
    <Div style="simple-cell">5</Div>
    <Div style="simple-cell">6</Div>
    <Div style="simple-cell">7</Div>
    <Div style="simple-cell">8</Div>
    <Div style="simple-cell">9</Div>
</Group>
```
```
style simple-grid {
    GridLayoutRowAutoSize = 50px 100px;
}
```

The first additional row will be 50px, the next one 100px and the one after that will use the first value again - 50px.
This is how it looks like with nine cells:

![](/assets/img/grid-6.png) 

The same rules also apply if the `GridLayoutDirection` is `Vertical`. Instead of setting the `GridLayoutRowAutoSize` 
we now set `GridLayoutColAutoSize` since the additional cells create new columns instead of new row tracks.

```
style simple-grid {
    LayoutType = Grid;
    GridLayoutDirection = Vertical;
    GridLayoutColAutoSize = 50px 100px; 
}
```

![](/assets/img/grid-7.png) 

## Auto Size
{% include alert.html type="danger" title="Experimental Feature" %}
Using `auto` as a size unit in a grid will always default to content size.
Use `LayoutFit` on your grid items to fit into the cell space.

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

### Grid Cell Functions

`cell(base, fr grow, fr shrink, shrink-limit, grow-limit)`  
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
 `GridLayoutColTemplate = 100px cell(0, 1fr, 1fr, 0, infinite) 100px;`
 `GridLayoutColTemplate = cell(100px, 0, 0, 100px, 100px)`

Elements may become block providers based on their `PreferredSize` not on `Min-` or `MaxSize`s.
