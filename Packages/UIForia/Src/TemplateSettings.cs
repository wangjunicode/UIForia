using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UIForia {

    public class TemplateSettings {

        public string assemblyName;
        public string outputPath;
        public string codeFileExtension;
        public string preCompiledTemplatePath;
        public string templateResolutionBasePath;
        public string applicationName;
        public string templateRoot;
        public Type rootType;
        public ResourceManager resourceManager;
        public Func<Type, string, string> filePathResolver;
        public List<Type> dynamicallyCreatedTypes;

        public MaterialReference[] materialAssets;

        public TemplateSettings() {
            this.applicationName = "DefaultApplication";
            this.assemblyName = "UIForia.Application";
            this.outputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.preCompiledTemplatePath = "Assets/__UIForiaGenerated__";
            this.codeFileExtension = "cs";
            this.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
        }

        public string StrippedApplicationName => Regex.Replace(applicationName, @"\s", "" );

        public virtual string TryReadFile(string templatePath) {
            try {
                return File.ReadAllText(templatePath);
            }
            catch (FileNotFoundException e) {
                Debug.LogWarning(e.Message);
                throw;
            }
            catch (Exception) {
                return null;
            }
        }

        public string GetInternalTemplatePath(string fileName) {
            return Path.GetFullPath(Path.Combine(GetCallPath(), fileName));
        }

        private string GetCallPath([CallerFilePath] string callerFilePath = "") {
            return Path.GetDirectoryName(callerFilePath);
        }

        public string GetTemplatePath(string templateAttrTemplate) {
            return Path.GetFullPath(Path.Combine(templateResolutionBasePath, templateAttrTemplate)); 
        }

    }

}