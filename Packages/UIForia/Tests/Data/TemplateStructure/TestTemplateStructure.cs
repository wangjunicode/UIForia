using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace TemplateStructure {

    public class TestTemplateStructure {

        private bool usePreCompiledTemplates = true;

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

            TemplateCodeGenerator.Generate(typeof(T), settings);
            
            CompiledTemplateData compiledTemplates = usePreCompiledTemplates
                ? TemplateLoader.LoadPrecompiledTemplates(settings)
                : TemplateLoader.LoadRuntimeTemplates(typeof(T), settings);

            return new MockApplication(compiledTemplates, null);
        }

        [Template("Data/TemplateStructure/SlotOverride/TemplateStructure_SlotOverride_Main.xml")]
        public class TemplateStructure_SlotOverride_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride/TemplateStructure_SlotOverride_Expand.xml")]
        public class TemplateStructure_SlotOverride_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseOverride() {
            MockApplication app = Setup<TemplateStructure_SlotOverride_Main>();

            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Expand>(app.RootElement[0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
            Assert.AreEqual("Override successful", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Main.xml")]
        public class TemplateStructure_SlotOverrideDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Expand.xml")]
        public class TemplateStructure_SlotOverrideDefault_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseDefault() {
            MockApplication app = Setup<TemplateStructure_SlotOverrideDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideDefault_Expand>(app.RootElement[0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
            Assert.AreEqual("Override failed", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Main.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Exposer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Exposer : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Definer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverrideExtern_OuterOverride() {
            MockApplication app = Setup<TemplateStructure_SlotOverrideExternOuterOverride_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Override from outer layer", textElement.GetText().Trim());
        }


        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer : UIElement { }
        
        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_ExternDefault() {
            MockApplication app = Setup<TemplateStructure_SlotOverride_Extern_ExternDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Override from exposer layer", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer : UIElement { }
        
        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_DefinerDefault() {
            MockApplication app = Setup<TemplateStructure_SlotOverride_Extern_DefinerDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Not overridden", textElement.GetText().Trim());
        }

    }

}