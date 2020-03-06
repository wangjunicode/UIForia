using System;
using System.Collections.Generic;
using UIForia.Style2;
using UIForia.Util;

namespace UIForia {

    public abstract class Module {

        public abstract TemplateSettings Configure();

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;

        public virtual bool EvaluateStyleCondition(string conditionName) {
            return false;
        }

        public virtual string GetTemplatePath(string path, string callerPath) {
            return path;
        }

        public virtual string GetStylePath(string path, string callerPath) {
            return path;
        }

        public void UpdateConditions(DisplayConfiguration displayConfiguration) {
            if (styleConditions == null) return;

            conditionResults = conditionResults ?? new List<bool>();
            conditionResults.Clear();

            for (int i = 0; i < styleConditions.Count; i++) {
                conditionResults.Add(styleConditions[i].fn(displayConfiguration));
            }
        }

        public List<bool> GetDisplayConditions() {
            return conditionResults;
        }

        public void RegisterDisplayCondition(string condition, Func<DisplayConfiguration, bool> fn) {
            if (styleConditions == null) {
                styleConditions = new List<StyleCondition>();
                styleConditions.Add(new StyleCondition(0, condition, fn));
                return;
            }

            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == condition) {
                    throw new Exception("Duplicate DisplayCondition '" + condition + "'");
                }
            }

            styleConditions.Add(new StyleCondition(styleConditions.Count, condition, fn));
        }

        public int GetDisplayConditionId(CharSpan conditionSpan) {
            if (styleConditions == null) return -1;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionSpan) {
                    return i;
                }
            }

            return -1;
        }

        public bool HasStyleCondition(string conditionName) {
            return HasStyleCondition(new CharSpan(conditionName));
        }

        public bool HasStyleCondition(CharSpan conditionName) {
            if (styleConditions == null) return false;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionName) {
                    return true;
                }
            }

            return false;
        }

    }

    public class Module<T> : Module {

        public override TemplateSettings Configure() {
            return new TemplateSettings() {
                rootType = typeof(T),
                // styleRoot = "path/to/root"
            };
        }

    }

}