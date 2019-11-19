---
title: Production Build Setup
showInMain: true
tags: 
 - build
layout: page
---

# Production Build Setup

In order to build and bundle your UIForia application you need to copy all templates and styles into 
the `StreamingAssets` directory. You can do that manually by clicking the menu item `Build Templates` in
Unity's UIForia menu item or programmatically by executing `UIForia.Builder.BuildTemplates()` from your
build script. To test your application with the copied templates go to your
 [UIForiaSettings](/docs/getting-started#uiforiasettings) file and activate `Load Templates From Streaming Assets`.
 
 