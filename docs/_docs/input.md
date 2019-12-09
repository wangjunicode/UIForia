---
title: Input
layout: page
tags:
 - input
---

# Input

## Capture and Bubble Phase

## Input Event Bindings

### Mouse Events

 Attribute      | Template Attribute | $event          | Description
----------------|--------------------|-----------------|--------------------------------------------------
 OnMouseMove    | onMouseMove        | MouseInputEvent | Fires if the mouse position changed
 OnMouseEnter   | onMouseEnter       | MouseInputEvent | The mouse enters the bounds of the element
 OnMouseHover   | onMouseHover       | MouseInputEvent | The mouse wanders within the bounds of the element
 OnMouseExit    | onMouseExit        | MouseInputEvent | The mouse left the bounds of the element
 OnMouseDown    | onMouseDown        | MouseInputEvent | Any mouse button is down
 OnMouseUp      | onMouseUp          | MouseInputEvent | Any mouse button is up
 OnMouseClick   | onMouseClick       | MouseInputEvent | The mouse clicked the element with the left button. Need double or triple click? Use the MouseInputEvent's IsDoubleClick and IsTripleClick properties.
 OnMouseContext | onMouseContext     | MouseInputEvent | Use this for right click behavior
 OnMouseWheel   | onMouseScroll      | MouseInputEvent | Fires when you roll your mouse wheel
 
### Drag Events

 Attribute      | Template Attribute | $event           | Description
----------------|--------------------|------------------|--------------------------------------------------
 OnDragCreate   | onDragCreate       | MouseInputEvent  | Fires when dragging starts. All mouse buttons can drag.
 OnDragMove     | onDragMove         | DragEvent [^1]   | Fires when the mouse moves and still drags.
 OnDragEnter    | onDragEnter        | DragEvent [^1]   | Fires when this element is entered while dragging. Activate a drop area with this event maybe?
 OnDragHover    | onDragHover        | DragEvent [^1]   | Fires when your mouse hovers an element while dragging.
 OnDragExit     | onDragExit         | DragEvent [^1]   | Fires when this element is exited while dragging.
 OnDragDrop     | onDragDrop         | DragEvent [^1]   | You just dropped whatever you were dragging. Right here. 
 OnDragCancel   | onDragCancel       | DragEvent [^1]   | Hit escape while dragging? Use this if you want to do something if dragging stopped unnaturally.

 [^1] When implementing a custom OnDragCreate handler you have to return a custom DragEvent. You can cast $event to 
 your custom DragEvent type in those event handlers.
 
 Attribute      | Template Attribute | $event           | Description
----------------|--------------------|------------------|--------------------------------------------------
 OnDragCreate   | onDragCreate       | MouseInputEvent  | Fires when dragging starts. All mouse buttons can drag.   

     
### Keyboard Events

## IFocusable