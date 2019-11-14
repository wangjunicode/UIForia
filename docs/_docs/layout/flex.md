---
id: FlexBoxLayoutGuide
title: Flexbox Layout Guide
tags:
 - layout
layout: page
---


#  FlexBox
Flexbox is a one-dimensional layout mode, which was designed with a flexible and consistent layout for different screen sizes. 

Flexbox is described as one-dimensional since you can only utilize one dimension at a time — either a column or a row. If you wish to use a two-dimensional layout, a grid layout will provide the option of using both rows and columns.


---------------------------------------------------------------

## Flex Container
Although flexbox is set as the default layout, you would set `LayoutType` to `flex` in the parent element which holds the children in your XML.  
  
### `LayoutType = Flex`

## FlexLayoutDirection
 `FlexLayoutDirection`  sets the direction in which your elements are laid out. There are four possible values:

* `Row` aligns elements from left to right.
* `Column` aligns elements from top to bottom.
* `Horizontal` aligns elements horizontally. 
* `Vertical` aligns elements vertically. 

## Main Axis and Cross Axis
The layout will be structured according to the main axis and the cross axis. The main axis will be defined by `FlexLayoutDirection`, and the cross axis is set perpendicular to it. 


## `FlexMainAxisAlignment`
Defines how to align children within the main axis.   
  
Although flexbox is set as the default layout, you would set `LayoutType` to `flex` in the parent element which holds the children in your XML.

  
* `Start` aligns children at the start of the container's main axis.     
<br/><br/>             
* `End` aligns children at the end of the container's main axis.   
<br/><br/>       
* `Center` aligns children at the center of the container's main axis.  
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's main axis.  
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child across the container's main axis.  



 
```
style flex-start {
    FlexLayoutMainAxisAlignment = Start;
}

style flex-end {
    FlexLayoutMainAxisAlignment = End;
}

style flex-center {
    FlexLayoutMainAxisAlignment = Center;
}

style flex-space-between {
    FlexLayoutMainAxisAlignment = SpaceBetween;
}

style flex-space-around {
    FlexLayoutMainAxisAlignment = SpaceAround;
}
```

![flex-align] (assets/flex-alignment.png)

    
#### `FlexCrossAxisAlignment`     
The cross axis is set perpendicular to the main axis. If your main axis is set to `FlexLayoutDirection` = Column, the cross axis runs along the rows.
  
* `Start` aligns children at the start of the container's cross axis.     
<br/><br/>             
* `End` aligns children at the end of the container's cross axis.
<br/><br/>       
* `Center` aligns children at the center of the container's cross axis.
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's cross axis.
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child, across the container's cross axis.
<br/><br/>  
* `Stretch` distributes children to take up the entire space in the cross axis.
     
#### `FlexItemSelfAlignment`  
Elements can override how they are laid out on the cross axis with this property. 
* `Start` aligns children at the start of the container's cross axis.     
<br/><br/>             
* `End` aligns children at the end of the container's cross axis.
<br/><br/>       
* `Center` aligns children at the center of the container's cross axis.
<br/><br/>         
* `SpaceBetween` aligns children with an equal amount of space between them across the container's cross axis.
<br/><br/>        
* `SpaceAround` aligns children with equal space between them, where space is allocated to the beginning of the first child and the end of the last child, across the container's cross axis.  
<br/><br/> 
* `Stretch` distributes children to take up the entire space in the cross axis.

<br/>

---------------------------------------------------------------

  
    
#### `FlexItemGrow : int`    
When there is remaining space, defines what ratio of that extra space the element will get  

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

![flex-align] (assets/flex-grow.png)
  
#### `FlexItemShrink : int`   
 When there is not enough space to fit all elements, define what ratio of that extra space gets deducted from the element  
  
#### `FlexLayoutWrap : LayoutWrap`   
Defines whether flex items area forced on a single line or multiple lines in the flexbox container  
