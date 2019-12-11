---
id: Sound
showInMain: true
title: Stylable Sound
tags:
 - uiforia 
 - sound
layout: page
order: 31
---

# Stylable Sound
Similar to [Animations](/docs/animations) you can also create `sound` sections that let you define
and configure sounds and use them using a `run` command in a style group.

```
[Template("Documentation/Features/SoundSystemDemo.xml")]
public class SoundSystemDemo : UIElement {

    public UISoundData currentSound;
    
    public override void OnCreate() {
        Application.SoundSystem.onSoundPlayed += evt => { currentSound = evt.SoundData; };
        Application.SoundSystem.onSoundStopped += evt => { currentSound = default; };
    }
}
```
Subscribe to the event handlers `Application.SoundSystem.onSound*` to get notified when a sound
starts, stops, pauses or resumes playing. That's where you would use the event's `UISoundData`
to play the sound.

```xml
<Group style="button-container">
    <Button style="button ping-button">Ping Button</Button>
    <Button style="button pong-button">Pong Button</Button>
</Group>
<Div>Current Sound {currentSound.name}</Div>
```

Here we're settings up a simple template that triggers sound events on hover and displays the
currently playing sound data.

```
sound ping {
    Asset = "Sounds/ping";
    Duration = 1s;
    Volume = 0.5;
    Pitch = 1.4;
}

sound pong {
    Asset = "Sounds/ping";
    Duration = 3s;
    Volume = 0.6;
    Pitch = 0.2;
}

style ping-button {
    [hover] {
        [enter] run sound(ping);
        [exit] stop sound(ping);
    }
}

style pong-button {
    [hover] {
        [enter] run sound(pong);
        [exit] stop sound(pong);
    }
}
```

Notice the `[enter]` and `[exit]` markers as well as the support for the different run command actions:
We support `run`, `pause`, `resume` and `stop`. When the run command is triggered the corresponding
event callback in `Application.SoundSystem` will be called.

## Sound Properties (a.k.a. `UISoundData`)
These are all the properties that are supported in `sound` groups in a style sheet.
They correspond to the fields in `UISoundData`, which is part of the event object sent by the 
`Application.SoundSystem` callback events.

| Property   | Type              | Description                                   |
|:-----------|:------------------|:----------------------------------------------|
| Asset      | string            | Path or custom identifier of your sound file. |
| Pitch      | float             | Default: 1                                    |
| Volume     | float             | Default: 1                                    |
| PitchRange | FloatRange        | Usage: `range(min, max)` -> `range(0.1, 0.4)` |
| Tempo      | float             | Default: 1                                    |
| Duration   | UITimeMeasurement | Default: 100%, supported units: %, s, ms      |
| Iterations | int               | Default: 1                                    |
| MixerGroup | string            | Refers to Unity Mixer Group                   |
