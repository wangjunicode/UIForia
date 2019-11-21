---
title: Elements
description: Built-in Elements
layout: page
tags:
 - InputElement
 - Select
 - ScrollView
 - Repeat
 - Dynamic
---

# Built-in Elements
UIForia comes with a few built-in elements.

## Text
There are a couple of ways to get your text on screen.

1. Plain text. Just write what you want where you want it. This will compile into a single
`<Text>` element that you'll be able to see in the [UIForia Inspector](/docs/tools/#uiforia-inspector).
 
```xml
<UITemplate>
    <Contents>
        Just type some text
    </Contents>
</UITemplate> 
```
2. The Explicit Text Element. Defining Text elements directly can be useful for a couple of
reasons. Since they are individual elements they are of course also treated as separate things
by the [layout type](/docs/layout). When being used in a grid they might be three different 
columns. You also have the advantage of adding styles to the text nodes.
```xml
<UITemplate>
    <Contents>
        <Text>What early tongue so sweet saluteth me?</Text>
        <Text style="bold-text">Young son, it argues a distemper'd head</Text>
        <Text>So soon to bid good morrow to thy bed:</Text>
    </Contents>
</UITemplate> 
```
3. There are couple of other aliases for text elements.
 - `<Heading1>`
 - `<Heading2>`
 - `<Heading3>`
 - `<Heading4>`
 - `<Heading5>`
 - `<Heading6>`
 - `<Label>`
 - `<Paragraph>`
 
 ```xml
<UITemplate>
    <Contents>
        <Heading1>Act 1</Heading1>
        <Heading2>Prologue</Heading2>
        <Paragraph>
            Two households, both alike in dignity,
            In fair Verona, where we lay our scene,
            From ancient grudge break to new mutiny [...]
        </Paragraph>
    </Contents>
</UITemplate> 
```

## Containers
Containers serve only structural purposes, i.e. for better readability you'd use 
different containers throughout your template. 
- `Div`
- `Group`
- `Panel`
- `Section`
- `Header`
- `Footer`

 ```xml
<UITemplate>
    <Contents>
        <Header>
            <Panel>
                <Group>
                    <Div>Menu Item 1</Div>
                    <Div>Menu Item 2</Div>
                    <Div>Menu Item 3</Div>
                </Group>
            </Panel>
        </Header>
        <Section>
            Section A
        </Section>
        <Section>
            Section B
        </Section>
        <Footer>
            Room for notes   
        </Footer>
    </Contents>
</UITemplate> 
```

All containers are equivalent in terms of behavior or default styles. They take no
parameters and simply add another layer to your element hierarchy.
You can of course make your own containers by inheriting from `UIContainerElement` 
instead of `UIElement`. Then you won't need any accompanying template. 

## Repeat

``` xml
<Repeat list="data.entries">
    $item.name
    $index
</Repeat>
```

### RepeatableList

As of right now you have to wrap your data in a `RepeatableList<T>` to use the `Repeat` element.


## InputElement

``` xml
<Input value.read.write="name" placeholder="'Enter your name'" />
```

is a shorthand for the generic version:

``` xml
<InputElement--string value.read.write="name" placeholder="'Enter your name'" />
```


``` xml
<InputElement--int value.read.write="age" placeholder="'Enter your age'" />
```

## Select

## ScrollView

## Dynamic

