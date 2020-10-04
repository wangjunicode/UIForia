using System.IO;

namespace UIForia {

    public class PrecompileSettings {

        public string assemblyName;
        public string codeOutputPath;
        public string styleOutputPath;
        public string rootTypeName;
        public string codeFileExtension;

        public PrecompileSettings() {
            this.assemblyName = "UIForia.Application";
            this.styleOutputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.codeOutputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.codeFileExtension = "generated.cs";
            this.rootTypeName = "UIForiaGeneratedApplication";
        }

    }

}