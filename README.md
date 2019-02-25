# UIForia User Guide
UIForia (/juːˈfɔːriə/) is a stylable template based UI system.  

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
export const color0 = rgba(@redVal, 0, 0, 1); // semicolon is optional 
export const baseDirection = Vertical;

// reference UIForia enums 
const anotherDirection : LayoutDirection = LayoutDirection.Horizontal;

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

   @exportAs("name")
   [state-name-here] {
       // block of styles
       StyleName = StyleValue;
   }

}

@audio xy {
    
}



<TagName> {
    [attr:other] {
        
    }
    
   
}

```