using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Parsing.Expression {

    [DebuggerDisplay("{rawType.Name}")]
    public struct ProcessedType {

        private static readonly Type[] s_Signature = {typeof(ExpressionCompiler)};
        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly Action<ExpressionCompiler> beforeCompileChildren;

        public ProcessedType(Type rawType) {
            this.rawType = rawType;
            this.templateAttr = rawType.GetCustomAttribute<TemplateAttribute>();
            this.beforeCompileChildren = null;
            IEnumerable<TemplateCompilePlugin> plugins = rawType.GetCustomAttributes<TemplateCompilePlugin>();

            foreach (TemplateCompilePlugin plugin in plugins) {
                MethodInfo info = rawType.GetMethod(plugin.methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, s_Signature, null);
                if (info != null) {
                    this.beforeCompileChildren = (Action<ExpressionCompiler>) Delegate.CreateDelegate(typeof(Action<ExpressionCompiler>), info);
                }
                else {
                    UnityEngine.Debug.Log($"Tried to find method {plugin.methodName} on type {rawType.Name} but could not find a valid compile plugin method");
                }
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