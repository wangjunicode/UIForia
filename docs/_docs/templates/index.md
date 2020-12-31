---
id: TemplateConcepts
showInMain: true
title: Template Concepts and common terms explained
tags: 
 - template
layout: page
order: 9
---

# Templates

UIForia has a very sophisticated template engine that takes in an
XML-like template syntax and converts it to IL code that is executed.
There are two ways this can happen, either at application start up time
or as a build step. When running in dynamic mode, all the templates are
parsed and IL is generated for them (assuming there weren't any
compilation errors)

When precompiling we generate real C# code. This means we can fully support any AOT platforms and even run in Unity's il2cpp mode.  

Performance between these two modes should be relatively similar between
the two modes, though precompiling might be slightly faster. Typically
in development you would use dynamic mode because it allows you to
hot-reload the entire UI when you change a template or style file. The
exception to this rule is debugging. If you run into an issue with your
templates that you'd like to debug, it may be easier to generate the
code and then set a breakpoint in the generated code.

Ok, on to the fun stuff!

## Exploring Template Syntax

A template is just an xml like file that describes the structure of your
UIForia UI. UIForia is entirely template based and does not support
manual hierarchy manipulation from code. This is completely by design
and while it may feel limiting when you first read this, it is actually
a huge benefit because of another awesome feature: Data Binding. 

Here is a quick sample of a template that we can dissect, its is a
theoretical list of three inventory items.

```
<!-- This is a comment! -->

<!-- All templates begin with a <UITemplate> root element. You can only have one of these per template file and it must be at the root -->
<UITemplate>
    
    <!-- we want to include an external style sheet-->
    <Style path="path/to/file.style"/>
    
    <!-- <Contents> is where our actual elements are, it is a reference to the backing class of the template so any attributes -->
    <!-- you add here will be included if you use this template in other places, more on this later! -->
    <Contents style="root">
        
         <!-- we define three groups of elements, each composed of a container that wraps an image and a piece of text -->
        <Group style="inventory-item-group">
             
            <!--    The <Image> element lets us display an image. Here we pass the location of our texture as a string to the `src` attribute. -->
            <!--    This is called Data Binding and we'll talk a lot more about this later -->
            <Image src="'icons/inventory_items_0'"/>
            <Text> Inventory Item 0 </Text>
        </Group>
        
        <Group style="inventory-item-group">
            <Image src="'icons/inventory_items_1'"/>
            <Text> Inventory Item 1 </Text>
        </Group>
        
         <Group style="inventory-item-group">
            <Image src="'icons/inventory_items_2'"/>
            <Text> Inventory Item 2 </Text>
        </Group>
        
    </Contents>
    
```

Ok cool, so we can display three inventory items with fixed text and a
hard coded image path. That kinda sucks and I wouldn't want to build a
game that way. 


## Data Binding

Any field, property, method, event, or value you use in data binding
expressions will need to be public. The reason for this is that when the
code is pre-compiled to C#, it will not compile if you use non
accessible values. 

In general you want to keep the data flow of
your UI in a top down direction. There are use cases for two way binding
on some elements like `<Input>` or `<Select>` but in general you should
strive to pass data down to children and not up to parents. 

### Fields and Properties

The majority of data binding you will use in UIForia templates will be
field or property bindings. 

```
[Template("path/to/template.xml")]
public class MyElement : UIElement {
    public float floatValue;
}

<!-- app.xml -->
<UITemplate>
    <Contents>
        <MyElement floatValue="5f"/>
    </Contents>
</UITemplate>
```

### Attributes

Attributes come in two flavors: static and dynamic. Static attributes
are set once and only updated from code. Dynamic attributes are
evaluated every frame based on some expression that you provide. All
attributes must be of type String, but we are smart about doing
conversions so in many cases you can provide a piece of data that is not
string typed and the expression compiler will convert it to string for
you. 

Here is how you define a static attribute.

```
<Element attr:someAttribute="this-is-my-value"/>
```
Note the `attr` prefix. If this is missing the compiler will assume you
are trying to bind a field or property and might throw errors in your
face if that field or property doesn't exist in your backing class.

