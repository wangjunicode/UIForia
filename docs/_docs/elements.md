---
title: Elements
description: Built-in Elements
layout: page
tags:
 - InputElement
 - Select
 - ScrollView
 - Repeat
 - Dynamic
---

# Built-in Elements



## Text

## Containers 

## Repeat

``` xml
<Repeat list="data.entries">
    $item.name
    $index
</Repeat>
```

### RepeatableList

As of right now you have to wrap your data in a `RepeatableList<T>` to use the `Repeat` element.


## InputElement

``` xml
<Input value.read.write="name" placeholder="'Enter your name'" />
```

is a shorthand for the generic version:

``` xml
<InputElement--string value.read.write="name" placeholder="'Enter your name'" />
```


``` xml
<InputElement--int value.read.write="age" placeholder="'Enter your age'" />
```

## Select

## ScrollView

## Dynamic

