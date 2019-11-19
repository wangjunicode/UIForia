---
title: Style Units
description: px, pca, %, vh, vw, all style units explained
layout: page
---

# Units
### UIMeasurement 
Used for things that need to measured and can relate to other element sizes.
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `ParentContentArea` in style sheets: `pca` The measure of the parents size minus it's padding and border
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

### GridTemplate
Used for defining grid layouts.
 * `Pixel`  in style sheets: `px` -- one device pixel
 * `MaxContent` in style sheets: `mx` the max of the sizes of all elements in a given row or column
 * `MinContent` in style sheets: `mn` the min of the sizes of all elements in a given row or column
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `Em` in style sheets: `em` the em size of the current font applied to the element
 * `FractionalRemaining` in style sheets: `fr` a fractional portion of the unallocated space in a row or column
 * `ParentContentArea` in style sheets: `pca` The measure of the parents size minus it's padding and border
 * `ParentSize` in style sheets: `psz` The total measure of the parent's size

### Transform
Units for defining how an element's position can be transformed.
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
 * `AnchorHeight` in stylesheets: `ah` the absolute distance between the element's `AnchorTop` and `AnchorBottom`
 * `ViewWidth` in style sheets: `vw` the width of the root element in the element's hierarchy
 * `ViewHeight` in style sheets: `vh` the height of the root element in the element's hierarchy
 * `ParentWidth` in style sheets: `pw` the width of the parent including border and padding on x axis
 * `ParentHeight` in style sheets: `ph` the height of the parent including border and padding on y axis
 * `ParentContentAreaWidth` in style sheets: `pcaw` the width of the parent excluding border and padding on x axis 
 * `ParentContentAreaHeight` in style sheets: `pcah` the height of the pare bvnt excluding border and padding on y axis 
 * `ScreenWidth` in style sheets: `sw` the width of the screen
 * `ScreenHeight` in style sheets: `sh` the height of the screen  