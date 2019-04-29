using System;
using Documentation.DocumentationElements;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {
    public class DynamicDataTest<T> : IDynamicData where T : UIElement {

        public Type ElementType => typeof(T);

    }

    [Template("Documentation/Features/DynamicDemo.xml")]
    public class DynamicDemo : UIElement {

        public IDynamicData currentDynamicItem;

        public void SelectDynamic(int idx) {
            switch (idx) {
                case 0:
                    currentDynamicItem = new DynamicDataTest<DynamicType0>();
                    break;
                case 1:
                    currentDynamicItem = new DynamicDataTest<DynamicType1>();
                    break;
            }
        }

    }
}