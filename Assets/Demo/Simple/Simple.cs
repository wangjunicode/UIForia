using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {

    [Template("Demo/Simple/Simple.xml")]
    public class Simple : UIElement {
        
        public int boxClicks1;
        public int boxClicks2;
        public int boxClicks3;

        public List<string> words = new List<string>() {
            "Matt","is", "the","greatest"
        };
        
        private bool isEnglish = true;
        
        public string HeaderText => isEnglish 
                ? "The German Click Flag" 
                : "Die Deutsche Klickfahne";

        public void ToggleLanguage() {
            isEnglish = !isEnglish;
        }
     
        public void OnBoxClicked(int boxIndex) {
            switch (boxIndex) {
                case 1:
                    boxClicks1++;
                    break;
                case 2:
                    boxClicks2++;
                    break;
                case 3:
                    boxClicks3++;
                    break;
            }
        }
    }
}