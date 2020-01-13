using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {
        
        protected GameApplication(CompiledTemplateData templateData, ResourceManager resourceManager) : base(templateData, resourceManager) { }

        public static Application Create(CompiledTemplateData compiledOutput, Camera camera) {
            
            GameApplication retn = new GameApplication(compiledOutput, null);
            
            retn.SetCamera(camera);
            
            return retn;
        }

    }

}