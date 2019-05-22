using System;
using System.Globalization;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.Features {

    public struct Thing {
        public string ThingValue;
    }

    [Template("Documentation/Features/BindingsDemo.xml")]
    public class BindingsDemo : UIElement {
        public static string simpleBinding;
        
        public RepeatableList<int> numbers = new RepeatableList<int>() {1, 2, 3, 4, 5, 6, 7};
        
        public string GetDynamicStyle() {
            return "dynamic-style";
        }

        public Thing GetThing() {
            return new Thing {
                ThingValue = "Current Time: " + DateTime.Now.ToString(CultureInfo.InvariantCulture)
            };
        }
    }

}