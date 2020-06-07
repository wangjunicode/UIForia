using UIForia.Attributes;
using UIForia.Elements;

[Template("CompilerDemo/Tmp.xml")]
public class Tmp : UIElement {


        public bool condition;

        public bool SomeCondition() {
            return condition;
        }

  
    
}
