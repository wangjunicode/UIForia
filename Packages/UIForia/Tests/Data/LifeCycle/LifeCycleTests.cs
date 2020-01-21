using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace LifeCycle {

    public class LifeCycleTests {

        private bool usePreCompiledTemplates = false;
        private bool generateCode = false;

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

        public class LifeCycleElement : UIContainerElement { }


        [Template("Data/LifeCycle/LifeCycleTests_EnableElement.xml#test1")]
        public class LifeCycleTest_EnableElement : UIElement { }


        [Test]
        public void EnableElement() {
            MockApplication app = Setup<LifeCycleTest_EnableElement>();
            LifeCycleTest_EnableElement e = (LifeCycleTest_EnableElement) app.RootElement;
            throw new NotImplementedException();
        }

    }

}