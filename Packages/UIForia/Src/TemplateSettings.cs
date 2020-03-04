using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UIForia.Style2;
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

        private List<StyleCondition> conditions;

        public TemplateSettings() {
            this.conditions = new List<StyleCondition>();
            this.applicationName = "DefaultApplication";
            this.assemblyName = "UIForia.Application";
            this.outputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.preCompiledTemplatePath = "Assets/__UIForiaGenerated__";
            this.codeFileExtension = "cs";
            this.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
        }

        private static readonly string s_InternalNonStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");

        public string StrippedApplicationName => Regex.Replace(applicationName, @"\s", "");

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

        public bool TryGetStyleCondition(string conditionName, out StyleCondition condition) {
            for (int i = 0; i < conditions.Count; i++) {
                if (conditionName == conditions[i].name) {
                    condition = conditions[i];
                    return true;
                }
            }

            condition = default;
            return false;
        }

        public bool TryGetStyleCondition(CharSpan conditionName, out StyleCondition condition) {
            for (int i = 0; i < conditions.Count; i++) {
                if (StringUtil.EqualsRangeUnsafe(conditions[i].name, conditionName)) {
                    condition = conditions[i];
                    return true;
                }
            }

            condition = default;
            return false;
        }

        public Func<DisplayConfiguration, bool> GetStyleCondition(string conditionName) {
            for (int i = 0; i < conditions.Count; i++) {
                if (conditions[i].name == conditionName) {
                    return conditions[i].fn;
                }
            }

            return null;
        }

        public Func<DisplayConfiguration, bool> GetStyleCondition(int id) {
            if (id >= 0 && id < conditions.Count) {
                return conditions[id].fn;
            }

            return null;
        }

        public void RegisterStyleCondition(string conditionName, Func<DisplayConfiguration, bool> fn) {
            for (int i = 0; i < conditions.Count; i++) {
                if (conditions[i].name == conditionName) {
                    throw new Exception("Duplicate style condition name: " + conditionName);
                }
            }

            if (fn == null) {
                throw new Exception("Function provided to TemplateSettings.RegisterStyleCondition cannot be null");
            }

            conditions.Add(new StyleCondition(conditions.Count, conditionName, fn));
        }

        public string GetInternalTemplatePath(string fileName) {
            return Path.GetFullPath(Path.Combine(s_InternalNonStreamingPath, fileName));
        }

        public string GetTemplatePath(string templateAttrTemplate) {
            return Path.GetFullPath(Path.Combine(templateResolutionBasePath, templateAttrTemplate));
        }

    }

}