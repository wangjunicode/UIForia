using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;

namespace Documentation {
    
    [Template("Documentation/Features/InputDemo")]
    public class InputDemo : UIElement {
        
        public string tv = "my string to read";

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

        public float floatValue;
        public string rwValue;

        public string regularValue;

        public RepeatableList<string> autocompleteList;

        public override void OnCreate() {
            autocompleteList = new RepeatableList<string>();
            Autocomplete(null);
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is SubmitEvent && autocompleteList.Count > 0) {
                rwValue = autocompleteList[0];
                evt.StopPropagation();
            }
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