Static attributes are always treated as string values and do not need to
be wrapped in a string specifier (single quotes)

Here is how you define a dynamic attribute

```
<!-- Fun fact: the compiler can optimize away the string concat for you! -->
<Element attr:someAttribute="{'some-' + valueBinding}"/>
```

Note we still use the `attr` prefix, but we now use curly braces to tell
the compiler to treat this attribute as a dynamic value that is
computed. The value in the curly braces is evaluated every frame and
assigned to the `someAttribute` attribute. The value can be any
expression that returns a string or can be coerced to something that
returns a string. 

One performance note here: UIForia is very very smart about string
concatentation in expressions and will probably not use `string.Concat`
in the generated code. However, if you provide a type the compiler needs
to call `ToString()` on, this will be done every frame which will
allocate a lot. So try not to pass in values that need `ToString()`
called on them. In general, any primitive type like `int`, `bool`,
`float` etc will be converted to string in a non-allocating way. Other
types will just get `ToString()` called and WILL allocate. 

Here are some more examples of attribute magic

```
<Element 
    attr:attr0="{someField}"
    attr:attr1="{SomeProperty}"
    attr:attr2="{SomeProperty * 5 + 'cool!'}"
    attr:attr3="{GetAttrValue()}"
    attr:attr4="{position.ToString()}"
    attr:someAttribute="{'some-' + valueBinding}"/>
```

### Shared Styles

Shared styles can be applied in a number of ways, the most common ways
are through style bindings on the element. Like attribute bindings,
style bindings come in two flavors, static and dynamic. Again, static
styles are computed once and only updated via code while dynamic styles
are evaluated every frame. 

Here is an example of a static style usage. This would apply `style-one`
and `style-two` to our element. Styles are always applied in the order
they are defined, from left to right. 

```
<Element style="style-one style-two"/>
```

Here is an example of dynamically styling an element. In this case we
always apply the style `style-one` to the element and we also apply
`selected-style` if our `isSelected` bool evaluates to true. Note the
ternary here, if `isSelected` is false we don't want to apply any style,
so we pass an empty string to the ternary. 

Note that we use the string syntax (single quotes) inside our dynamic
style but not inside the static one!

```
<Element style="style-one {isSelected ? 'selected-style' : ''}"/>
```

Passing an empty string like that is ugly and annoying, so we can also
optionally omit the ternary colon and the empty string entirely. The
compiler sees a one sided ternary and assumes the type should be the
default of the left side expression, which in this particular case is an
empty string. 

This is totally equivalent to the above definition but is easier to
read. 

```
<Element style="style-one {isSelected ? 'selected-style'}"/>
```

Dynamic styles are evaluated every frame and if the resulting styles
changed between two frames, it will update the element. 

Unlike dynamic attributes, dynamic styles can accept more types than
just strings. You can also pass in expressions that return the following
types: `string`, `char[]`, `IList<string>`, `UIStyleGroupContainer`, or
`IList<UIStyleGroupContainer>`.

### Instance styles

Sometimes you just want to override a few style properties for an
individual element and not have to create a shared style definition.
That's where instance styles come in, which (surprise) can also be data
bound!

Here is an example where we set an element's background color to
something defined in a `myColor` field in our backing class.

```
<Element style:backgroundColor="myColor"/>
```

Any style property can be bound in this way. Currently we don't support
the style syntax for these properties, you'll have to use C#. For
example if you want to set the element's `PreferredWidth` property to
50% of its content size in an instance style binding you'll have to pass
in `new UIMeasurement(0.5, UIMeasurementUnit.Content)` value instead of
using the style syntax which would be `0.5cnt`. This will probably be
improved in a later release. 

Styles can also apply to specific states `hover`, `active`, `focused`
like this:  
```
<Element style:hover.backgroundColor="myColor"/>
```

Instance styles are updated every frame, so if you need to overwrite
them in code, it will be reset to it's expression result in the next
frame. The best practice for this is either to use a `once` binding or
just make sure you always compute your value in code and use the result
in your expression.

```
<!-- Instance style is set only once instead of every frame -->
<Element style.once:hover.backgroundColor="myColor"/>
```
