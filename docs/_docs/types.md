---
id: types
title: Types
layout: page
tags:
  - uiforia
  - api
---

# UIForia Types


## Overflow

| value   | description                                                                                                       |
|:--------|:------------------------------------------------------------------------------------------------------------------|
| Visible | default; all children that overflow the bounds of the element will be visible until they are clipped by the view. |
| Hidden  | Children will be clipped at the [ClipBounds](/docs/types#clipbounds)                                              |
| Scroll  | Only used by the [ScrollView Element](/docs/elements#scrollview)                                                  |

## ClipBehavior

| value  | description                                                                                            |
|:-------|:-------------------------------------------------------------------------------------------------------|
| Never  | Opt-out of regular clipping behavior. Elements with this property set will                             |
| Normal | Elements will be clipped by the view / screen by default or a parent that sets a custom Overflow value |
| View   |                                                                                                        |
| Screen |                                                                                                        |

## ClipBounds
See a description of our [box model](/docs/layout/#box-model) for more information.

| value      | description                                                                                           |
|:-----------|:------------------------------------------------------------------------------------------------------|
| BorderBox  | default. If an element is defining `Overflow = Hidden` the clip bounds will be the border box.        |
| ContentBox | Clips the children at its content box, which means that padding and border visually affects clipping. |

## PointerEvents

| value  | description                                                                                                              |
|:-------|:-------------------------------------------------------------------------------------------------------------------------|
| Normal | the default; mouse / touch input will be registered                                                                      |
| None   | Opt out of input detection; will fire no hover, click or any other mouse / touch event. Applies to all children as well. |

## LayoutFit

| value          | description                                                         |
|:---------------|:--------------------------------------------------------------------|
| Default / None | The element keeps its original size                                 |
| Grow           | The element will try to grow into the allocated space, if smaller.  |
| Shrink         | The element will try to shrink into the allocated space, if bigger. |
| Fill           | The element will try to grow or shrink into the allocated space.    |
| FillParent     | The element will try to grow or shrink into the parent's space.     |

## LayoutType

| value  | description                                                        |
|:-------|:-------------------------------------------------------------------|
| Flex   | Default if not specified. See the [flex guide](/docs/layout/flex). |
| Grid   | See the [flex guide](/docs/layout/grid).                           |
| Radial | <span class="badge badge-danger">experimental</span>               |
| Stack  | See the [stack guide](/docs/layout/stack).                                                                     |

## LayoutBehavior

| value              | description                                                                                                |
|:-------------------|:-----------------------------------------------------------------------------------------------------------|
| Normal             | Layout will happen as expected from the `LayoutType`.                                                      |
| Ignored            | The element will not take part in its parent's layout. It will not be one of the flex items or grid cells. |
| TranscludeChildren | The element will act as a proxy and all its children will be laid out by its parent.                       |

## AlignmentTarget
{% include alignment-target.md %}

## AlignmentDirection

| value | description                                                                                    |
|:------|:-----------------------------------------------------------------------------------------------|
| Start | the default; starts offset / origin at the start of the axis.                                  |
| End   | Start offset / origin alignment from the end of the axis; from bottom to top or right to left. |

## CursorStyle
In style sheets the syntax is: 
`Cursor` `=` [url](#url) `[ hotSpotX ]` `[ hotSpotY ]`;  
`C#`: UIForia.Rendering.CursorStyle

## URL
Written as `Property = url("path/to/your/asset");`.
Depending on the property the `C#` API may expect different types:
### `UnityEngine.Texture2D` 
- `BackgroundImage`
- `Cursor`

### `UIForia.FontAsset`
- `TextFontAsset`

### `UIForia.FontStyle`
- `TextFontStyle`

## Primitives
### `int`
Only the basic integer notation is supported, which means no underscore delimiter in 
int property values like `1_000`. 

### `float`
Same as with int there's no underscore delimiter. The trailing `f` is not required.

### `string`
They have to be on a single line and be surrounded by quotation marks `"string"`

## Units
### UIMeasurement 
Used for things that need to measured and can relate to other element sizes.
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `ParentContentArea` in style sheets: `pca` The measure of the parents size minus its padding and border
 * `ParentSize` in style sheets: `psz` The total measure of the parent's size
 * `Em` in style sheets: `em` the em size of the current font applied to the element
 * `Content` in style sheets: `cnt` the size of the element's content
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 
### UIFixedLength
Used for tings that have lengths relative to an element.
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `Percent` in style sheets: `%` the percentage of an element's size on the related axis
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `Em` in style sheets: `em` the em size of the current font applied to the element

### GridTrackSize

| Style unit | GridTemplateUnit    | Description                                                             |
|-----------:|:--------------------|:------------------------------------------------------------------------|
|         px | Pixel               | just plain pixels                                                       |
|         em | Em                  | factor relative to the current font-size                                |
|         mx | MaxContent          | The grid track will take the natural content size of its biggest cell.  |
|         mn | MinContent          | The grid track will take the natural content size of its smallest cell. |
|         fr | FractionalRemaining | The fractional portion of the unallocated space in a row or column.     |
|        pca | ParentContentArea   | The measure of the parents size minus its padding and border           |
|        psz | ParentSize          | The total measure of the parent's size                                  |
|         vw | ViewportWidth       | the width of the view the element is in                                 |
|         vh | ViewportHeight      | the height of the view the element is in                                |
|          % | Percent             | Cells will get allocated x% of the grids size.                          |


### OffsetMeasurement
{% include offsetmeasurements.md %} 