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
        public static string simpleBinding;
        
        public RepeatableList<int> numbers = new RepeatableList<int>() {1, 2, 3, 4, 5, 6, 7};

        public CustomInputData MyVal;

        public float num;

        public Action<float> CustomOnNumChange = n =>
                        Debug.Log($"{n} is the new value, custom action got called!!");

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