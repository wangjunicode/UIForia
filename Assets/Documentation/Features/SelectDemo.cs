using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.Features {

    [Template("Documentation/Features/SelectDemo.xml")]
    public class SelectDemo : UIElement {

        public class SelectOption<T> : ISelectOption<T> {

            public string Label { get; set; }
            public T Value { get; set; }

            public SelectOption(string label, T value) {
                this.Label = label;
                this.Value = value;
            }

        }
        
        public RepeatableList<ISelectOption<int>> intList;
        public RepeatableList<ISelectOption<string>> stringList;
        
        public  override void OnCreate() {
            intList = new RepeatableList<ISelectOption<int>>();
            stringList = new RepeatableList<ISelectOption<string>>();
            
            for (int i = 0; i < 10; i++) {
                intList.Add(new SelectOption<int>(i.ToString(), i));
                stringList.Add(new SelectOption<string>("this is label for option number: " + i, "value is : " + i));
            }
            
        }

    }

}