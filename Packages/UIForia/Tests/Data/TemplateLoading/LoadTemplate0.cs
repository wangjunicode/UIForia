using UIForia.Attributes;
using UIForia.Elements;

namespace UIForia.Test.TestData {

    public class RefTypeThing {

        public RefTypeThing nested;
        public int intVal;

    }
    
    [Template("Data/TemplateLoading/LoadTemplate0.xml")]
    public class LoadTemplate0 : UIElement {

        public RefTypeThing refTypeThing;
        public float floatVal;
        public int intVal;
        public int intVal2 { get; set; }
        public string computed = "i-am-computed";

    }

}