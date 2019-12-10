using System;
using System.Diagnostics;
using UIForia.Compilers;
using UIForia.Parsing;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public class GameApplication : Application {
        
        protected GameApplication(CompiledTemplateData templateData, ResourceManager resourceManager) : base(templateData, resourceManager) { }
        
        public static GameApplication Create(Type type, TemplateSettings templateSettings, Camera camera, Action<Application> onBootstrap = null) {
            
            TemplateCompiler compiler = new TemplateCompiler(templateSettings);

            CompiledTemplateData compiledOutput = new RuntimeTemplateData(templateSettings);

            Debug.Log("Starting");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            compiler.CompileTemplates(type, compiledOutput);
            watch.Stop();
            Debug.Log("loaded app in " + watch.ElapsedMilliseconds);
            compiledOutput.LoadTemplates();
            
            GameApplication retn = new GameApplication(compiledOutput, null);
            
            onBootstrap?.Invoke(retn);
            
            retn.SetCamera(camera);
            
            return retn;
        }

        public static Application CreatePrecompiled(CompiledTemplateData compiledOutput, Camera camera) {
            
            compiledOutput.LoadTemplates();

            GameApplication retn = new GameApplication(compiledOutput, null);
            
            retn.SetCamera(camera);
            
            return retn;
        }

    }

}