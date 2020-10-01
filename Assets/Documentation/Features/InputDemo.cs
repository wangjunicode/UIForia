using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

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

        public List<string> autocompleteList;

        public override void OnCreate() {
            autocompleteList = new List<string>();
            Autocomplete(default);
        }

        public void Autocomplete(KeyboardInputEvent evt) {
            autocompleteList.Clear();

            if (rwValue == null) {
                rwValue = "";
            }
            
            foreach (var val in VALUES) {
                if (val.StartsWith(rwValue.ToLower())) {
                    autocompleteList.Add(val);
                }
            }
        }
    }
    
}
