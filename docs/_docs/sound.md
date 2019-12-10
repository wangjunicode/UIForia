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

```xml
<Group style="button-container">
    <Button style="button ping-button">Ping Button</Button>
    <Button style="button pong-button">Pong Button</Button>
</Group>
<Div>Current Sound {currentSound.Name}</Div>
```

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

## Sound Properties

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
