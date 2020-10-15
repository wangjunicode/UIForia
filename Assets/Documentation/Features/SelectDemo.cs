using System.Collections.Generic;
using SeedLib;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {

    public class Language {
        public string Name;
    }

    [Template("Documentation/Features/SelectDemo.xml")]
    public class SelectDemo : UIElement {

        public List<ISelectOption<int>> intList;
        public List<ISelectOption<string>>[] translations;
        public List<ISelectOption<string>> languages;

        public List<Language> selectedLanguage;

        public List<string> words;
        
        public override void OnCreate() {
            intList = new List<ISelectOption<int>>();
            
            translations = new List<ISelectOption<string>>[] {
                    new List<ISelectOption<string>>() {
                            new SelectOption<string>("Hello", "en"),
                            new SelectOption<string>("Hallo", "de")
                    },
                    
                    new List<ISelectOption<string>>() {
                            new SelectOption<string>("World", "en"),
                            new SelectOption<string>("Welt", "de"),
                            new SelectOption<string>("supercalifragilisticexpialidocious", "en"),
                            new SelectOption<string>("Supercalifragilisticexpialigetisch", "de"),
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
            
            languages = new List<ISelectOption<string>>() {
                    new SelectOption<string>("English", "en"), 
                    new SelectOption<string>("German", "de"),
            };
            
            words = new List<string>() {
                    "hello",
                    "world"
            };
            
            selectedLanguage = new List<Language>() {
                    new Language() { Name = "de"}, 
                    new Language() { Name = "de"}
            };
            
            for (int i = 0; i < 10; i++) {
                intList.Add(new SelectOption<int>(i.ToString(), i));
            }
        }
        
        public void ClearTranslations(int index) {
            List<ISelectOption<string>> options = translations[index];
            
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