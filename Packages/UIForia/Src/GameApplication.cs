using System;
using UIForia.Compilers;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {
        
        protected GameApplication(CompiledTemplateData templateData, ResourceManager resourceManager) : base(templateData, resourceManager) { }
        
        public static GameApplication Create(Type type, TemplateSettings templateSettings, Camera camera, Action<Application> onBootstrap = null) {
            
            TemplateCompiler compiler = new TemplateCompiler(templateSettings);

            CompiledTemplateData compiledOutput = new RuntimeTemplateData(templateSettings);

            compiler.CompileTemplates(type, compiledOutput);

            compiledOutput.LoadTemplates();

            GameApplication retn = new GameApplication(compiledOutput, null);
            
            onBootstrap?.Invoke(retn);
            
            retn.SetCamera(camera);
            
            return retn;
        }

    }

}