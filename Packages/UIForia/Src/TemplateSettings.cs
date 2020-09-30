using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UIForia.Elements;
using UIForia.Text;
using UIForia.Util;
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
        internal LightList<TextEffectDefinition> textEffectDefs;

        public readonly string templateFileExtension = ".xml";
        
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

        public string GetTemplatePath(string templateAttrTemplate) {
            return Path.GetFullPath(Path.Combine(templateResolutionBasePath, templateAttrTemplate)); 
        }

        public void RegisterTextEffect(string effectName, ITextEffectSpawner textEffectSpawner) {
            
            if (string.IsNullOrEmpty(effectName) || textEffectSpawner == null) {
                return;
            }
            
            textEffectDefs = textEffectDefs ?? new LightList<TextEffectDefinition>();
            
            for (int i = 0; i < textEffectDefs.size; i++) {
                if (textEffectDefs.array[i].effectName == effectName) {
                    throw new Exception("Duplicate Text Effect registered with name: " + effectName);
                }
            }

            textEffectDefs.Add(new TextEffectDefinition() {
                effectName = effectName,
                effectSpawner = textEffectSpawner
            });
        }
        
        private string GetCallPath([CallerFilePath] string callerFilePath = "") {
            return Path.GetDirectoryName(callerFilePath);
        }

    }

}