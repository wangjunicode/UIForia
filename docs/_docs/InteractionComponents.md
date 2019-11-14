---
id: InteractionComponents
title: Interaction Components 
layout: page

---

This section documents components in UIForia for interaction. 

List of [input event bindings](Expressions.md/#input-event-bindings)

### Button
A Button has an onMouseClick event binding with its associated method call.  

** 1. Create a method in your C# script **

```
public void OnButtonClick() 
{
    //do something
}
```
 
** 2. Call the method in your XML element via a property  **  
`<Div style="button" onMouseClick="OnButtonClick()">`


<br/>

-------------------------

### Toggle
A Toggle contains a checkbox for checking or unchecking an option. 

onValueChanged invokes when the Toggle is clicked.
```
[WriteBinding(nameof(isChecked))]
        public event Action<bool> onValueChanged;

        public Color checkedColor = Color.red;
        public Color uncheckedColor = Color.clear;
        
        [OnMouseClick]
        public void Toggle() {
            isChecked = !isChecked;
            onValueChanged?.Invoke(isChecked);
        }
```
<br/>

-------------------------

### Scrollbar
`<ScrollView>` element in XML  

### Dropdown

```    
        public class SelectOption<T> : ISelectOption<T> {

            public string Label { get; set; }
            public T Value { get; set; }

            public SelectOption(string label, T value) {
                this.Label = label;
                this.Value = value;
            }

        }
        
        public RepeatableList<ISelectOption<string>>[] translations;
        
        public override void OnCreate() {
  
            translations = new RepeatableList<ISelectOption<string>>[] {
                    new RepeatableList<ISelectOption<string>>() {
                            new SelectOption<string>("Hello", "en"),
                            new SelectOption<string>("Hallo", "de")
                    }         
        }
```        
                  
<br/>

-------------------------

### Input Field
`<Input>` element in your XML
<br/>  

Parameter        | Modifier         |Description 
-----------------|-----------------|---------                                    
 value           | read(default)   | `read` on its own will not make much sense. The input field is going to read the value every frame from the property, thus overwriting any change immediately with the start value.`read` really is only useful for display purposes, e.g. if you use disabled.           
 value           | read.write sets | `read.write` reads the property and puts its value into the input field and will write the value back to the bound property on every change. No additional change handler necessary. Use this if you have default values or want to change the input`s value from outside events.
 value           | write           |  `write` will only write the value back to the bound property.  
 
 
 

  
    
```
 Input value.read.write="tv" autofocus="true"/>
         <Input value.read.write="tv"/>
         <Text style="text mb20">{tv}</Text>
         
         <Label forElement="'input1'">Some label</Label>
         <Input x-id="input1" value.read.write="rwValue" onKeyUp.late="Autocomplete($event)" placeholder="'Some value please'" />
 
         <Label forElement="'input2'">Another label</Label>
         <Input x-id="input2" value.write="regularValue" />
 
         <Label>Float value Input, value is {floatValue}. Also demonstrates the MaxLength property.</Label>
         
         <InputElement--float value.read.write="floatValue" MaxLength="8"/>         
```

<br/>

-------------------------

### Modal
Create a modal or any window that opens with an event binding:

** 1. In your C# script: **
```
// Create a list to add your view and window element
 public List<UIView> windowName = new List<UIView>(); 
 
 public void OnMouseHover() {
    // set the screen position of the window
    Vector2 position = layoutResult.screenPosition; 
    
    // create the view and window elements
    UIView view = Application.CreateView("Item Stats", new Rect(position.x, position.y, 300, 300)); 
    NameOfComponent window = Application.CreateElement<NameOfComponent>(); 
    
    // add the view and window to your List
    windowName.Add(view); 
    view.AddChild(window); 
}
```
** 2. Call the method in your XML element via a property  **  
`<Div style="button" onMouseHover="OnMouseHover()">`


   

