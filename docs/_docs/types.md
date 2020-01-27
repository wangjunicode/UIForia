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

## AlignmentTarget

| value             | description                                                                            |
|:------------------|:---------------------------------------------------------------------------------------|
| LayoutBox         | the default; target the layout box that has been assigned to the element by its parent |
| Parent            | Target the parent's layout box.                                                        |
| ParentContentArea | Target the parent's [content box](/docs/layout/#box-model).                            |
| View              | Target the view. Does not affect parent transforms!                                    |
| Screen            | Target the screen. Does not affect parent transforms!                                  |

## AlignmentDirection

| value | description                                                                                    |
|:------|:-----------------------------------------------------------------------------------------------|
| Start | the default; starts offset / origin at the start of the axis.                                  |
| End   | Start offset / origin alignment from the end of the axis; from bottom to top or right to left. |

## Cursor
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