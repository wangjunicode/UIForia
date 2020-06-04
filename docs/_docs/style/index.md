---
title: Style Sheets
showInMain: true
description: Learn how to write style sheets
layout: page
order: 10
tags:
  - uiforia
  - style
  - syntax
---

# Style Sheets




## Syntax

Keyword           | Description
----------------- |:------------------------------------------------------                                    
`export`          | Used in conjunction with `const`; exported constants can be imported by other scripts.
`const`           | Defines a constant that can be referenced throughout the style file.
`import`          | Imports constants or style definitions for other style files.
`as`              | Renames a constant in the local context. More details in the section about imports.
`from`            | Specify the style file location that should be imported. More details in the section about imports.
`style`           | Creates a style definition.
`[attr:attrName]` | Used inside a style group to create an attribute style group. See the attribute section below for more.
`not` / `and`     | Used in conjunction with attribute style definitions. See the attribute section below for more.

### Syntax Highlighting
There aren't any IDE plugins yet, but we created a Rider file type configuration file that gives you at
least some syntax highlighting and autocomplete for all the keywords, although, of course, no syntax checking.

[Download the file here](/assets/settings.zip) and use Rider's import settings feature via `File > Import Settings`. 
The `.style` extension should be mapped accordingly after a restart.

## Create an element style
  
```
Style <Group> {

}
```

## Export theme constants
  
```
const redVal = 200;
export const color0 = rgba(@redVal, 0, 0, 1);
export const baseDirection = Vertical;
```

## Reference UIForia Enums
`const anotherDirection = Horizontal;`


## Import Exported Constants from other Style Files
`import vars as Constants from "file.style";`

## Create a style definition that can be assigned in the template via the style attribute
```
style MattsStyleSheet {

   // styles not applied in a block implicitly belong to the 'default' group
   // states applies in a block not wrapped in a group block belong to the 'default' group

   // example:
   // [hover] {}

   [attr:attrName] {
       [state-name-here] {
           BackgroundColor = @Constants.color0;
       }
   }

   [attr:attrName="value"] {}

   [attr:attrName="value"] and [attr:other] {}

   not [attr:attrName] {}

   not [$siblingIndex % 3 == 0] {
       MarginTop: @vars.margin1;
   }

   not [$siblingIndex > 2] {
       @use styleNameHere;
   }

   [state-name-here] {
       // block of styles
       StyleName = StyleValue;
   }

}
```




## Style by Attributes
 
So you want to add a special style for your element based on its attributes?
 
```<Input attr:disabled="true" text="'some text'" style="button"/>```
  
Be aware of UIForia's distinction between attributes and properties. To set an attribute you have to use the
`x-` notation. 

To add a stylable `disabled` attribute you have to write `x-disabled` and assign your value.  

UIForia attributes always expect plain strings as values in contrast to properties, which expect expressions (hence 
the double quoting when passing strings).  

After defining an attribute in your element, go to your style definition and add an attribute style group like this:
```
style button {
    BackgroundColor = rgba(240, 240, 240, 255);             // almost white if not disabled
    
    [attr:disabled="true"] {
        BackgroundColor = rgba(190, 190, 190, 255);         // a bit grey if the disabled attribute has the string value "true"
        
        [hover] {
            BackgroundColor = rgba(100, 100, 100, 255);     // hover the disabled attribute and it becomes very dark
        }
    }
}
```


## Colors
The colors are defined by CSS color values.
  
[Full list of colors supported by UIForia](docs/misc#list-of-all-supported-colors)
  

**Color Names**  
You can declare the name of the color. For example, `Color = blue` or `Color = red`


**RGB**  
UIForia supports RGB color values. The parameters (red, green, blue) specify the intensity of color as an integer between 0 and 255.

**RGBA**  
RGBA extends upon RGB by adding an alpha channel, which sets the opacity.
RGBA(red, green, blue, alpha)

**Hex Code**  
The CSS 6-digit hex color notation specifies RGB colors using hexadecimal values. 



