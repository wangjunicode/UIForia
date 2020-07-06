using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UIForia.Elements;
using UnityEditor.PackageManager;
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
        
        public TemplateSettings() {
            this.applicationName = "DefaultApplication";
            this.assemblyName = "UIForia.Application";
            this.outputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.preCompiledTemplatePath = "Assets/__UIForiaGenerated__";
            this.codeFileExtension = "cs";
            this.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
        }
        
        // todo -- remove this dirty hack!
        static TemplateSettings() {
            s_InternalNonStreamingPath = Path.Combine(PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(UIElement))).resolvedPath, "Src");
        }

        private static readonly string s_InternalNonStreamingPath;

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
            return Path.GetFullPath(Path.Combine(s_InternalNonStreamingPath, fileName));
        }

        public string GetTemplatePath(string templateAttrTemplate) {
            return Path.GetFullPath(Path.Combine(templateResolutionBasePath, templateAttrTemplate)); 
        }

    }

}