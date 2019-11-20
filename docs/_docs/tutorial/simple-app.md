---
title: Simple App Tutorial
layout: page
showInMain: true
description: Learn how to build a simple UI
tags:
  - tutorial
order: 2
---

# Simple App Tutorial
Continuing the [getting started section](/docs/getting-started) we'll assume you have the following setup in your `template root path`:

`MyAppRoot.cs`:

```C#
using UIForia.Attributes;
using UIForia.Elements;

[Template("MyAppRoot.xml")]
public class MyAppRoot : UIElement {
    public string safeWord = "bananas";
}
```

`MyAppRoot.xml`:

```xml
<UITemplate>
    <Style path="MyAppRoot.style" />
    <Contents>
        The safe word is {safeWord}!
    </Contents>
</UITemplate> 
```

`MyAppRoot.style`: _still empty_

## Architecture of a UIElement
Let's double back for a second and look at the individual parts.

### Backing class
Writing your own UIElement always requires a backing class and they have to inherit from `UIForia.Elements.UIElement`.
Instantiation is handled via UIForia internally, so please don't rely on constructors for initialization.
All public fields and methods of your backing class may be used as [bindings](/docs/data-binding) in templates.
Public fields may also serve as parameters and receive values from bindings in parent elements.

The template path has to be defined with the `Template` attribute and is relative to your template root path.
You can omit the template definition by inheriting from [UIContainerElement](/docs/elements#containers),
which might be useful if your element is purely functional, semantical or uses a custom [painter](/docs/painter-api)
to render itself.

There are a couple of [life-cycle methods](/docs/circle-of-life) to override to initialize, update or clean up
your element state.

### Template
UIForia [templates](/docs/templates) are xml based and require only little mandatory structure. 

```xml
<UITemplate>
    <Contents>
        your content here
    </Contents>
</UITemplate> 
```

Must have:
- exactly one `<UITemplate>` node
- a `<Contents>` element in your `<UITemplate>` node

May have:
- zero or more `<Style>` references in your `<UITemplate>` node
- a `<Using>` directive to import namespaces
- actual content in the `<Contents>` node

### Style sheets
A style sheets let you set properties to change the element's layout. This could be fonts, 
[colors](/docs/misc#list-of-all-supported-colors), [sizes](/docs/style/style-units), shadows, positions,
[layout types](/docs/layout) or [animations](/docs/animations). You group these properties in
what we call `style groups`. A style group must have a unique name per style sheet.
A simple one that sets the background and text color would look like this:

```
style mycontainer {
    BackgroundColor = black;
    TextColor = white;
}
```

Apply this style to any node within `<Contents>`, even to the node itself:

```xml
<UITemplate>
    <Contents style="mycontainer">
        your content here
    </Contents>
</UITemplate> 
```

If you need your text to be styled individually just wrap it in the `<Text>` node.
The `<Text>` node is special, as it is automatically inserted during compile time so you'll
notice these nodes even if you didn't add them yourself.

`MyAppRoot.style`:
```
style big-text {
    TextFontSize = 22px;
}
```

`MyAppRoot.xml`:
```xml
<UITemplate>
    <Contents style="mycontainer">
        your content here
        <Text style="big-text">Slighlty bigger text</Text>
    </Contents>
</UITemplate> 
```

There are sensible defaults for every property. `TextFontSize` defaults at 18px, `TextColor` is black.
Some properties are inherited, though, so when running this example you'll see that all text white
although you did not specify a `TextColor` for the `big-text` style group.
There's a reference for every [style property](/docs/style/style-properties), too!

A style group can also host special style groups itself: 
### attribute and input state
There's a whole [guide around style sheets](/docs/style), which you should check out.
