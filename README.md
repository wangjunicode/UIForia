# UIForia User Guide
UIForia (/juːˈfɔːriə/) is a stylable template based UI system.  

## Box Model
Unlike CSS there is only one box model in UIForia, which is the equivalent to the CSS border-box model.
The content width of an element will be reduced by border and padding values. Defining a width of 100% and 
additionally a border and/or padding will not result in a box bigger than 100%. An element with a fixed 
width of 100px on the other hand will "eat up" border and padding, effectively shrinking the content box.

## Syntax

Keyword           | Description
----------------- |:------------------------------------------------------                                    
`export`          | Used in conjunction with `const`; exported constants can be imported by other scripts.
`const`           | Defines a constant that can be referenced throughout the style file.
`import`          | Imports constants or style definitions for other style files.
`as`              | Renames a constant in the local context. More details in the section about imports.
`from`            | Specify the style file location that should be imported. More details in the section about imports.
`style`           | Creates a style definition.
`[attr:attrName]` | Used inside a style definition to create an attribute sub-style-group. See the attribute section below for more.
`not` / `and`     | Used in conjunction with attribute style definitions. See the attribute section below for more.

### Example
```
// place single line comments everywhere

// export theme constants e.g.
const redVal = 200;
export const color0 = rgba(@redVal, 0, 0, 1);
export const baseDirection = Vertical;

// reference UIForia enums 
const anotherDirection = Horizontal;

// import exported constants from other style files
import vars as Constants from "file.style";

// create an element style which will be applied to all elements of that kind 
style <Group> {
    
}

// create a style definition that can be assigned in the template via the style attribute
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

audio xy {
    
}



<TagName> {
    [attr:other] {
        
    }
    
   
}

```


##Style by Attributes
So you want to add a special style for your element based on its attributes?
```<Input x-disabled="true" text="'some text'" style="button"/>```
Be aware of UIForia's distinction between attributes and properties. To set an attribute you have to use the
`x-` notation. To add a stylable `disabled` attribute you have to write `x-disabled` and assign your value.
UIForia attributes always expect plain strings as values in contrast to properties, which expect expressions (hence 
the double quoting when passing strings).

After defining an attribute in your element go to your style definition and add an attribute style group like this:
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

## Sizes:


