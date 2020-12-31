---
title: Stack Layout Guide
tags: 
 - layout
layout: page
---

# Stack Layout

In a stack layout the default size of the element is determined based on the max size of its children.
All children are aligned on top of each other.

So, how is this useful? Well, imagine an element that renders a bunch of items and has a few images
or buttons aligned around itself. As the biggest element the Repeat will dictate the size of the whole
box and the "satellite" buttons can be aligned to float in the corners without affecting the position
of the main content.

Of course all those buttons can also receive `LayoutBehavior = Ignored;` but 
[`Ignored`](/docs/style/style-properties#layoutbehavior) comes with a tiny performance overhead.

```
style stack {
    LayoutType = Stack;
}
```

```xml
<Contents style="stack">
    <Repeat list="items">
        <Item data="$item"/>
    </Repeat>
    <Button style="align-top-left"/>
    <Button style="align-top-right"/>
    <Button style="align-bottom-right"/>
</Contents>
```

Let's make another example where we want three images to render on top of each other:

```xml
<Contents style="stack">
    <Image src="background1" />
    <Image src="background2" />
    <Image src="background3" />
</Contents>
```

The size of the outer element will again be determined by the largest child. If all images have the
same dimensions you can combine different layers of backgrounds for stylistic reasons. Maybe expand
this pattern into a parallax effect even!

Stack layouts make the element as wide as the widest child and as tall as the highest child. That
might be the solution to a third kind of use case: 

```xml
<Contents style="stack">
    <Div style:preferredWidth="400">A very wide element</Div>
    <Div style:preferredHeight="400">A very tall element</Div>
</Contents>
```

The outer element will now be 400px by 400px big. 