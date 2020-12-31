---
title: Data Binding
layout: page
tags:
 - binding
---

# Data Binding

Data bindings in UIForia let you connect any of your element's public properties to parameters
of elements in your templates. Bindings are run via [expression](#expressions). You can also
bind methods to event handlers or use more complex single line expressions to figure what value
to bin during runtime.

Bindings run every frame. If you picture your element hierarchy data bindings flow from top to
bottom. You can use bindings in many places such as [expression](#expressions) in the template,
parameter bindings, style bindings, attribute bindings or when defining template variables.

## Expressions

Please have a look at the [expression api here](/docs/expressions)

## Expressions in the template

Expressions in templates can occur in any child block of the `<Contents>` element. They will
be run every frame and their output will be a string, so don't mind calling `ToString()` as
that is the default conversion for any object.

Expressions in templates require a special syntax to run:

- they must be wrapped in curly braces
- they must consist of a single line 
- string literals must use the single quote instead of the double quote

```xml
<Contents>
    {myProperty}
    <Div>{IsThisTrue ? 'yes' : 'no'}</Div>
</Contents>
```

## Parameter Expressions 

An `<Inventory>` element might pass data to various children elements like this: 

Inventory.cs:
```c#
[Template]
public class Inventory : UIElement {
    public ItemData data;
}
```

Inventory.xml
```xml
<Contents>
    <Items value="data"/>
</Contents>
```

Note that parameter bindings don't require the expression to be wrapped in curly braces, they 
expect an expression by default.

## Synchronized bindings

Sometimes your child elements handle some form of data input and you need to react to changes. 
In that case you can opt-in to *synchronized bindings*, which adds the binding to a special 
write-back phase. A `<Form>` element might want to output whatever is in one of its `<Input>`
fields and/or send it to a database when a submit button is clicked. A sync binding to the 
Input's value parameter will write all changes back to the bound property.

```c#
[Template]
public class Form : UIElement {

    private UserService userService;

    public string name;

    public void Submit() {
        userService.SaveName(name);
    }
}
```

```xml
<Contents>
    <Input sync:value="name"/>
    <Button mouse:click="Submit()">Submit</Button>
</Contents>
```

Here we're using two bindings. First the sync binding that will update the Form's `name` property
if the input element changed its `value` parameter internally in any way.
The second binding is an [input event binding](/docs/input), which are documented separately. 
Note that we don't have to pass around any references. `name` will always be up-to-date when `Submit`
is being executed.

## Change Handlers

If synchronizing properties is not enough and action has to be taken for every changed value
we can use change handlers. You can register change handlers for every parameter like this:

`<MyElement myParam="data" onChange:myParam="MyChangeHandler($oldValue, $newValue)"/>`

```c#
[Template]
public class Form : UIElement {

    private UserService userService;

    public string name;
    
    public override void OnEnable() {
        name = userService.GetName();
    }

    public void OnChange(string oldValue, string newValue) {
        userService.SaveName(newValue);
    }
}
```

```xml
<Contents>
    <Input value="name" onChange:value="OnChange($oldValue, $newValue)"/>
</Contents>
```

This Form element doesn't need submit buttons, it saves all changes immediately. With the optional 
`$oldValue` and `$newValue` parameters you can do all kinds of validation before actually saving
the update of course. Note that we didn't need a sync binding, as we can get the updated value
from the `$newValue` context variable.

## Attribute Bindings

Unlike the other bindings attributes always expect string literals to be the input.

```xml
<MyElement attr:id="root" />
```

The attribute "id" will be the string `root`. If you want dynamic attribute bindings you need to 
add an expression with curly braces.

```xml
<MyElement attr:id="{ GetId() }" />
```

Now that's an expression!

## Style bindings

As you learned in the [style guide](/docs/style) you can add one or many styles to an element:

```xml
<MyElement style="one two three"/>
```

But dynamically adding styles works too:

```xml
<MyElement style="one two three {IsThisTrue ? 'four'}"/>
```

Note: you can leave out the else-part of the ternary!

### Style Property Bindings

Any [style property](/docs/style/style-properties) (not the short-hand properties like `AlignX` though)
can be set via bindings too ():

```xml
<MyElement style:backgroundColor="GetColor()"/>
```

Note: Those instance bindings will win over properties set in style sheets!