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

        public override void OnCreate() {
            dictionary = new RepeatableList<(string, string)>() {
                ValueTuple.Create("Mein lieber Herr Gesangsverein!", "My dear mister singing club!"),
                ValueTuple.Create("Ich habe Kohldampf!", "I'm having cabbage steam!")
            };
        }
        
    }
}
