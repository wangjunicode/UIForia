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
 OnMouseMove    | mouse:move         | MouseInputEvent | Fires if the mouse position changed
 OnMouseEnter   | mouse:enter        | MouseInputEvent | The mouse enters the bounds of the element
 OnMouseHover   | mouse:hover        | MouseInputEvent | The mouse wanders within the bounds of the element
 OnMouseExit    | mouse:exit         | MouseInputEvent | The mouse left the bounds of the element
 OnMouseDown    | mouse:down         | MouseInputEvent | Any mouse button is down
 OnMouseUp      | mouse:up           | MouseInputEvent | Any mouse button is up
 OnMouseClick   | mouse:click        | MouseInputEvent | The mouse clicked the element with the left button. Need double or triple click? Use the MouseInputEvent's IsDoubleClick and IsTripleClick properties.
 OnMouseContext | mouse:context      | MouseInputEvent | Use this for right click behavior
 OnMouseWheel   | mouse:scroll       | MouseInputEvent | Fires when you roll your mouse wheel
 
### Drag Events

 Attribute      | Template Attribute | $event           | Description
----------------|--------------------|------------------|--------------------------------------------------
 OnDragCreate   | drag:create        | MouseInputEvent  | Fires when dragging starts. All mouse buttons can drag.
 OnDragMove     | drag:move          | DragEvent [^1]   | Fires when the mouse moves and still drags.
 OnDragEnter    | drag:enter         | DragEvent [^1]   | Fires when this element is entered while dragging. Activate a drop area with this event maybe?
 OnDragHover    | drag:hover         | DragEvent [^1]   | Fires when your mouse hovers an element while dragging.
 OnDragExit     | drag:exit          | DragEvent [^1]   | Fires when this element is exited while dragging.
 OnDragDrop     | drag:drop          | DragEvent [^1]   | You just dropped whatever you were dragging. Right here. 
 OnDragCancel   | drag:cancel        | DragEvent [^1]   | Hit escape while dragging? Use this if you want to do something if dragging stopped unnaturally.

 [^1] When implementing a custom OnDragCreate handler you have to return a custom DragEvent. You can cast $event to 
 your custom DragEvent type in those event handlers.
     
### Keyboard Events

## IFocusable