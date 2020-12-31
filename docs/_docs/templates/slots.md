---
id: Slots
title: Slots
tags:
- template
  layout: page
---

# Slots
A template slot is a portion of an element that can be overridden.

ElementA might define a slot `title`:
```xml
<Contents>
    <define:title>default title</define:title>
</Contents>
```

When using the element it can be overridden:
```xml
<Contents>
    <ElementA>
        <override:title>{GetTitle()}</override:title>
    </ElementA>
</Contents>
```

If you element should accept regular children elements you have to define
a children slot:

ElementB:
```xml
<Contents>
    <Div>My content</Div>
    <define:Children />
</Contents>
```

The only specialty here is that children slots, unlike other slots, do not
require the `override:` element. It will be there, as a content sized
container element. This is important to keep in mind if an exact element
hierarchy is required for your layout to work.

```xml
<Contents>
    Those two render the same:
    <ElementB>This works fine</ElementB>
    <ElementB>
        <override:Children>
            This works fine
        </override:Children>
    </ElementB>
</Contents>
```

Slots can be styled like any other element too. They are regular elements otherwise.

Some built in elements like `Repeat` or the container elements `Div` or `Group` or
any text element accept children but do not expose children slots.

#### Forwarding slots

When composing elements of others that define slots you may want to expose those
slots. This is being done with slot forwarding:

ElementC:
```xml
<Contents>
    <ElementA>
        <forward:title>Element C's default</forward:title>
    </ElementA>
</Contents>
```

ElementD:
```xml
<Contents>
    <ElementC>
        <override:title>I override the forwarded slot!</override:title>
    </ElementC>
</Contents>
```