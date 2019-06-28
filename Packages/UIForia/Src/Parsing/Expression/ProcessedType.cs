using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Exceptions;
using UIForia.Templates;
using Debug = UnityEngine.Debug;

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

        public ProcessedType(Type rawType, TemplateAttribute templateAttr) {
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.getResolvers = null;
            // todo -- remove this and replace with a better way to introduce context
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
                case TemplateType.Internal: {
                    string templatePath = Application.Settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(templateRoot, $"Cannot find template in (internal) path {templatePath}.");
                    }
                    return file;
                }

                case TemplateType.File: {
                    string templatePath = Application.Settings.GetTemplatePath(templateRoot, templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(templateRoot, $"Cannot find template in path {templatePath}.");
                    }

                    return file;
                }

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
            catch (FileNotFoundException e) {
                Debug.LogWarning(e.Message);
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