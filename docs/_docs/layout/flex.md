---
id: FlexBoxLayoutGuide
title: Flexbox Layout Guide
tags:
 - layout
layout: page
---

# The Flex LayoutType
`Flex` is the default `LayoutType`. It lays out elements on an axis in one direction, which you can define to be
either from left to right on the `Horizontal` or top to bottom on the `Vertical` axis.

```
style flex-horizontal {
    FlexLayoutDirection = Horizontal;
}

style flex-vertical {
    FlexLayoutDirection = Vertical;
}
```

There's no need to specify the `LayoutType = Flex` as it is the default anyway.

### `FlexLayoutDirection: LayoutDirection`
 `FlexLayoutDirection` sets the direction in which your elements are laid out. There are two possible values:
 
* `Vertical`, which is also the default `FlexLayoutDirection`, places the elements children in a vertical track. 
* `Horizontal` will place every child of your element in one horizontal track. Horizontal flex elements may also wrap.

Children of a flex layout element, we call them items, are placed inside a virtual track in the direction you chose.
In order to fill up all the space that is available to the track you can use the `FlexItemGrow` and `FlexItemShrink` 
properties.

The natural layout flow is top to bottom and left to right. If you want to change that use the [alignment style properties](/docs/alignments).

### `FlexItemGrow: int`
If the flex element provides more space than is used by its items you might want to distribute that extra
space among one or more of them. The grow value defines the ratio of the extra space the item will consume.  

```
style flex-grow-1 {
    FlexItemGrow = 1;
}

style flex-grow-2 {
    FlexItemGrow = 2;
}

style flex-item {
    PreferredSize = 50px 1cnt;
    MinWidth = 1cnt;
    Padding = 0.5em;
    BackgroundColor = red;
    Margin = 0.25em;
    TextAlignment = Center;
}
```

![flex-align](/assets/img/flex-grow.png)

In the example above the item with the style group `flex-grow-2` takes up twice the amount of extra
space than the item with the style group `flex-grow-1`.

### `FlexItemShrink: int`   
If the element's track size becomes bigger than the element's preferred size the shrink property may be
applied to one or more items to reduce their size based on the same proportion method as described for growing them.

Combining both, `FlexItemGrow` and `FlexItemShrink`, will be useful in many place where you want to take up all the 
available space but not more than that.


### `FlexLayoutWrap: LayoutWrap`
Wrapping can only be used if the `FlexLayoutDirection = Horizontal`. Possible values are `None` (default) and `WrapHorizontal`.
Vertical layouts cannot be wrapped as it would be too computationally expensive to figure out the correct dimensions of the tracks.


