---
id: EditorTools
title: Editor Tools
layout: page
tags:
 - editor
---

# Editor Tools

## UIForia Hierarchy  
Go to **Window -> UIForia Hierarchy** within Unity    

Within UIForia Hierarchy, select “Game App” inside the Application dropdown. This will load a tree structure of all the elements from your XML file. 
  
After selecting “Game App” in the UIForia Hierarchy, toggle **Activate Select Mode** to use the UIForia Inspector. You can now hoover over elements in the Game view. 

### Show Meta Data
Displays meta data, i.e. the unique id of the element.

### Activate Select Mode
Activate this, focus the Game panel in Unity (click on the tab on windows or click anywhere into it on macOS), move your mouse, observe.
While hovering elements you'll see the hierarchy and inspector updating. Left-click on anything in your UI and the selection mode will stop.

### Show Disabled
Displays disabled elements. 
  
![Image of UIForia Hierarchy](/assets/img/UIForia_Hierarchy.png)

## UIForia Inspector
Go to **Window -> UIForia Inspector** within Unity
  
UIForia’s Inspector displays styles and states of a selected element. To select an element, pick one from the 
UIForia Hierarchy. Once selected you'll find four tabs to choose from: 

### Element
Here you'll see:
- attributes that have been set via template or code on the element (`<Group x-id="nav"/>`: id would show up)
- view and view port that the element is defined in
- local and screen position - the local position is always relative to the element's direct parent
- [allocated and actual size](/docs/layout/#layoutbox-and-allocated-size)

### Applied Styles
Shows you a list of all applied styles, the end result after all applicable style groups have been applied.

On the top of the panel you'll see some controls to force the element to enter one of the special input states: hover, active, focus

If a style defines a run command (`run animation`) you'll see a list of them and some of their configuration in the bottom.
  
### Computed Styles 
Similar to applied styles in features, but you'll have some debug tools too and two toggles:
- show all - gives you a list of all style properties including all default values
- show sources - groups the style properties by their source style group

### Settings
Enable **Draw Debug Box** in the settings to see the box model highlighting when selecting elements in the hierarchy.
Also feel free to change the box model color scheme.

![Image of UIForia Hierarchy](/assets/img/UIForia_Inspector.png)


