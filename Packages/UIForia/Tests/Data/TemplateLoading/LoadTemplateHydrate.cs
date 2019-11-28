using System;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Test.TestData {

    [Template("Data/TemplateLoading/LoadTemplateHydrate.xml")]
    public class LoadTemplateHydrate : UIElement {

        public float floatVal;
        public int intVal;
        public int intVal2 { get; set; }

        [AliasGenericParameter(0, "valueName")]
        public event Action<string> onDidSomething;
        
        public override void OnUpdate() {}

        [OnPropertyChanged(nameof(intVal))]
        public void HandleIntValChanged() { }
        
    }

} 