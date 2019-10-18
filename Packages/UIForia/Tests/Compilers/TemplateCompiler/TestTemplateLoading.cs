using System.IO;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers;
using UIForia.Test.TestData;

[TestFixture]
public class TestTemplateLoading {

    [Test]
    public void LoadTemplateFromFile() {
        
        TemplateSettings settings = new TemplateSettings();
        settings.preCompiledTemplatePath = "Assets/UIForia_Generated/TestApp";
        settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");

        TemplateCompiler2 compiler = new TemplateCompiler2(settings);
        
        // maybe this should also know the root type for an application
        ICompiledTemplateData compiledOutput = new PreCompiledTemplateData("appName");
        
        compiler.CompileTemplates(typeof(LoadTemplate0), compiledOutput);

        compiledOutput.GenerateCode();
        
        // MockApplication application = new MockApplication(settings, compiledOutput);

    }

}