using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace Documentation {
    
    [Template("Features/InputDemo.xml")]
    public class InputDemo : UIElement {
        
        public string tv = "my string to read my string to read my string to read my string to read my string to read";

        public static readonly string[] VALUES = {
            "anchor",
            "label",
            "line-height",
            "table",
            "tbody",
            "div",
            "dropdown",
            "group",
            "h1",
            "h2",
            "h3",
            "h4",
            "input",
        };

        public float floatValue = 1115.52f;
        public double doubleValue = 1234567.8901d;
        public string rwValue;

        public string regularValue;
        public Action<FocusEvent> FocusedMe => (evt) => Debug.Log("Focused");

        public RepeatableList<string> autocompleteList;

        public override void OnCreate() {
            autocompleteList = new RepeatableList<string>();
            Autocomplete(default);
        }

        // public override void HandleUIEvent(UIEvent evt) {
        //     if (evt is SubmitEvent && autocompleteList.Count > 0) {
        //         rwValue = autocompleteList[0];
        //         evt.StopPropagation();
        //     }
        // }

        public void Autocomplete(KeyboardInputEvent evt) {
            autocompleteList.Clear();

            if (rwValue == null) {
                rwValue = "";
            }
            
            foreach (string val in VALUES) {
                if (val.StartsWith(rwValue.ToLower())) {
                    autocompleteList.Add(val);
                }
            }
        }
    }
    
}
