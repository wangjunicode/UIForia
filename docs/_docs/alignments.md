---
title: Alignment
description: How to use the Alignment properties
layout: page
showInMain: true
order: 25
tags:
 - uiforia
 - style
 - layout
---

# Alignments
Alignments change the position of elements after they have been laid out. You can adjust an element's
position by absolute `px`, relative `%` and more. Here's a list of all units:
{% include offsetmeasurements.md %}

Except for `px` and `%` all units expect the fractional notation. `0.5w` is 50% of the actual width for example.

Below you'll find all alignment style properties explained. Open the the [UIForia Documentation app](/docs/getting-started#check-out-the-demo-app)
to try out a live demo. You'll find it under the menu item `Alignment Demo`.

## AlignmentTarget
```
AlignmentTarget[X|Y] = *AlignmentTarget ...
```
 
{% include alignment-target.md %}

Just setting the target will move the element to the origin of the targeted box. X and Y targets may be different and
will be calculated independently. Setting the AlignmentTarget of an element and all its children to `Parent` will make
them overlap at the origin of the parent's box. Often you'll want to align siblings to their respective `LayoutBoxes`. 
Choose `Parent` or `ParentContentArea` if you're trying to align single elements for special purposes 
(stick that container of elements to one side, e.g.).

There are three style properties to change the AlignmentTarget:

`AlignmentTarget`, which is a shorthand that accepts either one or two values.

`AlignmentTarget = [AlignmentTargetXY]`, which sets the AlignmentTarget for both axes to the same value

or

`AlignmentTarget = [AlignmentTargetX] [AlignmentTargetY]`

And then there are the individual properties:

`AlignmentTargetX = [AlignmentTargetX]` and `AlignmentTargetY = [AlignmentTargetY]`

Note: you cannot set an `AlignmentTarget` in an animation!

## AlignmentOrigin and AlignmentOffset
```
AlignmentOrigin[X|Y] = *OffsetMeasurement ...
AlignmentOffset[X|Y] = *OffsetMeasurement ...
```

The AlignmentOrigin defines how far away from the targeted `AlignmentTarget` the element should start. When using any other 
`OffsetMeasurementUnit` than `%` you'll might confuse `AlignmentOrigin` and `AlignmentOffset` since they appear to do the 
same thing.

The difference really becomes visible once you use `%`. A `%` of `AlignmentOrigin` will resolve to that portion of the targeted
box, which can be the element's `LayoutBox` (which might be bigger than the actual size of the element, depending on the layout),
the element's `Parent` or `ParentContentArea` or even `View` or `Screen` size.

Setting an `AlignmentTarget = Parent` and an `AlignmentOrigin = 50%` will move the element to the middle of the of its parent.
The element is not yet centered though. Here's an example of how a (blue) child that has half the parent's size would be 
aligned in that case:

<div style="width: 100%; height: 60px; background: yellow; position: relative; padding: 10px 0;">
    <div style="position: absolute; left: 50%; width: 50%; height: 40px; background: blue;"></div>
</div>

If you'd want to center the blue element in the yellow one you have to move it back to the left by half its own size.
Setting `AlignmentOffset = -50%` would do the trick. A `%` value in `AlignmentOffset` always resolves to the relative size of
the element itself. Now the blue element is perfectly centered: 

<div style="width: 100%; height: 60px; background: yellow; position: relative; padding: 10px 0;">
    <div style="position: absolute; left: 25%; width: 50%; height: 40px; background: blue;"></div>
</div>

AlignmentOrigin and AlignmentOffset **can** be animated. Moving an element from the left edge of the parent to the right edge,
like a progress bar maybe, would be as simple as animating the `AlignmentOrigin` from `0%` to `100%` and the `AlignmentOffset`
from `0%` to `-100%`. Doing the same in css is a bit more tedious, so we won't include a live example here ;).

## AlignmentDirection
```
AlignmentDirection[X|Y] = [Start | End] ...
```
The `AlignmentDirection` is by default `Start`, which means setting that the element will be aligned at the start of the 
targeted box (top left). But you could also set `AlignmentDirection = End` if you'd rather offset your element from the 
bottom/right of your target. `AlignmentDirection` is, like the other properties above, a shorthand that sets both axes when
one value is provided or each axis individually of two values are present:

`AlignmentDirection = [AlignmentDirectionXY]`

