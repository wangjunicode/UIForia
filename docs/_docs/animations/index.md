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
With Animations you can animate from one [style property](/docs/style) value to another. Most numeric style properties can
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
groups have to be defined and you might already know them:
```
style container {
    run animation(fadeIn);
    [hover] { }
    [active] { }
    [focus] { }
}
```

You can run animation when any of the states are entered: `[hover] { run animation(flash); }`.

The same applies to attribute style groups, which means you could trigger an animation by setting / unsetting an element's
attribute value.

groups have to be defined and you might already know them:
```
style container {
    [attr:display="show"] {
        run animation(fadeIn);
    }
}
```
```c#
element.SetAttribute("display", "show");
```

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
In the previous examples we already used the `[options]` block a couple of times to change our animation settings. 
Let's look at all the options that are currently supported:

```
    [options] {
        Iterations = Infinite;
        Duration = 750;
        Delay = 0.2;
        Direction = Forward;
        TimingFunction = QuadraticEaseInOut;
    }
```

| Properties     | Description                                                                                                                      | Type               | Default  |
|:---------------|:---------------------------------------------------------------------------------------------------------------------------------|:-------------------|:---------|
| Iterations     | Defines how many times the anmiation should be repeated. Special values: -1 or Infinite will loop the animation for ever.        | int                | 1        |
| Duration       | Duration of the whole animation.                                                                                                 | UITimeMeasurement  | 1000ms   |
| Delay          | Delays the start of the animation.                                                                                               | UITimeMeasurement  | 0ms      |
| Direction      | Values: `Forward` or `Reverse`. Play the animation from 0% to 100% when using `Forward` or from 100% to 0% if `Reverse` is used. | AnimationDirection | Forward  |
| LoopType       | Values: `Constant` (default) or `PingPong`. Plays the animation in reverse until the duration or iteration count is met.         | AnimationLoopType  | Constant |
| TimingFunction | Choose an [easing function](#timing-functions) to adjust the change rate of the style properties between keyframes.              | EasingFunction     | Linear   |

## C# Animation API
Animations done via style sheets are very powerful already but sometimes you just need them to be extra customized.
With the Animation API you can load an animation definition from a style sheet or create your own from scratch.
It also gives you the power to pause, resume and stop a running animation. And you can subscribe to the animation's
life-cycle events.

(Try not to over-use the C# API since style sheet based animations are always easier to read and maintain.)

### Load Animation From Style Sheet
Inside your `UIElement` you can get a copy of any animation definition from your style sheets.
```
AnimationData flashAnimationData = Application.GetAnimationFromFile("Documentation/Features/AnimationDemo.style", "flash");
```

You can change all the options or even add keyframes:
```
flashAnimationData.options.duration = 1200;
animationData.frames.Add(new AnimationKeyFrame(0.3f, new StyleKeyFrameValue[] {
        new StyleKeyFrameValue(StylePropertyId.PreferredWidth, "200px"), 
}));
```

Then run the animation on any element you want:
```
Application.Animate(animationTarget, flashAnimationData);
```

Note that we refer to keyframes in code with floats between 0 and 1. The keyframe at
`30%` of the animation was therefore defined as `0.3f`. 

### UIForia.Animation.AnimationTrigger
AnimationTriggers are custom event callbacks that get triggered on specific keyframes.

```
AnimationTrigger[] triggers = {
        new AnimationTrigger(0.65f, (StyleAnimationEvent evt) => {
            if (evt.options.duration.HasValue) {
                float start = evt.options.duration.Value.AsSeconds() * 0.65f;
                float end = evt.options.duration.Value.AsSeconds() * 0.9f;
                int duration = (int) (end - start);
                one_BlueOrbit.SetEnabled(true);
                Application.Animate(one_BlueOrbit, OrbitFadeAnim(duration));
            }
        })
};
```

When the animation reaches `65%` of its runtime the custom `StyleAnimationEvent` will be run.
In that example we start another customized animation at this keyframe.   

## Event Callbacks
There are five built in event callbacks that you might choose over an animation trigger:
- onStart
- onEnd
- onCanceled
- onCompleted
- onTick

To run custom code as soon as an animation is done you'd do this:
```
animationData.onCompleted = (StyleAnimationEvent evt) => { };
```

### Timing Functions 
The `TimingFunction` option can be one of the following: 
- Linear (which is the default)
- QuadraticEaseIn
- QuadraticEaseOut
- QuadraticEaseInOut
- CubicEaseIn
- CubicEaseOut
- CubicEaseInOut
- QuarticEaseIn
- QuarticEaseOut
- QuarticEaseInOut
- QuinticEaseIn
- QuinticEaseOut
- QuinticEaseInOut
- SineEaseIn
- SineEaseOut
- SineEaseInOut
- CircularEaseIn
- CircularEaseOut
- CircularEaseInOut
- ExponentialEaseIn
- ExponentialEaseOut
- ExponentialEaseInOut
- ElasticEaseIn
- ElasticEaseOut
- ElasticEaseInOut
- BackEaseIn
- BackEaseOut
- BackEaseInOut
- BounceEaseIn
- BounceEaseOut
- BounceEaseInOut

For visual examples of easing functions, see <https://easings.net>

#### Custom Easing Functions
Custom easing function are currently not supported :(

## Sprite Animations
Basic sprite animations are here! We support sprite based animations based on individual images.
Put all your sprite images into some directory under `Resources` and make sure they all start with the
same prefix and end with their frame number. The image `SpriteAnimation/Frame_2.png` would match
this `PathPrefix` configuration: `PathPrefix = "SpriteAnimation/Frame_";`.

Currently all frames are being played linearly, so if you want a particular frame to last a little
longer you'll have to use the `C#` API to add a [Trigger](/docs/animations/index#uiforiaanimationanimationtrigger)
that pauses the animation for a while. Note that `AnimationTrigger.time` will return the current frame count
instead of a keyframe.

The `spritesheet` group takes a subset of `AnimationOptions` known from animation `[options]` groups with
some small differences:
- Duration will does not limit the animation to 1 second by default, instead the every frame will get screen time
- TimingFunction is not supported

Well, here's the full list again:

| Properties     | Description                                                                                                                      | Type               | Default  |
|:---------------|:---------------------------------------------------------------------------------------------------------------------------------|:-------------------|:---------|
| Fps            | Defines the frames per second.                                                                                                   | int                | 30       |
| StartFrame     | The first frame to use from your sprite animation.                                                                               | int                | 0        |
| EndFrame       | The last frame to use from your sprite animation.                                                                                | int                | 0        |
| PathPrefix     | Common cath and prefix that is shared by all your sprite frames. Totally required!                                               | string             |          |
| Iterations     | Defines how many times the anmiation should be repeated. Special values: -1 or Infinite will loop the animation for ever.        | int                | 1        |
| Duration       | Duration of the whole animation.                                                                                                 | UITimeMeasurement  | **100%**     |
| Delay          | Delays the start of the animation.                                                                                               | UITimeMeasurement  | 0ms      |
| Direction      | Values: `Forward` or `Reverse`. Play the animation from 0% to 100% when using `Forward` or from 100% to 0% if `Reverse` is used. | AnimationDirection | Forward  |
| LoopType       | Values: `Constant` (default) or `PingPong`. Plays the animation in reverse until the duration or iteration count is met.         | AnimationLoopType  | Constant |
| TimingFunction | **NOT SUPPORTED**                                                                                                                      | -                  | -        |

An example of a `spritesheet` group:
```
spritesheet walk {
    PathPrefix = "SpriteAnimation/Frame_";
    StartFrame = 4;
    EndFrame = 10;
    Fps = 10;
    Delay = 1;
    Iterations = Infinite;
}
```

...and for more, please have a look at our AnimationDemo 
- [style](https://github.com/klanggames/UIForia/blob/master/Assets/Documentation/Features/AnimationDemo.style)
- [xml](https://github.com/klanggames/UIForia/blob/master/Assets/Documentation/Features/AnimationDemo.xml)
- [and backing class](https://github.com/klanggames/UIForia/blob/master/Assets/Documentation/Features/AnimationDemo.cs)