---
layout: page
title: Getting Started
description: How to set up a simple UI
showInMain: true
order: 0
---

# Getting Started
## Install UIForia into your project
That's as easy as copying the following files into your `Packages` directory:
- `/Packages/UIForia/Editor`
- `/Packages/UIForia/Src`
- `/Packages/UIForia/Resources`
- `/Packages/UIForia/package.json`

## Configure UIForia
Once you've installed the package let Unity recompile and this menu should appear in Unity's menu bar:

![uiforia-menu](/assets/img/getting-started-uiforia-menu.png)

### The UIForia menu
#### `Create UIForia Settings and Materials`
Click on the first menu item that says `Create UIForia Settings and Materials`. You'll find a couple of new
files in your `/Resources` directory.

![uiforia-generated-files](/assets/img/getting-started-resources.png)

Make sure everything is wired up correctly by clicking on the `UIForiaSettings` file. You should see this
in the inspector:

![uiforia-settings](/assets/img/getting-started-settings.png)

Just for some closure we'll quickly explain what the other options are:
#### "Build Templates"
Copies (soon compiles) your templates into the `/StreamingAssets` directory for [production builds](/docs/production-build).

#### "Refresh UI Templates"
Maybe one the major USPs of UIForia. Change any [template](/docs/templates) or [style](/docs/style) during play mode, 
hit ctrl+g on your keyboard (or this menu item) and the whole UI rebuilds itself.

### UIForiaSettings

Apart from the automatic material setup there are currently two things you can change in the settings yourself.
1. `Load Templates From Streaming Assets` (default: off) \
You need this to test your [production build](/docs/production-build) on your local machine. 

2. You can add namespaces \
This will be explained in more detail in the [Template Guide](/docs/templates) but to give a short answer:
sometimes you'll want to write an [expression](/docs/expressions) that makes use of one of your custom class or enum 
types like `<StatusDisplay value="Status.Completed" />` where `Status` is an enum in your project's namespace.
Normally you'd have to add a `<Using namespace="This.Is.A.Namespace" />` to your template but with this setting
you can just import a namespace by default for all templates. It's your responsibility not to generate name clashes!

## Check out the demo app
If you clone the [UIForia repo](https://github.com/klanggames/UIForia) you can simply open it in Unity, load the
Documentation scene from `/Assets/Documentation/Scenes/DocumentationScene` and hit play. It's a very basic showcase
of features that you can take as a basis for creating your own UI. Throughout the guide we'll referer to the 
documentation app a couple of times so make sure you've got it running to follow the examples.

## Add UI to your application
Now that you have UIForia in your project  and maybe had a peek at the demo app, let's continue our journey and get
some UI into your app. First thing we'll have to do is to create an instance of `UIForia.GameApplication`.

Let's look at the `GameApplication`'s `Create` method for a second:

```C#
    GameApplication Create<T>(string applicationId, 
                              Camera camera, 
                              string templateRootPath = null, 
                              Action<Application> onBootstrap = null
    ) where T : UIElement
```

`applicationId`: just pick any name for your application. Comes in handy later, when debugging. Or maybe you
want multiple UIs at the same time which are completely independent from each other (3D, VR, split screen etc.)? 
Has to be unique, that's all. Well, and fun.

`camera`: we need some camera to render to, please create one yourself and pass it in here.
Some useful configuration that might speak for itself:
- Clear flags: Depth
- Depth: something positive!
- Clipping Planes Near: very low, e.g. 0.3
- Clipping Planes Far: very big: e.g. 70000

Otherwise you might have problems seeing any UI at all.

`templateRootPath`: it's optional, but can save you some typing. Some projects have really deep directory structures
and you do have to type all the paths to templates and style sheets relative to your `Assets` directory. 
E.g. if your UI lives in `/Assets/MyApp/Src/Client/UI/` you would pass in 
`/MyApp/Src/Client/UI/` as your template root path and all of your references in code and templates would be 
relative to that directory. If you set up your paths wrong you'll get an error:
 `FileNotFoundException: Could not find file "...\Assets\...YourFile.xml"`

`onBootstrap`: this callback conveniently fires when the `GameApplication` is properly set up but before elements
have been created. Useful if you want to integrate [Zenject](/docs/misc#zenject) for example.

`T : UIElement`: UIForia requires you to provide one root element per [view](/docs/layout#views) and that very first
view is free with this call. For the sake of this tutorial create a `MyAppRoot.cs`, `MyAppRoot.xml` and `MyAppRoot.style`
in your template root directory `/Assets/MyApp/Src/Client/UI/`.

So, with all that knowledge let's craft our first UIForia app:
```C#

public class UIViewBehavior : MonoBehaviour { 
    
    private GameApplication app;

    public UISystem() {
        app = GameApplication.Create<MyAppRoot>(
            "bananas",
             camera,
            "MyApp/Src/Client/UI"
        );
    }

    public void OnUpdate() {
        app.Update();
    }
}
```

Great, now fill that `MyAppRoot.cs` with some life:

```C#
public class MyAppRoot : UIElement {
    public string safeWord = "bananas";
}
```

Open your `MyAppRoot.xml` and add this:

```xml
<UITemplate>

    <Style path="MyAppRoot.style" />
    
    <Contents>
        The safe word is {safeWord}!
    </Contents>
</UITemplate> 
```

That's it, you can ship this or [continue with the simple app setup in the tutorial section](/docs/tutorial/simple-app).
