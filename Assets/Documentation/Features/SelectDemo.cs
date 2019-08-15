using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.Features {

    public class Language {
        public string Name;
    }

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
        public RepeatableList<ISelectOption<string>>[] translations;
        public RepeatableList<ISelectOption<string>> languages;

        public RepeatableList<Language> selectedLanguage;

        public RepeatableList<string> words;
        
        public override void OnCreate() {
            intList = new RepeatableList<ISelectOption<int>>();
            
            translations = new RepeatableList<ISelectOption<string>>[] {
                    new RepeatableList<ISelectOption<string>>() {
                            new SelectOption<string>("Hello", "en"),
                            new SelectOption<string>("Hallo", "de")
                    },
                    
                    new RepeatableList<ISelectOption<string>>() {
                            new SelectOption<string>("World", "en"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("Welt", "de")
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
            
            selectedLanguage = new RepeatableList<Language>() {
                    new Language() { Name = "de"}, 
                    new Language() { Name = "de"}
            };
            
            for (int i = 0; i < 10; i++) {
                intList.Add(new SelectOption<int>(i.ToString(), i));
            }
        }
        
        public void ClearTranslations(int index) {
            RepeatableList<ISelectOption<string>> options = translations[index];
            
            List<ISelectOption<string>> temp = new List<ISelectOption<string>>();
            foreach (var option in options) {
                temp.Add(option);
            }
            
            options.Clear();
                  
            foreach (var option in temp) {
                options.Add(option);
            }

            selectedLanguage[index].Name = null;
        }

        public void ClearWords() {
            words.Clear();
        }

        public void AddWords() {
            words.Add("hello");
            words.Add("world");
        }
    }

}