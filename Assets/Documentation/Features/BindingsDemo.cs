using System;
using System.Globalization;
using Documentation.DocumentationElements;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace Documentation.Features {

    public struct Thing {
        public string ThingValue;
    }

    [Template("Documentation/Features/BindingsDemo.xml")]
    public class BindingsDemo : UIElement {
        
        public string simpleBinding = "this is a simple binding";
        
        public RepeatableList<int> numbers = new RepeatableList<int>() {1, 2, 3, 4, 5, 6, 7};

        public CustomInputData MyVal;

        public float num;

        public Action<float> CustomOnNumChange = n =>
                        Debug.Log($"{n} is the new value, custom action got called!!");
       
        [OnPropertyChanged(nameof(MyVal))]
        public void OnMyValChanged(string propertyName) {
            Debug.Log($"BindingsDemo: {propertyName} changed");
            //onValueChanged?.Invoke(Value);
        }
        
        [OnPropertyChanged(nameof(num))]
        public void OnNumValueChanged(string propertyName) {
            Debug.Log($"BindingsDemo: {propertyName} changed");
            // invoking the custom event handler trigger code in BindingsDemo.cs
            //onNumChanged?.Invoke(num);
            // Doesn't do anything obvious. Should be related to two-way-binding. todo: figure that out
        }
        public override void OnCreate() {
            num = 1f;
            MyVal = new CustomInputData();
        }

        public string GetDynamicStyle() {
            return "dynamic-style";
        }

        public Thing GetThing() {
            return new Thing {
                ThingValue = "Current Time: " + DateTime.Now.ToString(CultureInfo.InvariantCulture)
            };
        }

        public void OnGreenClick(MouseInputEvent evt) {
            evt.StopPropagation();
            MyVal = new CustomInputData() {
                    MyVal = evt.MousePosition.y
            };
            num = evt.MousePosition.y;
        }
    }

}