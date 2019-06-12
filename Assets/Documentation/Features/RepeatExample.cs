using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.Features {
    
    [Template("Documentation/Features/RepeatExample")]
    public class RepeatExample : UIElement {

        public RepeatableList<ValueTuple<string, string>> dictionary;

        public string GetAStyle() {
            return "a";
        }

        [OnMouseClick()]
        public void RemoveAll() {
            dictionary.Clear();
        }

        public override void OnCreate() {
            dictionary = new RepeatableList<(string, string)>() {
                ValueTuple.Create("Mein lieber Herr Gesangsverein!", "My dear mister singing club!")
            };
        }
        
    }
}
