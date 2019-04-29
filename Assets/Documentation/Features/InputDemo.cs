using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation {
    
    [Template("Documentation/Features/InputDemo")]
    public class InputDemo : UIElement {
        
        public string textValue = "my string to read";

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

        public string rwValue;

        public string regularValue;

        public RepeatableList<string> autocompleteList;

        public override void OnCreate() {
            autocompleteList = new RepeatableList<string>();
            Autocomplete();
        }

        public void Autocomplete() {
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
