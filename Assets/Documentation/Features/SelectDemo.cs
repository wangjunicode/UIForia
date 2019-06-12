using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;
using UnityEditor.U2D;

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

        public int selectedInt;
        public string selectedString = "3";
        public int selectedStringIndex = 3;
        
        public RepeatableList<ISelectOption<int>> intList;
        public RepeatableList<ISelectOption<int>>[] translations;
        public RepeatableList<ISelectOption<string>> languages;

        public RepeatableList<int> selectedLanguage;

        public RepeatableList<string> words;
        
        public override void OnCreate() {
            intList = new RepeatableList<ISelectOption<int>>();
            
            translations = new RepeatableList<ISelectOption<int>>[] {
                    new RepeatableList<ISelectOption<int>>() {
                            new SelectOption<int>("Hello", 0),
                            new SelectOption<int>("Hallo", 1)
                    },
                    
                    new RepeatableList<ISelectOption<int>>() {
                            new SelectOption<int>("World", 0),
                            new SelectOption<int>("Welt", 1)
                    },
            };
            
            languages = new RepeatableList<ISelectOption<string>>() {
                    new SelectOption<string>("English", "en"), 
                    new SelectOption<string>("German", "de"),
            };
            
            words = new RepeatableList<string>() {
                    "hello",
                    "world"
            };
            
            selectedLanguage = new RepeatableList<int>() {
                    0, 0
            };
            
            for (int i = 0; i < 10; i++) {
                intList.Add(new SelectOption<int>(i.ToString(), i));
                
            }
        }

        private void SetEmpty(MouseInputEvent evt) {
            if (evt.IsConsumed) {
                return;
            }
            selectedStringIndex = -1;
        }

        private void ClearWords() {
            words.Clear();
        }

        private void AddWords() {
            words.Add("hello");
            words.Add("world");
        }
    }

}