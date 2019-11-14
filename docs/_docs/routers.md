---
id: Router
title: Router
layout: page
---

The router connects your UI with its location.

```
<?xml version="1.0" encoding="utf-8"?>
<UITemplate>
    <Style src="Documentation/Router.style"/>
    <Contents style="root-container">

            <Div style="navigation">
                <Anchor routerName="'demo'" href="'/index'">Home</Anchor>
                <Anchor routerName="'demo'" href="'/AnimationDemo'">Animation</Anchor>
            </Div>
            
            <ScrollView style="demo-panel" x-router="demo" x-defaultRoute="/index">
                <Group x-route="/index"/> 

                <AnimationDemo x-route="/AnimationDemo"/>
            </ScrollView>

    </Contents>

</UITemplate>
```

`Anchor` links to the desired C# script, with the property `routerName`

`AnimationDemo` is the name of the component we are linking, with its `x-route` file path