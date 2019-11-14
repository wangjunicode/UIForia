---
id: Motivation
title: Motivation
layout: page
---

### Challenges we faced with UI at Klang Games [](https://www.google.com)
We're developing a very UI heavy game called [Seed](https://www.klang-games.com/seed). Our previous solution consisted of using Unity UI with a data binding framework called “Data Bind for Unity”. Our designer authored 
UI in photoshop and then built a hierarchy of UI objects in the Unity editor. After this stage, a developer made it “real” by adding behaviors, linking the data binding framework and implementing animations.

--------------------------------

## How does UIForia work?

#### Template First Approach
UIForia uses text templates for styles and elements, with similar syntax from XML, HTML, and JSX. There is a one to one relationship between templates and elements.



#### Data Binding  
Easy to reason about, no gotchas, precise error messaging   
   
#### Fast by default 
   
* Linq Expressions / Reflection for dev iteration    
* Code gen for release 
     
#### Framework agnostic
UIForia can be dropped into any C# project and works out-of-box. Reference imports and alises are highly customizable. 


#### Sophisticated Layout engine
The layout system consists of flexbox, flow, multi-resolution flex grid, radial, and anchor layout types.  
 
**Layout Features** 
* Formal concept of layers  
* Axis-independent, automatic clipping and scrolling  
* Really easy to reason about  
* Formal box model (margin, border, padding)  
* Element sizing units make sense to designers  
* Pixel, Em, ViewportWidth, ViewportHeight  
* Content Size, Parent Size, Parent Content Area Size  
* Customizable per-element scroll behavior (normal, sticky, fixed)  
* Translate, rotate, scale, pivot behavior is adjustable  

#### Style System 
Styling is similar to CSS and can be defined in code, in a style sheet, or inline in a template.

There are style states and pseudo states for hover, focus, blur, active, and disabled.

UIForia also has event support, which is integrated with animation and layout systems.
 
Over 100 context based, adjustable properties!  


#### Painting 
UIForia's Painter can be used to draw graphics. If you're familiar with HTML canvas, you will notice many similarities.

**Features:** 
* Procedural Shapes	  
* No extra sprites for cutouts, rounding corners, stars, circles, and ellipses
* Multiple fill options 
* Hierarchical opacity settings  
* SDF Text rendering  
* Infinitely nested clipping  
* Full vector graphics package  
* Easy to define effects  
* Particles  
* Auto Sprite batching  
 
#### Approachable Animations
Hierarchical animations are made easy with UIForia. Animations are declarative via style sheets and integrated with the game via eventing.
 
    
There are options for options for delay, offset, loop timing, loop direction control, and multiple curve types.
  
Multiple types: Sequences, groups, properties, key-frames
  
Animations can be styled with [UIForia Style Properties](styleproperties)

#### Input Made Easy
Handlers can be defined directly in templates or on backing-classes via annotations.  
All eventing works via 2 phase capture/bubble.  
* Events can be consumed and told to stop propagating  
* No leaks. User never needs to manage event handler life cycle  

* Bonus: No GameObjects, flat memory usage  
* More Bonus: Usable in inspectors and editor windows in exactly the same way as the game with 0 code changes.  




