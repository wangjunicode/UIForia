---
id: Templates
title: Templates
layout: page
order: 0
---


## Writing UIForia templates

UIForia uses a template syntax loosely based on XML. Using this syntax you can define the hierarchical structure of your UI.  

Templates are made up of various sections, `<Style>`, `<Using>`, `<Contents>`. Let's dive into these one by one.
## `<Style>`
This is where you put the style definitions for this template. Styles can either be defined inline in the template file, or you can provide a path attribute that references a `.style` file that lives elsewhere.

```
    <UITemplate>
        <Style>
            style my-style {
                BackgroundColor = red;
            }
        </Style>
        
        <Style path="path/to/file.style"/>
        
        <Style path"path/to/other/file.style" as="util"/>
    </UITemplate>
```



<!--## Writing XML Templates-->
<!--Using XML markup, you can define the hierarchical structure of the user interface.-->

<!--The following code example shows the logical structure for a button, which triggers an onMouseClick to open a window:-->

<!--```-->
<!--<?xml version="1.0" encoding="utf-8"?> -->
<!--<UITemplate> -->

<!--    <Style path="UI/Window.style"/>-->

<!--    <Contents style="window-container">-->

<!--        <Group style="create-button-row">-->
<!--            <Div onMouseClick="OnButtonClick()" style="button">Create Window</Div>-->
<!--        </Group>-->
<!--        -->
<!--    </Contents>-->

<!--</UITemplate>-->
<!--```-->

<!--The first line is the XML declaration. -->

<!--`<UITemplate>` defines the document root-->

<!--`<Style path=" ">` assigns the path of the stylesheet, with all your styling properties to set the dimensions of the elements and how they are displayed on the screen.-->

<!--`<Contents style" ">` acts as the parent container for the entire file.-->

<!--`<Group style =" ">` assigns an element to be styled in Window.style-->

<!--`<Div style =" ">` is the child element, with a specified style, and an input event binding (onMouseClick). The method OnButtonClick() is declared in the C# file.-->


<!--Although you can style the elements within the XML file using a `<style>` tag, it is recommended that you use a separate stylesheet. -->


<!--x-id-->
<!--style-->
<!--eventmethod-->
<!--if-->

<!--`<Div if="IsInStartZone" style="dragme" onDragCreate="StartDrag($element)">Dragme</Div>`-->


<!--<br/>-->

<!------------------------------------------------------------------->

<!--### Reusing Components-->
<!--Designing large user interfaces is simple in UIForia. You can import other XML files by defining the class in a tag `<NameofClass>`-->

<!--For example, say that you have a checkbox UI element that has a square and a check-mark image. You can create an XML template file to reuse the checkbox UI element in other XML files. Perhaps you want to use checkbox.xml in your form.xml file: `<Checkbox>`-->

<!--#### Loading XML from C#-->
<!--The C# file must define the path of the .xml. This should be declared before your class.-->


<!--`[Template("UI/Checkbox.xml")]`   -->
<!--`public class AnimationDemo : UIElement`   -->
<!--`{}`-->

<!--Every class inherits from UIElement in order for the templates to be matched and use their respective elements.-->



<!--<br/>-->

<!------------------------------------------------------------------->


<!--#### Property Access-->
<!--Any property on an element can be accessed. -->
<!--````C#-->
<!--    <SomeElement someValue="someFieldOnTheType"/>-->
<!--    <SomeElement someValue="someArrayField[6]"/>-->
<!--    <SomeElement someValue="some.nested.array[6].some.other.property"/>-->
<!--    <SomeElement someValue="some.nested.array[6 + 1 * 2 / 5].some.other.property"/>-->
<!--````-->

<!--#### Method References-->
<!--Methods can also be referenced directly and their output used. They are special in that their arguments -->
<!--can take computed expressions as well as literal values-->
<!--````C#-->
<!--    <SomeElement someValue="myMethod(45, true, someField.nested.value + something)"/>-->
<!--````-->

<!--#### Aliases-->

<!--Various aliases can be made available to the expression engine. The most prominent use of these -->
<!--are within `<Repeat>` tags. Aliases are defined by the context in which the expression runs. Anything-->
<!--can be an alias, the syntax is simply $ + some identifier. Important to note is that aliases are -->
<!--type checked just like all expressions, so you won't be able to accidentally mis-use them-->


<!--````C#-->
<!--    <SomeElement intValue="$intAlias"/>-->
<!--````-->







<!--FlexItemSelfAlignment: sets alignment of specific flex item in the flex container-->







