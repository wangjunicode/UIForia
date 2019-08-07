## Data Structures for Animations 
Hierarchical animations made easy with UIForia. It can be simple easing tweens or really complex. Integrate with the game via eventing. Options for delay, offset, loop timing, loop direction control, multiple curve types.  Declarative via style sheets. Integrate with data binding engine to be dynamically targetable.  
  
Multiple types: Sequences, groups, properties, key-frames


**AnimationData** has the following properties:

AnimationData Properties           | Values
----------------- |:------------------------------------------------------                                    
`AnimationOptions`  | float `loopTime`<br> int `iterations`<br> float `delay` <br> int `duration` <br> int `forwardStartDelay` <br> int `reverseStartDelay` <br> AnimationDirection `direction` // Forward or Reverse <br> AnimationLoopType `loopType` // Constant or PingPong <br> EasingFunction `timingFunction` <br> AnimationPlaybackType `playbackType` // KeyFrame, Parallel, or Sequential
`AnimationKeyFrame` |  float `key` //frame rate<br> StyleProperty `property` // assign any of the UIForia styling properties 
`AnimationTrigger`  | `time` = frames <br> fn = `function`
`onStart` <br> `onEnd` <br> `onCanceled` <br>  `onEnd` <br> `onCanceled` <br> `onCanceled` <br> `onCompleted` <br> `onTick` |  UIElement `target` <br> AnimationEventType `evtType` <br> AnimationState `state` <br> AnimationOptions `options`                                 
`AnimationEventType`| `Trigger` <br> `Tick` <br> `DirectionChange` <br> `Start` <br> `Reset` <br> `End` <br> `Complete` <br> `Cancel` <br>    
 `AnimationState`    | UIelement `target` <br> float `elapsedTotalTime` <br> float `elapsedIterationTim` <br> int `iterationCount` <br> int `frameCount` <br> `totalProgress` <br> `iterationProgress` <br> UITaskState `status` <br> int `currentIteration`
 
 *EasingFunction =    Linear, QuadraticEaseIn, QuadraticEaseOut, QuadraticEaseInOut, CubicEaseIn, CubicEaseOut,CubicEaseInOut,QuarticEaseIn, QuarticEaseOut, QuarticEaseInOut, QuinticEaseIn, QuinticEaseOut, QuinticEaseInOut, SineEaseIn, SineEaseOut, SineEaseInOut, CircularEaseIn, CircularEaseOut, CircularEaseInOut, ExponentialEaseIn, ExponentialEaseOut, ExponentialEaseInOut, ElasticEaseIn, ElasticEaseOut, ElasticEaseInOut, BackEaseIn, BackEaseOut,BackEaseInOut, BounceEaseIn,BounceEaseOut,BounceEaseInOut
  
       
Example using AnimationOptions:
```
public AnimationData RedBgAnim() {
            AnimationOptions options = new AnimationOptions() {
                duration = 3200, 
                iterations = -1 // infinite iterations
            };
```

*AnimationData* struct must be used as a type when creating a new animation object and using its properties.   
`AnimationOptions` sets the settings, timing, and direction of your animations.  
`AnimationKeyFrame` sets your styles per keyframe.   
`AnimationTrigger` sets animation triggers.     
`AnimationState` sets the state.    

