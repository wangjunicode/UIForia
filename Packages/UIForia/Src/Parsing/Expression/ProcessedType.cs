using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Templates;

namespace UIForia.Parsing.Expression {

    [DebuggerDisplay("{rawType.Name}")]
    public struct ProcessedType {

        private static readonly Type[] s_Signature = {
            typeof(IList<ExpressionAliasResolver>),
            typeof(AttributeList)
        };

        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly Action<IList<ExpressionAliasResolver>, AttributeList> getResolvers;

        public ProcessedType(Type rawType) {
            this.rawType = rawType;
            this.templateAttr = rawType.GetCustomAttribute<TemplateAttribute>();
            this.getResolvers = null;
            MethodInfo info = rawType.GetMethod("GetAliasResolvers", BindingFlags.Static | BindingFlags.NonPublic, null, s_Signature, null);

            if (info != null) {
                this.getResolvers = (Action<IList<ExpressionAliasResolver>, AttributeList>) Delegate.CreateDelegate(
                    typeof(Action<IList<ExpressionAliasResolver>, AttributeList>), info
                );
            }
        }

        public string GetTemplate(string templateRoot) {
            if (templateAttr == null) {
                throw new Exception($"Template not defined for {rawType.Name}");
            }

            switch (templateAttr.templateType) {
                case TemplateType.Internal:
                    string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src", templateAttr.template));
                    return TryReadFile(path);
                case TemplateType.File:
                    return TryReadFile(Path.GetFullPath(Path.Combine(templateRoot, templateAttr.template)));
                default:
                    return templateAttr.template;
            }
        }

        private static string TryReadFile(string path) {
            if (!path.EndsWith(".xml")) {
                path += ".xml";
            }

            // todo should probably be cached, but be careful about reloading

            try {
                return File.ReadAllText(path);
            }
            catch (FileNotFoundException) {
                throw;
            }
            catch (Exception) {
                return null;
            }
        }

        public bool HasTemplatePath() {
            return templateAttr.templateType == TemplateType.File;
        }

        // path from Assets directory
        public string GetTemplatePath() {
            return !HasTemplatePath() ? rawType.AssemblyQualifiedName : templateAttr.template;
        }

    }

}