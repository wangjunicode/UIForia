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
| list        | (x)      | `RepeatableList<T>` | Data that you want to iterate                                                                                                                      |
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
The InputElement supports generic typed values. It's one of the basic form elements (forms is an upcoming feature, stay tuned).

| Parameter      | Required | Type                                   | Description                                                                                      |
|:---------------|:---------|:---------------------------------------|:-------------------------------------------------------------------------------------------------|
| value          | (x)      | T                                      | That's the value that is being displayed in the input field                                      |
| placeholder    |          | string                                 | A value that should be displayed if the `value` is empty, a placeholder.                         |
| autofocus      |          | bool                                   | Set this to true and the input element tries to grab the [focus](/docs/input#ifocusable)         |
| caretBlinkRate |          | float                                  | (experimental) change how quickly the caret blinks... or maybe you don't                         |
| MaxLength      |          | int                                    | Limit the number of characters that can be typed                                                 |
| formatter      |          | UIForia.Elements.IInputFormatter       | The default formatters support formatting float, double and int.                                 |
| deserializer   |          | UIForia.Elements.IInputDeserializer<T> | The default deserializer will output strings as-is, and parse int, float, and double.            |
| serializer     |          | UIForia.Elements.IInputSerializer<T>   | Used ToString by default to serialize.                                                           |
| onValueChanged |          | event Action<T>                        | Subscribe to this event if your wrapping element needs to do things when the input value changed |

Custom formatting / serialization might be necessary if you have number fields with very unique needs.
The defaults will work well for most basic cases but might not be what you want if you're trying to display
numbers with a super high precision.

### Examples

``` xml
<Input value.read.write="name" placeholder="'Enter your name'" />
```

is a shorthand for the generic version:

``` xml
<InputElement--string value.read.write="name" placeholder="'Enter your name'" />
```

`value` is an `int`:
``` xml
<InputElement--int value.read.write="age" placeholder="'Enter your age'" />
```

## Select

| Parameter           | Required | Type                             | Supports Write Binding | Description                                                                                                                              |
|:--------------------|:---------|:---------------------------------|:-----------------------|:-----------------------------------------------------------------------------------------------------------------------------------------|
| selectedValue       |          | T                                | yes                    | Sets the selected value. The `selectedIndex` will be figured out automatically                                                           |
| onValueChanged      |          | `Action<T>`                      | *                      | Gets executed when the select changes the selectedValue.                                                                                 |
| selectedIndex       |          | int                              | yes                    | You can bind to the selectedIndex in addition or as an alternative to seletedValue.                                                      |
| onIndexChanged      |          | `Action<int>`                    | *                      | Gets executed when the selected value changes and thus the selectedIndex with it.                                                        |
| defaultValue        |          | T                                |                        | Will be used if the `selectedValue` is null or unset.                                                                                    |
| options             | yes      | RepeatableList<ISelectOption<T>> |                        | The backing data for the select element.                                                                                                 |
| selectedElementIcon |          | string                           |                        | Defaults to `"icons/ui_icon_popover_checkmark@2x"`; it's the little icon that gets drawn next to the selected value in the options list. |
| disabled            |          | bool                             |                        | Disables the select element                                                                                                              |
| scrollSpeed         |          | float                            |                        | Default: 10                                                                                                                              |
| disableOverflowX    |          | bool                             |                        | Disables horizontal scrolling                                                                                                            |
| disableOverflowY    |          | bool                             |                        | Disables vertical scrolling                                                                                                              |

## Write Bindings
<span class="badge badge-info">A more detailed article around bindings will come later!</span>

As seen in `InputElement` and `Select` you can opt-in to write bindings for some of their parameters.
That allows for automatic write-back into your custom element's property. By default all property bindings
are read bindings. Element properties can be configured with binding options.

These two examples are the same for instance, since `.read` is implicitly added for every property binding:
``` xml
<InputElement--string value.read.write="name" placeholder="'Enter your name'" />
<InputElement--string value.read.write="name" placeholder.read="'Enter your name'" />
```

That roughly translates to: 
- read the value bound to the placeholder parameter value every frame and store the (static) value into the input's placeholder property.

For the value field it would be
- read the value of the property `name` from your backing class and store its value in the value parameter of the input element
- if the `value` changes due to some action inside of the input element write that change back to `name`  


## Custom Generic Types for InputElement or Select
 
When using custom generic types make sure you import it in your template `<Using namespace="your.name.space" />`:
``` xml
<UITemplate>
    <Using namespace="your.name.space" />
    <Using namespace="UIForia.Animation" />
    <Contents>
        <InputElement--CustomType value.read.write="myval" />
        <Select--AnimationDirection options="directions" selectedValue.read.write="direction" />
    </Contents>
</UITemplate>
```

## ScrollView
Use this element if you want to define a fixed size area that may have children occupying more than that space.
The ScrollView element must not be content sized, otherwise you'll never see any scroll bars. Basically any other size will work.

| Parameter        | Required | Type  | Description                                                                                                           |
|:-----------------|:---------|:------|:----------------------------------------------------------------------------------------------------------------------|
| scrollSpeed      |          | float | Default: 50                                                                                                           |
| fadeTime         |          | float | Default: 2 - the time it takes until the scroll handles start to fade out                                             |
| fadeTarget       |          | float | By default the scroll handles disappear. Set this parameter to a value between 0 and 1 to change their final opacity. |
| disableOverflowX |          | bool  | Disables horizontal scrolling                                                                                         |
| disableOverflowY |          | bool  | Disables vertical scrolling                                                                                           |

### ScrollView API
Use `FindBy` in your element to get a reference to the ScrollView, then you may use one of the following
methods to change the scroll position.

#### `ScrollElementIntoView(UIElement element)`
The passed in element must be a child of the ScrollView, of course. The resulting
scroll position will be so that the element's top or bottom edge aligns with the 
ScrollView's upper or lower edge, depending on the necessary scroll direction.

#### `ScrollToHorizontalPercent(float percentage)`
`percentage` must be a value between 0 and 1. 

#### `ScrollToVerticalPercent(float percentage)`
`percentage` must be a value between 0 and 1.
 
#### UIScrollEvent
ScrollViews also consume UIScrollEvents. When any child of a ScrollView raises
this event the scroll position will be changed to the event's `ScrollDestinationX`
or `ScrollDestinationY` properties. If any of them is smaller than 0 there
will be no scrolling in that axis.

To raise an event you'd do this:

```
public void ScrollUp(MouseInputEvent evt) {
    TriggerEvent(new UIScrollEvent(-1, 0));
}
```

## Dynamic

