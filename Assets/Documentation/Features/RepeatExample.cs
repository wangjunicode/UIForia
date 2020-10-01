using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {
    
    [Template("Features/RepeatExample.xml")]
    public class RepeatExample : UIElement {

        public List<string> wordList;

        public string newWord;

        public override void OnCreate() {
            wordList = new List<string>() {
                "Hello"
            };
        }

        public void AddWord() {
            if (string.IsNullOrWhiteSpace(newWord) || wordList.Contains(newWord)) {
                return;
            }

            wordList.Add(newWord);
            wordList.Sort();
        }
    }
}
