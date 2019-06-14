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

        internal static readonly string s_InternalStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForiaInternal");
        internal static readonly string s_InternalNonStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");
        internal static readonly string s_UserNonStreamingPath = UnityEngine.Application.dataPath;
        internal static readonly string s_UserStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia");

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
                    if (Application.ReadTemplatesFromStreamingAssets) {
                        string path = Path.GetFullPath(Path.Combine(s_InternalStreamingPath, templateAttr.template));
                        return TryReadFile(path);
                    }
                    else {
                        string path = Path.GetFullPath(Path.Combine(s_InternalNonStreamingPath, templateAttr.template));
                        return TryReadFile(path);
                        
                    }
                
                case TemplateType.File:
                    if (Application.ReadTemplatesFromStreamingAssets) {
                        return TryReadFile(Path.GetFullPath(Path.Combine(s_UserStreamingPath, templateAttr.template)));
                    }
                    else {
                        return TryReadFile(Path.GetFullPath(Path.Combine(s_UserNonStreamingPath, templateAttr.template)));
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