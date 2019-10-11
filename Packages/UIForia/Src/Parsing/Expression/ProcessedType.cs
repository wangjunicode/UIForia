using System;
using System.Diagnostics;
using System.IO;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing.Expression {

    public struct TemplateDefinition {

        public string contents;
        public TemplateLanguage language;
        public string filePath;

    }

    [DebuggerDisplay("{rawType.Name}")]
    public struct ProcessedType {

        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly bool requiresTemplateExpansion;
        public bool requiresUpdateFn;

        public ProcessedType(Type rawType, TemplateAttribute templateAttr) {
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate)));
            this.requiresTemplateExpansion = (
                !typeof(UIContainerElement).IsAssignableFrom(rawType) &&
                !typeof(UITextElement).IsAssignableFrom(rawType) &&
                !typeof(UISlotDefinition).IsAssignableFrom(rawType) &&
                !typeof(UISlotContent).IsAssignableFrom(rawType)
            );
        }

        public TemplateDefinition GetTemplate(string templateRoot) {
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

                    TemplateLanguage language = TemplateLanguage.XML;
//                    if (Path.GetExtension(templatePath) == "xml") {
//                    }

                    return new TemplateDefinition() {
                        contents = file,
                        language = language
                    };
                }

                case TemplateType.File: {
                    string templatePath = Application.Settings.GetTemplatePath(templateRoot, templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(templateRoot, $"Cannot find template in path {templatePath}.");
                    }

                    TemplateLanguage language = TemplateLanguage.XML;

                    return new TemplateDefinition() {
                        contents = file,
                        language = language
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
            }
        }

        public TemplateDefinition GetTemplateFromApplication(Application application) {
            if (templateAttr == null) {
                throw new Exception($"Template not defined for {rawType.Name}");
            }

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string templatePath = application.settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(application.TemplateRootPath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = GetTemplatePath(),
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.File: {
                    string templatePath = application.settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(application.TemplateRootPath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
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

        public static bool operator ==(ProcessedType processedType, Type type) {
            return processedType.rawType == type;
        }

        public static bool operator !=(ProcessedType processedType, Type type) {
            return processedType.rawType != type;
        }

    }

}