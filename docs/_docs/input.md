---
title: Input
layout: page
tags:
 - input
---

# UIForia's InputSystem

## Capture and Bubble Phase
Events in UIForia first "bubble up" the element hierarchy, that is the input event will be triggered first on the
element that got clicked or dragged over, then subsequently triggered on every parent element. After hitting the
root of the element tree the capture phase begins and the event bubbles back down again. Event handlers can choose
in which phase they want to trigger.  

## Input Event Bindings

### Mouse Events

 Attribute       | Template Attribute | $event          | Description
-----------------|--------------------|-----------------|--------------------------------------------------
 OnMouseMove     | mouse:move         | MouseInputEvent | Fires if the mouse position changed
 OnMouseEnter    | mouse:enter        | MouseInputEvent | The mouse enters the bounds of the element
 OnMouseHover    | mouse:hover        | MouseInputEvent | The mouse wanders within the bounds of the element
 OnMouseExit     | mouse:exit         | MouseInputEvent | The mouse left the bounds of the element
 OnMouseHeldDown | mouse:helddown     | MouseInputEvent | Any mouse button is being held down
 OnMouseDown     | mouse:down         | MouseInputEvent | Any mouse button is down
 OnMouseUp       | mouse:up           | MouseInputEvent | Any mouse button is up
 OnMouseClick    | mouse:click        | MouseInputEvent | The mouse clicked the element with the left button. Need double or triple click? Use the MouseInputEvent's IsDoubleClick and IsTripleClick properties.
 OnMouseContext  | mouse:context      | MouseInputEvent | Use this for right click behavior
 OnMouseWheel    | mouse:scroll       | MouseInputEvent | Fires when you roll your mouse wheel
 
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
   // todo      | drag:update        | DragEvent [^1]   | // todo

 [^1] When implementing a custom OnDragCreate handler you have to return a custom DragEvent. You can cast $event to 
 your custom DragEvent type in those event handlers.
     
### Keyboard Events

 Attribute      | Template Attribute      | $event             | Description
----------------|-------------------------|--------------------|--------------------------------------------------
 OnKeyDown      | key:down                | KeyboardInputEvent | 
 OnKeyUp        | key:up                  | KeyboardInputEvent |
 OnKeyHeldDown  | key:held / key:helddown | KeyboardInputEvent | 
 OnKeyHeldDownWithFocus  | key:held.focus | KeyboardInputEvent | Only fires if the element also has focus

## Modifiers
Every input event (mouse, drag, keyboard) supports additional modifiers.
The syntax is simple, just append a `.modifier` to your event like this: `mouse:down.shift` (fires only if any shift key is also pressed)

Currently supported modifiers:
- focus
- capture
- shift
- ctrl / control
- cmd / command
- alt 

## IFocusable
Add the `IFocusable` interface to any element that may get focus, e.g. custom input elements or context menus.

In order to focus an element it needs to request it from UIForia's InputSystem:

```
    [OnMouseClick]
    public void OnClick() {
        application.InputSystem.RequestFocus(this);
    }
```

The IFocusable interface comes with two methods that need to implemented:

`public bool Focus()`, which gets executed when the InputSystem's `RequestFocus` gets called. If, for some reason,
the element does not want to accept the focus you'd return false here, maybe because it's in a custom disabled state.

`public void Blur()` gets called when the element had the focus but another element requested (and got) it or
when the focus just got released.

## Other characteristics of an InputEvent
### Event Propagation
Every input event has the option to stop the bubble/capture event propagation, i.e. the event will not trigger any
further event handlers: `evt.StopPropagation()`

Less drastic but similar in nature is the option to mark an input even as consumed: `evt.Consume()`
This only sets a flag in the event. O

