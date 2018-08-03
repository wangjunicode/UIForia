using Src;

namespace Spec {

    public class Test1 : UIElement { }

    public class Test2 : UIElement { }

    public class Test3 : UIElement {
        public ObservedProperty<float> floatProperty;
    }

    public class Test4 : UIElement {
        public ObservedProperty<bool> isPanelVisible;
    }
}