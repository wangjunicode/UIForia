---
id: Animation
showInMain: true
title:  Animation
tags:
 - uiforia 
 - animation
layout: page
order: 30
---

# Animations
With Animations you can animate from one [style property](StyleProperties.md) value to another. Most numeric style properties can
be animated, like sizes, alignment offsets and transforms.

You can either define your animation in a style sheet (the recommended way) or create one on the spot with the C# API.
Mixing both is possible as well, as you can load an animation definition from a style sheet in C#, customize it and pass it
to the animation API.



## Defining Animations in Style Sheets
```
animation pulse {
    [options] { TimingFunction = QuadraticEaseInOut; }
    [keyframes] {
        0% { TransformScale = 1; }
        50% { TransformScale = 1.105; }
        100% { TransformScale = 1; }
    }
}
```

![pulse](/assets/img/animation_pulse.gif)


The `animation` keyword starts the definition block, followed by a name that you choose, which would be `pulse` in this example.
Inside there may be an optional `[options]` block, which lets you define easing functions, duration, delay and other parameters for
the animation. The `[keyframes]` block is required and you need to add at least one keyframe.

You don't have to start your animation at keyframe `0%` (or end at `100%`) if you want to animate from whatever style property 
value is currently set to something else. This is usually handy when there's code that changes styles as well.

### Keyframe groups
```
animation bounce {
    [keyframes] {
        0%, 20%, 53%, 80%, 100% { TransformPositionY = 0; }
        40%, 43% { TransformPositionY = -30px; }
        70% { TransformPositionY = -15px; }
        90% { TransformPositionY = -4px; }
    }
}
```

![bounce](/assets/img/animation_bounce.gif)

Here's an example of an animation with a group of keyframes defining a common style property value.

Have a look at UIForia's documentation app in [the "Animation" section](https://github.com/klanggames/UIForia/blob/master/Assets/Documentation/Features/AnimationDemo.style)!

## Run Animations
Now that you know how to define an animation we'll look at how to run them. The easiest way would be via [style groups](/docs/style):
```
style container {
    run animation(fadeIn);
}

animation fadeIn {
    [options] { TimingFunction = QuadraticEaseInOut; }
    [keyframes] {
        0% { Opacity = 0; }
        100% { Opacity = 1; }
    }
}
```
![fadeIn](/assets/img/animation_fadeIn.gif)

The animation in this example will now be triggered when the style group `container` becomes active. Any property in a
style group is by default assigned to the `normal` style state group, which is implicitly there. Other style state
groups have to be defined:
```
style container {
    run animation(fadeIn);
    [hover] { }
    [active] { }
    [focus] { }
}
```

You can run animation when any of the states are entered: `[hover] { run animation(flash); }`.

### Run Animations on Exit
The previous examples can be read as: when the state is entered run the animation.
But sometimes you want to run another animation (maybe the inverse) once a state is exited. When hovered a button's background
color might change via animations. But when that state is exited the regular background color should be restored.
To make things easy there are two convenient attributes that you can add to run definitions:
 
`[enter]`, which marks a run definition to execute when the state is entered and `[exit]`, which does the opposite.

Let's look at a full example for our demo buttons:

```
style button {
    Padding = 7px;
    CornerBevelBottomRight = 7px;
    Border = 3px;
    BackgroundColor = rgba(120, 100, 100, 190);
    Margin = 3px;
    TextFontAsset = @theme.fontGothamBold;
    TextFontSize = 11px;
    TextColor = rgb(240, 240, 230);
    [hover] {
        Cursor = @theme.cursorHand;
        [enter] run animation(button-hover);
        [exit] run animation(button-normal);
    }
}

animation button-hover {
    [options] { TimingFunction = QuadraticEaseInOut; }
    [keyframes] { 
        100% { 
            BackgroundColor = rgba(220, 200, 200, 190);
            TextColor = rgb(40, 40, 30);
        }
    }
}

animation button-normal {
    [options] { TimingFunction = QuadraticEaseInOut; }
    [keyframes] { 
        100% {
            BackgroundColor = rgba(120, 100, 100, 190);
            TextColor = rgb(240, 240, 230);
        }
    }
}
```
![hover state](/assets/img/animation_hover_state.gif)

