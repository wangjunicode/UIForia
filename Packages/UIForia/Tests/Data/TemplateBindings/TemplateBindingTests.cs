using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace TemplateBinding {

    public class TemplateBindingTests {

        private bool usePreCompiledTemplates = false;
        private bool generateCode = true;

        public MockApplication Setup<T>(string appName = null) {
            if (appName == null) {
                StackTrace stackTrace = new StackTrace();
                appName = stackTrace.GetFrame(1).GetMethod().Name;
            }

            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = appName;
            settings.assemblyName = GetType().Assembly.GetName().Name;
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = "generated.xml.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");

            if (generateCode) {
                TemplateCodeGenerator.Generate(typeof(T), settings);
            }

            CompiledTemplateData compiledTemplates = usePreCompiledTemplates
                ? TemplateLoader.LoadPrecompiledTemplates(settings)
                : TemplateLoader.LoadRuntimeTemplates(typeof(T), settings);

            return new MockApplication(compiledTemplates, null);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding.xml")]
        public class TemplateBindingTest_BasicBindingOuter : UIElement { }

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding.xml#inner")]
        public class TemplateBindingTest_BasicBindingInner : UIElement {

            public int intVal = 5;

        }

        [Test]
        public void SimpleBinding() {
            MockApplication app = Setup<TemplateBindingTest_BasicBindingOuter>();
            TemplateBindingTest_BasicBindingInner inner = (TemplateBindingTest_BasicBindingInner) app.RootElement[0];
            Assert.AreEqual(5, inner.intVal);
            app.Update();
            Assert.AreEqual(25, inner.intVal);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_CreatedBinding.xml")]
        public class TemplateBindingTest_CreatedBindingOuter : UIElement {

            public int value = 15;

            public int GetValue() {
                return value;
            }

        }

        [Template("Data/TemplateBindings/TemplateBindingTest_CreatedBinding.xml#inner")]
        public class TemplateBindingTest_CreatedBindingInner : UIElement {

            public int intVal;

        }

        [Test]
        public void CreatedBinding() {
            MockApplication app = Setup<TemplateBindingTest_CreatedBindingOuter>();

            TemplateBindingTest_CreatedBindingOuter outer = (TemplateBindingTest_CreatedBindingOuter) app.RootElement;
            TemplateBindingTest_CreatedBindingInner inner = (TemplateBindingTest_CreatedBindingInner) app.RootElement[0];

            int original = outer.value;

            Assert.AreEqual(original, inner.intVal);
            outer.value = 25;
            app.Update();
            Assert.AreEqual(original, inner.intVal);
            Assert.AreEqual(25, outer.GetValue());
        }

    }

}