---
id: LayoutConcepts
showInMain: true
title: Layout Concepts and common terms explained
tags: 
 - layout
layout: page
order: 10
---

# Layout Concepts and common terms explained

## Block Sizes
When using one of the block sizes (`pca`, `psz`) the size of the element is dependent on the next 
_block size provider_ up in the hierarchy.

```
style container     { PreferredSize = 500px; }
style content-sized { PreferredSize = 1cnt; }
style child         { PreferredSize = 1psz; }
```

```xml
<Div x-id="div1" style="container">
    <Div x-id="div2" style="content-sized">
        <Div x-id="div3" style="child" />
    </Div>
</Div>
```

In this example the child will be 500px wide and high, just like the container div, although there's 
a content-sized container in between. 

## Box Model

  ![BoxModel](/assets/img/boxmodel.png)
  
Unlike CSS there is only one box model in UIForia, which is the equivalent to the CSS border-box model.
The actual size of an element is the sum of the border and padding values and the content box. 
Consider an element with the following spec:

```
PreferredSize = 100px;
Padding = 8px;
Border = 2px;
```

The actual size of the element will be 100px by 100px and the content box will be 90px by 90px.

## LayoutBox and Allocated Size

When an element gets laid out and its content size is calculated it passes this size (or parts of it) 
down to its children. The details vary between `LayoutType`s of course but the principle is the same
across all of them.

Let’s say there is an element that takes up the whole screen and has one child with a fixed
`PreferredSize`. The parent would allocate the available space of its content box for the
child.

When styling the child you might want to center it in the `LayoutBox` the parent allocated:
```
style child {
    PreferredSize = 100px;
    AlignX = Center LayoutBox;
    AlignY = Center LayoutBox;
}
``` 

Or you might want to grow the child into the `LayoutBox`. The `PreferredSize` is really
just a size the child _should_ have.
```
style child {
    PreferredSize = 100px;
    LayoutFit = Grow;
}
```

## ContentBox and Actual Size

The actual size of an element is the `PreferredSize` after growing or shrinking including borders 
and paddings. The content box is what the element’s LayoutType would use to determine the 
children’s LayoutBoxes, among other things of course.

## LayoutFit
A special property that can be applied to elements in any `LayoutType`. It's a shorthand for
`LayoutFitHorizontal` and `LayoutFitVertical`. Possible values are:
- `Grow`, which grows an element until it is as big as its allocated size (see above)
- `Shrink`, which shrinks an element to its allocated size.
- `Fill`, does both of the above, which is useful if the element might be bigger or smaller than the allocated size.

Use UIForia's HierarchyView to highlight an element's allocated space. Sometimes it's hard to know if there
is any space to grow into. Here's an example of an element that does not take up all the available space:

```
style container {
    PreferredSize = 200px;
    Padding = 10px;
}
style item {
    PreferredSize = 80px;
    BackgroundColor = rgba(0, 120, 0, 200);
    Margin = 10px;
}
```

```
<Group style="container">
    <Group style="item" />
</Group>
```

![](/assets/img/layoutFit-1.png) ![](/assets/img/layoutFit-2.png)

To the right you see the item being selected in the hierarchy view.

The yellow color shows the remaining allocated space. Setting `LayoutFit = Fill` (which sets both `LayoutFitHorizontal`
and `LayoutFitVertical`) will grow the item into that space but not into the rest of the white vertical area that
is technically available but not allocated. To grow into the un-allocated space you'd have to use the container's 
LayoutType's specific properties. In this case we're dealing with a [flex layout](/docs/layout/flex). Follow the link 
and read up on `FlexItemGrow`. 

Adding the newly found knowledge to the example we can make our item occupy all the space of its parent:

```
style fill-and-grow {
    FlexItemGrow = 1;
    LayoutFitHorizontal = Grow;
}
```

```
<Group style="container">
    <Group style="item fill-and-grow" />
</Group>
```

![](/assets/img/layoutFit-3.png) ![](/assets/img/layoutFit-2.1.png) ![](/assets/img/layoutFit-4.png)

Here the result in three steps: 1: only `FlexItemGrow`, 2: same, but selected in the hierarchy to visualize the allocated
space and 3: end result, stretched on both axis.

To reiterate, `LayoutFitVertical = Fill` wouldn't have had any effect because there was no further space allocated by its parent.

## Overflow

Overflow happens when an element’s actual size is bigger than its LayoutBox (allocated size)
or when its position is out that boxes’ bounds because of alignment for example.