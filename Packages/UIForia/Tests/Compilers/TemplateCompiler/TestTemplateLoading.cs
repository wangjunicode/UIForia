using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers;
using UIForia.Test.TestData;
using UnityEngine;

[TestFixture]
public class TestTemplateLoading {

    [Test]
    public void LoadTemplateFromFile() {
        
        TemplateSettings settings = new TemplateSettings();
        settings.applicationName = "TestApp";
        settings.assemblyName = GetType().Assembly.GetName().Name;
        settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
        settings.codeFileExtension = "cs";
        settings.preCompiledTemplatePath = "Assets/UIForia_Generated/TestApp";
        settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");

        TemplateCompiler2 compiler = new TemplateCompiler2(settings);

        // maybe this should also know the root type for an application
        PreCompiledTemplateData compiledOutput = new PreCompiledTemplateData(settings);

        compiler.CompileTemplates(typeof(LoadTemplate0), compiledOutput);

        compiledOutput.GenerateCode();

        compiledOutput.LoadTemplates();

        Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName("UIForia.Test");

        Debug.Log(assembly);
        // MockApplication application = new MockApplication(settings, compiledOutput);
    }

}