`AlignmentDirection = [AlignmentDirectionX] [AlignmentDirectionY]`

There are only two supported values here: `Start` and `End`.

The shorthand can be expanded into two individual properties:

`AlignmentDirectionX = [AlignmentDirectionX]`

`AlignmentDirectionY = [AlignmentDirectionY]`

To align something in the bottom-left of the screen, maybe 20px from the end you could write:

```
AlignmentTarget = Screen;
AlignmentDirection = Start End;
AlignmentOrigin = 20px;
```


{% include alert.html type="info" title="Note: `AlignmentDirection` **cannot** be used in animations!" %}


## AlignmentBoundary
```
AlignmentBoundaryX = [Unset (default) | Screen | Parent | ParentContentArea | Clipper | View]
AlignmentBoundaryY = [Unset (default) | Screen | Parent | ParentContentArea | Clipper | View]
```
There is also a shorthand for this property that accepts one or two parameters:
`AlignmentBoundary = [AlignmentBoundaryXY]`

`AlignmentBoundary = [AlignmentBoundaryX] [AlignmentBoundaryY]`

The AlignmentBoundary can be used to clamp the location of the aligned element to the specified
bounds:

| value             | description                                                                                                                                                |
|:------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| Unset             | The default, no bounds clamping happens.                                                                                                                   |
| Parent            | The aligned element will not leave the parent's layout box.                                                                                                |
| ParentContentArea | This one will keep the element within the parent's [content box](/docs/layout/#box-model).                                                                 |
| View              | The aligned element will not leave the view.                                                                                                               |
| Screen            | The aligned element will not leave the screen.                                                                                                             |
| Clipper           | Refers to the next element in the hierarchy that clips this element. A parent that defines `Overflow = Hidden;` for example. Ultimately the view / screen. |

![](/assets/img/alignment_boundary.gif)

## AlignX AlignY
`AlignX = AlignmentOrigin [AlignmentOffset] [AlignmentTarget] [AlignmentDirection]`

Now that you know all about the individual alignment properties, there's also a neat shorthand that lets you do all of
the above in just one line (per axis though).
`AlignX` and `AlignY` can each be used with one to four arguments, with a handy twist. We'll explain all combinations 
for `AlignX` only to not repeat too much information since `AlignY` works exactly the same.

### `AlignX = 25%`
Only one argument will set the `AlignmentOrigin` to the provided value (20% in the example)
and if value's unit is `%` it will **also** set the `AlignmentOffset` to the negative of the provided value,
which would be `-25%` in this case. Further up we explained how to perfectly center an element or move it from
left to right within the targeted element. With this shorthand you can animate `AlignX` from `0%` to `100%`
and your element would move! 
Setting `AlignX = 20px` would, as expected, only set the `AlignmentOriginX` and **not** the `AlignmentOffsetX`!

### `AlignX = 100% 0%`
This combination will set `AlignmentOriginX = 100%` and `AlignmentOffsetX = 0%`.

The element would be moved to the end of its targeted box but would would stick on its outside rather than
being aligned to the right side. If we change the example to `AlignX = 100% -0.5w` the element's center would 
be on the right edge of the targeted box. `-0.5w` is equivalent to `-50%` when used as a `AlignmentOffset` btw.

### `AlignX = 20px 0px ParentContentArea`
Here we define `AlignmentOriginX`, `AlignmentOffsetX` and the `AlignmentTargetX`.
 
There's a small gotcha here; we have to write `0px` **not omitting** the
unit since the style parser would interpret the token after the `0` as its unit. `0ParentContentArea` is not
a thing though!

### `AlignX = 0% 0% Parent End`
You can push your element to the end of its parent, aligning both their right edges like this.

Remember that `AlignmentTarget` and `AlignmentDirection` cannot be animated, which means that the `AlignX` 
shorthand can only be animated when used with one or two arguments!

### One more thing
We have one more shorthand for you. All `AlignmentOrigin` and `AlignmentOffset` property values have a shorthand
for the `0%`, `50%` and `100%` values, which are `Start`, `Center`, `End`.

Setting `AlignX = Center` is equivalent with `AlignX = 50%`, which is equivalent with `AlignX = 50% -50%` if you
forgot. Awesome, right? But you can use it in a confusing, yet correct way, too: `AlignX = Start Start Parent End`.
If you read the rest of this document you might know what it means :)
