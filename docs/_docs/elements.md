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

A very simple button element might not need its own template for example.

```c#
public class Button : UIContainerElement {}
```

It's not doing anything on its own but you can start to use the element name for your 
app's theme and create an [element style group](/docs/style/#create-an-element-style):

```
style <Button> {
    Border = 1px;
    BorderRadius = 3px;
    BorderColor = black;
    [hover] {
        BackgroundColor = rgba(200, 240, 200, 128);
    } 
}
```

## Repeat
This one emulates a for-each loop. The `<Repeat>` element takes one required and a couple
of optional parameters:

| Parameter   | Required | Type                | Description                                                                                                                                             |
|-------------|----------|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------|
| list        | (x)      | `RepeatableList<T>` | Data data that you want to iterate                                                                                                                      |
| as          |          | `string`            |  Change the variable alias of the current iteration item from the default `item` to something else. The alias can then be   referred to as `$yourItem`. |
| lengthAlias |          | `string`            |  Similar to `as` the `lengthAlias` changes the variable alias that refers to the length of the list.                                                    |
| indexAs     |          | `string`            |  Same as above, `indexAs` gives the index variable a new name.                                                                                          |

``` xml
<Repeat list="data.entries">
    <Div>{$item.name}  is the value at index {$index} / {$length}</Div>
</Repeat>
```

### RepeatableList
As of right now you have to wrap your data in a `RepeatableList<T>` to use the `Repeat` element.
Items added to RepeatableList will result in new elements being created, removing items will destroy the elements.
Right now you have to keep that fact in mind when using `<Repeat>`. Rather than removing and adding items
to the list, which will cause some significant garbage when done every frame, you could instead use 
`RepeatableList.Upsert` to change the data at an index. Since bindings run every frame you will see your
data being updated. But instead of deleting and recreating a `UIElement` for the changed value UIForia 
will just update its bound values.

```
[Template("MyElement.xml")]
public class MyElement : UIElement {

    public RepeatableList<string> currentlyActiveUserNames;
    
    public override void OnCreate() {
        currentlyActiveUserNames = new [] { "foo", "bar" };
    }
    
    public override void OnUpdate() {
        List<string> users = ActiveUserSystem.GetActiveUsers();
        currentlyActiveUserNames.ReplaceList(users);       
    }
}
```

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

