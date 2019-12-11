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
style child         { PreferredSize = 1bsz; }
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

## LayoutFit


## Views


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

## Overflow

Overflow happens when an element’s actual size is bigger than its LayoutBox (allocated size)
or when its position is out that boxes’ bounds because of alignment for example.