using System.Collections.Generic;
using Src;

namespace Spec {

    [Template("Tests/Templates/Parsing/Temp.xml")]
    public class Temp : UIElement { }

    [Template("Tests/Templates/Parsing/Test1.xml")]
    public class Test1 : UIElement { }

    [Template("Tests/Templates/Parsing/Test2.xml")]
    public class Test2 : UIElement { }

    [Template("Tests/Templates/Parsing/Test3.xml")]
    public class Test3 : UIElement {

        public ObservedProperty<float> floatProperty;

    }

    [Template("Tests/Templates/Parsing/Test4.xml")]
    public class Test4 : UIElement {

        public ObservedProperty<bool> isPanelVisible;

    }

    [Template("Tests/Templates/Parsing/Test5.xml")]
    public class Test5 : UIElement { }

    [Template("Tests/Templates/Parsing/Test6.xml")]
    public class Test6 : UIElement { }

    [Template("Tests/Templates/Parsing/Test7.xml")]
    public class Test7 : UIElement { }

    [Template("Tests/Templates/Parsing/Test8.xml")]
    public class Test8 : UIElement { }

    [Template("Tests/Templates/Parsing/Test9.xml")]
    public class Test9 : UIElement { }

    [Template("Tests/Templates/Parsing/Test10.xml")]
    public class Test10 : UIElement { }

    [Template("Tests/Templates/Parsing/Test11.xml")]
    public class Test11 : UIElement { }

    [Template("Tests/Templates/Parsing/Test12.xml")]
    public class Test12 : UIElement { }

    namespace Props {

        [Template("Tests/Templates/Props/Test1.xml")]
        public class Test1 : UIElement { }

        public class Test1Thing : UIElementPrimitive {

            [Prop] public string stringValue;
            [Prop] public float floatValue;
            [Prop] public int intValue;
            [Prop] public bool boolValue;

        }
        
        [Template("Tests/Templates/Props/Test2.xml")]
        public class Test2 : UIElement { }

        public class Test2Thing : UIElementPrimitive {

            [Prop] public string stringValue;
            [Prop] public float floatValue;
            [Prop] public int intValue;
            [Prop] public bool boolValue;

        }

        [Template("Tests/Templates/Props/Test3.xml")]
        public class Test3 : UIElement {

            public ObservedProperty<int> value;

            public override void Initialize() {
                value.Value = 123;
            }

        }

        
    }

}