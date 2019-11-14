---
id: Animation
title:  Animation
tags: 
 - animation
layout: page
---

# Animations

Hierarchical animations are made easy with UIForia. Animations are declarative via style sheets and integrated with the game via eventing. 

There are options for options for delay, offset, loop timing, loop direction control, and multiple curve types.   
  
Multiple types: Sequences, groups, properties, key-frames

Animations can be styled with [UIForia Style Properties](StyleProperties.md)
                              

## Animation in the Stylesheet
You can implement animations within the stylesheet. The structure is the same as creating a style for an element, except replace `style` with `animation`:
```
animation name {
    [options] {
        duration = 1000;
        iterations = 1;
        timingFunction = CubicEaseInOut;
    }
    [keyframes] {
        0% { 
            BackgroundColor = purple;
            PreferredWidth = 50px;        
        }
        100% {
             BackgroundColor = purple;
             PreferredWidth = 550px;
        }
    }
}
```

`run animation(name)` in the element will execute the properties you have set in the animation:
```
style name {
    run animation(name);
}
```


You have access to ```[options]``` and ```[keyframes]``` within the stylesheet. In order to implement more complicated features like state or triggers, you will need to declare those data structures and their properties in your C# script.

## Animation Properties

**AnimationData**:
  
`AnimationOptions` sets the settings, timing, and direction of your animations.  
`AnimationKeyFrame` sets styles per keyframe.   
`AnimationTrigger` sets animation triggers.     
`AnimationState` is a readonly property for returning state values.

---------------------------------------------------------------


### [options] 
In milliseconds 
  
Properties        | Description         | Type
----------------- |-----------------    |---------                                    
 loopTime         | sets the duration of a loop | float
 iterations       | sets the number of times the animation runs | int
 delay            | sets a delay                    | duration
 forwardStartDelay| sets a delay at the beginning                    | int
 reverseStartDelay|                     | int
 direction        | can be set to `Forward` or `Reverse`  | AnimationDirection
 loopType         | can be set to `Constant` or `PingPong`| AnimationLoopType
 timingFunction   | sets the [Easing function](#easing-functions)  | EasingFunction
 playbackType     |  determines ow the animation should be played. Set to `KeyFrame`, `Parallel`, or `Sequential` | animationPlaybackType


<br/>

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

 

 

For visual examples of easing functions, see https://easings.net

<br/>

---------------------------------------------------------------









## Menu Warning Example
![] (/static/img/animation-warning.gif)
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
