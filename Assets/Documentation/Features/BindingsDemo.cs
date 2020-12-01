using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {

    [Template("Documentation/Features/BindingsDemo.xml")]
    public class BindingsDemo : UIElement {
        public class InnerAccess {
            public string value;
        }

        public InnerAccess left = new InnerAccess() { value = "left value"};
        public InnerAccess right = null;
        
        public int stepperValue = 7;

    }

}