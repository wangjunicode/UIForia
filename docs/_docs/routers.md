---
id: Router
title: Router
layout: page
showInMain: true
order: 50
tags:
  - uiforia
---

# Routers
## What is a Router in UIForia?
Simply put, a router is a mechanism to enable or disable elements based on matching paths.
The basic mechanic of a router is to switch between routes, like a `switch case` statement
in your favourite language, C#.

```
swtich (route) {
    case "/start": return indexRoute;
    case "/options": return optionsRoute;
}
```

In UIForia templates you don't write `switch case` statements of course. This is how you 
would define a router and a `start` and `options` route for your game:

```xml
<Div x-router="navigation" x-defaultRoute="/start">
    <StartScreen x-route="/start" />
    <OptionsScreen x-route="/options" />
</Div>
```

The router is always initialized on some parent of your routes. Routes don't necessarily have to 
be direct children of the router, they can be deeply nested as well.
In this example we chose the `Div` container to become our router element, but it could be any
of your custom elements too. The attribute `x-router` defines the name of the router, which must
be unique in the application. `x-defaultRoute` defines the route that should be active by default
meaning that the element of this route will be enabled when the router is created.
Note that these are `x-` prefixed attributes and take no dynamic expressions, just string values.

Let's assume `<StartScreen>` and `<OptionsScreen>` are custom elements that create screens according
to their names. The `x-route` attribute defines their route name. Without this attribute these
elements would just be regular elements, but since we added `x-route` they will be known to their
parent router. Parent as in the next router up in the hierarchy, which just happens to be their
actual parent element.

### Recap
Router:
 - can be any element
 - must have `x-router` attribute
 - must have `x-defaultRoute` attribute

Route:
 - can be any element
 - must have `x-route` attribute

Using slashes like in URLs is not a must, it's merely a recommendation since more complex router 
setups will probably be more maintainable like this. Especially if we look at the next big 
feature of routes: *parameters*
 
## Routes with parameters
Sometimes you'll want one of your elements to behave differently for slightly different routes where
parts of the route refer to the content you want to display, like a details page for items. The data
might be coming from a CMS or other static source but the structure is always the same. 

```xml
<Div x-router="details">
    <ItemDetails x-route="/item/:itemId" />
</Div>
```

```c#
[Template("ItemDetails")]
public class ItemDetails : UIElement {

    public Item item;

    private Router router;

    public override void OnCreate() {
        router = Application.RoutingSystem.FindRouter("details");
    }

    public override void OnUpdate() {
        string itemId = router.GetParameter("itemId");
        item = ItemSource.GetById(itemId);
    }
}
```

In the example above we created a new router with the name `details`. The `ItemDisplay` element is the 
only route for this router and its purpose is to display all the information of an `Item`. We omitted
the template for `ItemDisplay` but imagine it would display a lot of useful information.

Notice the syntax for defining a `route parameter`: `x-route="/item/:itemId"`. The colon denotes the 
parameter name, not the actual value, so in the actual element class you can go ahead and get the 
current route's parameter called `itemId` like this:
`Application.RoutingSystem.FindRouter("details").GetParameter("itemId")`.

Since finding a router every frame (we need to read the route parameter in the OnUpdate method!) is 
slightly wasteful and we know the router will never change, we can keep a reference instead and initialize
in the `OnCreate` method.

But how does one link to an item? Here's one possibility:

```c#
public class Anchor : UIContainerElement {
    private string to;
    private string router;
    
    [OnMouseClick]
    public void OnClick() {
        Application.RoutingSystem.FindRouter(router).GoTo(to);
    }
}
```

This example of a simple anchor will find any router by name and go to the desired location when the user
[clicks on it](/docs/input). Use it like this: 

```xml
<Anchor to="'/items/radio'" router="'details'">Radio</Anchor>
```

The `:itemId` has been replaced by the actual id of the item, which in this case is just `radio`.
`Router.GetParameter` will always return a string, but it should be quite easy to write a URL 
parameter mapper on top of that.
