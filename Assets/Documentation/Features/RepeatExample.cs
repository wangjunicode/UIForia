using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.Features {
    
    [Template("Features/RepeatExample.xml")]
    public class RepeatExample : UIElement {

        public List<ValueTuple<string, string>> dictionary;

        public string GetAStyle() {
            return "a";
        }

        public override void OnCreate() {
            dictionary = new List<(string, string)>() {
                ValueTuple.Create("Mein lieber Herr Gesangsverein!", "My dear mister singing club!"),
                ValueTuple.Create("Ich habe Kohldampf!", "I'm having cabbage steam!")
            };
        }
        
    }
}