Notice how the `[enter]` animation stops as soon as the cursor leaves the button and the `[exit]` animation immediately kicks in.

## The `[options]` section
```
    [options] {
        TimingFunction = QuadraticEaseInOut;
        Duration = 750;
        Delay = 0.2;
        Direction = Forward;
        TimingFunction = QuadraticEaseInOut;
    }
```

| Properties     | Description                                                                                                                      | Type               | Default |
|:---------------|:---------------------------------------------------------------------------------------------------------------------------------|:-------------------|:--------|
| Iterations     | Defines how many times the anmiation should be repeated. Special values: -1 or Infinite will loop the animation for ever.        | int                | 1       |
| Duration       | Duration of the whole animation in ms.                                                                                           | duration           | 1000    |
| Delay          | Delay the animation in seconds.                                                                                                  | duration           | 0       |
| Direction      | Values: `Forward` or `Reverse`. Play the animation from 0% to 100% when using `Forward` or from 100% to 0% if `Reverse` is used. | AnimationDirection | Forward |
| TimingFunction | Choose an [easing function](#easing-functions) to adjust the change rate of the style properties between keyframes.              | EasingFunction     | Linear  |


---------------------------------------------------------------



### [keyframes] 
Keyframes in UIForia are inspired by CSS Animations. You can set UIForia's styling properties to any number of keyframes. 

Properties        | Description         | Type
----------------- |-----------------    |---------                                    
 key             |  keyframe time values, for e.g. 0%, 50%, 100%         | float
 property        |  name of style property                              | StyleProperty
 
 Keyframe values can be set to any values between 0% and 100% to define the states with each property.
 
```
 [keyframes] {
    0% {
        PreferredWidth = 50px;
    }
    50% {
        PreferredWidth = 80px;
    }
    100% {
        PreferredWidth = 100px;
    }
 }
 ```

<br/>

---------------------------------------------------------------

### AnimationTrigger

 Properties       | Description                                 | Type          
----------------- | ------------------------------------------- |---------                         
 time             | frames                                      | float         
 fun              | animation function that gets triggered      | StyleProperty 
  
```
AnimationTrigger[] triggers = {
                new AnimationTrigger(0.65f, (StyleAnimationEvent evt) => {
                    float start = (int) evt.options.duration * 0.65f;
                    float end = (int) evt.options.duration * 0.9f;
                    int duration = (int) (end - start);
                    one_BlueOrbit.SetEnabled(true);
                    Application.Animate(one_BlueOrbit, OrbitFadeAnim(duration));
                })
};
```

<br/>

---------------------------------------------------------------

 
### StyleAnimationEvent

 Properties        | Description         | Type
 ----------------- |-----------------    |---------                                    
  type             |                     | 
  target           |                     |
  state            |                     |
  options          |                     | 
  
<br/>

---------------------------------------------------------------

  
### AnimationState

 Properties        | Description                    | Type
 ----------------- |-----------------               |---------                                    
  target           |                                | UIelement
 elapsedTotalTime  | returns elapsed total time (milliseconds)  | float
 elapsedIterationTime |  returns iteration time (milliseconds)  | float
 currentIteration  | returns the current iteration              | int
 iterationCount     | returns number of iterations              | int
 frameCount         |  returns number of frames                 | int
 totalProgress      |  returns total progress                   | int
 iterationProgress  |   returns iteration progress              | int
`status` <br/>     |                    |  UITaskState         

int `currentIteration`


---------------------------------------------------------------
 
     
### Easing Functions 
 Easing functions can be created by setting the name of the function to the `timeFunction` property:
 
 BackEaseIn, &nbsp; BackEaseOut, &nbsp; BackEaseInOut, &nbsp; BounceEaseIn, &nbsp; BounceEaseOut, &nbsp; BounceEaseInOut, &nbsp; 
  CubicEaseIn, &nbsp; CubicEaseOut, &nbsp; CubicEaseInOut, &nbsp; QuarticEaseIn, &nbsp; CircularEaseIn, &nbsp; CircularEaseOut, &nbsp; CircularEaseInOut, 
  &nbsp; ExponentialEaseIn, &nbsp;  ExponentialEaseOut, ExponentialEaseInOut, &nbsp; ElasticEaseIn, &nbsp; ElasticEaseOut, &nbsp; ElasticEaseInOut, &nbsp; 
  Linear, &nbsp; QuadraticEaseIn, &nbsp;   QuadraticEaseOut, &nbsp;  QuadraticEaseInOut, &nbsp; QuarticEaseOut, &nbsp; QuarticEaseInOut,  &nbsp; QuinticEaseIn,
  &nbsp; QuinticEaseOut, &nbsp; QuinticEaseInOut,&nbsp; SineEaseIn, &nbsp; SineEaseOut, &nbsp; SineEaseInOut  
 

 
    
**QuinticEaseInEaseIn Example**

``` 
animation easeIn {
          [options] {
              duration = 3200;
              timingFunction = QuinticEaseIn;
          }
          [keyframes] {
              0% {PreferredSize = 48px;}
              100% {PreferredSize = 150px;}
          }
}
```   

![QuadEaseIn](https://media.giphy.com/media/ZCeMiLnmm4kufD0JgI/giphy.gif)

 

 

For visual examples of easing functions, see <https://easings.net>

<br/>

---------------------------------------------------------------









## Menu Warning Example
![](/static/img/animation-warning.gif)
In the example above, we have a warning icon with a yellow arc orbiting around it. 

In [keyframes] we set `TransformRotation` to orbit 360 degrees around the image.

**Stylesheet:**
```
style image {
      BackgroundImage = url("Images/warning");
      LayoutBehavior = Ignored;
      PreferredSize = 35px;
      TransformBehavior = AnchorMaxOffset;
      TransformPosition = 0.5pcaw -0.5h;
}

animation warning {

    [options] {
        duration = 2000;
        iterations = -1;
    }
    [keyframes] {
        0% { 
            TransformRotation = 0;
        }
        100% {
            TransformRotation = -360;
        }
    }
}

style orbit-container {
       LayoutType = Radial;
       PreferredSize = 35px;
       RadialLayoutEndAngle = 50;
       RadialLayoutRadius = 20;
       TransformRotation = 0;
       TransformPivot = 0.5 0.5;
       
       run animation(warning); 
}

style orbit {
    BackgroundColor = yellow;
}

style orbit-0 {
    PreferredSize= 2px;
}

style circle {
    BorderRadius = 50%;
    TransformPivot = 0.5 0.5; 
}
```







  

## Animation in C#
**AnimationData** struct must be used as a type when creating a new animation object and using its properties.


Example using AnimationOptions:
  
```
public AnimationData RedBgAnim() {
            AnimationOptions options = new AnimationOptions() {
                duration = 3200, 
                iterations = -1 // infinite iterations
            };
}
```

### Triggers
In order to use triggers, create a new object `AnimationTrigger` and set the `time` and `fn` values:


```C#
  AnimationTrigger[] triggers = {
                new AnimationTrigger(0.65f, (StyleAnimationEvent evt) => {
                    float start = (int) evt.options.duration * 0.65f;
                    float end = (int) evt.options.duration * 0.9f;
                    int duration = (int) (end - start);
                    one_BlueOrbit.SetEnabled(true);
                    Application.Animate(one_BlueOrbit, OrbitFadeAnim(duration));
                })
 };
```
