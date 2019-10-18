using System;
using System.IO;
using UnityEngine;

namespace UIForia {

    public class TemplateSettings {

        public string templateResolutionBasePath;
        public string preCompiledTemplatePath;

        private static readonly string s_InternalNonStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");

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