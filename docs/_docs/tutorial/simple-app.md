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
notice these nodes in the [UIForia Inspector](/docs/tools/#uiforia-inspector) even if you 
didn't add them yourself.

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

### Attribute and Input State Style Groups
A style group can also host special style groups itself. There are three [input](/docs/input) states: `hover`,
`active` and `focus`, for which there may be an equivalent sub style group.

`MyAppRoot.style`:
```
style big-text {
    TextFontSize = 22px;
    [hover] { TextColor = yellow; }
    [active] { TextColor = green; }
    [focus] { TextColor = red; }
}
```
You can add any style property to these sub style groups of course. Hover over the text to change its color to
yellow or hold your left mouse button down to change it to green. Notice that `[focus]` is a bit special,
as it requires the element to be focusable in the first place. A simple text can not get focus (yet) but an
[`<Input>` element](/docs/elements#inputelement) might!

There's a whole [guide around style sheets](/docs/style), which you should check out.
You should also have a look at the [list of built-in elements](/docs/elements#built-in-elements).

## Bindings and Expressions
Right in the beginning of this example we showed you a binding in the template already.
A binding is an expression of code that will be evaluated every frame (if it's not static).
Its result will be inserted at the position in the template (there are no bindings for
style sheets yet).

Going back to the example you can see the property `safeWord` in curly braces. Curly braces
start and end expressions in node bodies. The scope of the expression is always your
backing class, which means you can refer to all of its public properties and methods in your
template by just putting them in curly braces and using them as if they were real C# code.
[Have a look all features of expressions here](/docs/expressions).

`MyAppRoot.xml`:
```xml
<UITemplate>
    <Style path="MyAppRoot.style" />
    <Contents>
        The safe word is {safeWord}!
    </Contents>
</UITemplate> 
```

Now lets add one more element to the template that accepts a binding as a parameter.

`MyDisplayThing.cs`:
```c#
[Template("MyDisplayThing.xml")]
public class MyDisplayThing : UIElement {
    public string value;
}
```

`MyDisplayThing.xml`:
```xml
<UITemplate>
    <Contents>
        <Div>I'm displaying your value {value}</Div>
    </Contents>
</UITemplate> 
```

We created a new element called `MyDisplayThing` that we refer to by its class name in 
templates as `<MyDisplayThing>`. Note: Element class names must be unique in your assembly 
as you _don't_ prefix them with namespaces in templates like 
`<YourNameSpace.MyDisplayThing>`. 
Ok, let's see how custom elements are used:

`MyAppRoot.xml`:
```xml
<UITemplate>
    <Style path="MyAppRoot.style" />
    <Contents>
        <MyDisplayThing value="safeWord" />
    </Contents>
</UITemplate> 
```

The `value` parameter binding will ensure that the element instance always has an up to date
reference (or copy for value types). So whenever your code changes the `safeWord` property
in the parent, the bound value in children will be updated.

Some built-in attributes are not expecting expressions by default but rather plain strings.
The `style` attribute is one example. Let's add a custom style to the element instance:

`MyAppRoot.xml`:
```xml
<UITemplate>
    <Style path="MyAppRoot.style" />
    <Contents>
        <MyDisplayThing value="safeWord" style="align-center" />
    </Contents>
</UITemplate> 
```

`MyAppRoot.style`:
```
style align-center {
    AlignX = Center Screen;
    AlignY = Center Screen;
}
```

But what if you want a special `style group` to be assigned to the element if some
condition is met? You'd need an expression instead, which would run as a binding 
every frame and update your style attribute.

Curly braces are not necessary here. Instead we use brackets since we might (and will)
add more than one style group in that expression at once. A static style group for 
centering and a dynamic style group in case our condition is true.

`MyAppRoot.xml`:
```xml
<UITemplate>
    <Style path="MyAppRoot.style" />
    <Contents>
        <MyDisplayThing value="safeWord" 
                        style="['align-center', string.IsEmpty(safeWord) ? 'empty' : '']" />
    </Contents>
</UITemplate> 
```

`MyAppRoot.style`:
```
style empty {
    BackgroundColor = rgba(200, 0, 0, 128);
}
```

You can use regular C# in all expressions! The return type of this expression has to be 
`string`, so we have to return an empty string in case the condition is false, meaning
there will be no assignment of an additional style group.

