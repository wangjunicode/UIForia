---
title: Miscellaneous
description: Useful things, cheat sheets and so on without their own category
layout: page
tags:
  - colors
---

# Miscellaneous
## Best Practices
Don't override OnUpdate etc. if you don't provide a body. UIForia is smart enough to omit OnUpdate calls if 
the method has no override but it's not smart enough to figure out there's no code to run.

## Third party library support
### Zenject
Here's a quick tutorial for getting Zenject support for all of your UI classes.
Remember when you first created your [GameApplication](/docs/getting-started#add-ui-to-your-application)?
Go back to that and add an `onBootstrap` callback to that setup if you haven't:

```C#
(Application application) => {
    application.onElementRegistered += (element) => {
        for (int i = 0; i < element.ChildCount; i++) {
            OnElementRegistered(element.GetChild(i));
        }

        if (!element.isBuiltIn) {
            diContainer.Inject(element);
        }
    };
}
```

Done. Use `[Inject]` in all your UI elements, but please don't use constructor injection, as UIForia's is
trying to optimize a lot and your constructors are definitely affected by that. You can use init method
injection though!

## List of all supported colors
{% include colors.html %}

## Types

