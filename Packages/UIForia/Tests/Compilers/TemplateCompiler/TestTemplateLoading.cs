using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Test.TestData;
using Application = UnityEngine.Application;

[TestFixture]
public class TestTemplateLoading {

    public TemplateSettings Setup(string appName) {
        TemplateSettings settings = new TemplateSettings();
        settings.applicationName = appName;
        settings.assemblyName = GetType().Assembly.GetName().Name;
        settings.outputPath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
        settings.codeFileExtension = "cs";
        settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
        settings.templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests");
        return settings;
    }

    public CompiledTemplateData GetTemplateData<T>(TemplateSettings settings) {
        TemplateCompiler2 compiler = new TemplateCompiler2(settings);

        // maybe this should also know the root type for an application
        //var compiledOutput = new RuntimeTemplateData(settings);
        var compiledOutput = new PreCompiledTemplateData(settings);

        compiler.CompileTemplates(typeof(T), compiledOutput);
        
        compiledOutput.GenerateCode();
        
        compiledOutput.LoadTemplates();

        return compiledOutput;
        
    }
    
    [Test]
    public void LoadTemplateFromFile() {

        TemplateSettings settings = Setup(nameof(LoadTemplateFromFile));
//        
//        TemplateCompiler2 compiler = new TemplateCompiler2(settings);
//
//        // maybe this should also know the root type for an application
//        PreCompiledTemplateData compiledOutput = new PreCompiledTemplateData(settings);
//
//        compiler.CompileTemplates(typeof(LoadTemplate0), compiledOutput);
//        
//        compiledOutput.GenerateCode();
//        
//        compiledOutput.LoadTemplates();
        CompiledTemplateData templates = GetTemplateData<LoadTemplate0>(settings);
        
        MockApplication app = new MockApplication(templates, null);
        Assert.IsInstanceOf<LoadTemplate0>(app.GetView(0).RootElement);
        LoadTemplate0 root = (LoadTemplate0)app.GetView(0).RootElement;
        Assert.AreEqual(2, root.ChildCount);
        Assert.IsInstanceOf<UITextElement>(root.GetChild(0));
        Assert.IsInstanceOf<LoadTemplateHydrate>(root.GetChild(1));
    }

    [Test]
    public void VerifyHierarchy() {
        
    }

}