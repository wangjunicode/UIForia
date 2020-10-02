using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public readonly Type rawType;
        private StructList<PropertyChangeHandlerDesc> methods;
        public readonly string tagName;
        public int id;
        private Flags flags;
        private Expression ctorExpr;

        public string elementPath;
        public string templateId;
        public string templatePath;

        internal string[] importedStyleSheets;
        internal string implicitStyles;

        public readonly ElementArchetype archetype;

        private static int currentTypeId = -1;
        [ThreadStatic] private static LightList<string> s_StyleList;

        internal ProcessedType(Type rawType, string elementPath, string templatePath, string templateId, string tagName, string implicitStyles, string[] styleSheets) {
            this.id = Interlocked.Add(ref currentTypeId, 1);
            this.rawType = rawType;
            this.tagName = tagName;
            this.elementPath = elementPath;
            this.templatePath = templatePath;
            this.templateId = templateId;
            this.implicitStyles = implicitStyles;
            this.importedStyleSheets = styleSheets;

            if (rawType == typeof(UISlotDefinition)) {
                archetype = ElementArchetype.SlotDefine;
            }
            else if (rawType == typeof(UISlotForward)) {
                archetype = ElementArchetype.SlotForward;
            }
            else if (rawType == typeof(UISlotOverride)) {
                archetype = ElementArchetype.SlotOverride;
            }
            else if (rawType == typeof(UIMetaElement)) {
                archetype = ElementArchetype.Meta;
            }
            else if (DeclaresTemplate) {
                archetype = ElementArchetype.Template;
            }
            else if (typeof(UITextElement).IsAssignableFrom(rawType)) {
                // might not be cheap -- explore skipping when we know 100% is or isn't a text element
                flags |= Flags.IsTextElement;
            }
            else {
                IsContainerElement = true;
            }

            this.IsUnresolvedGeneric = rawType.IsGenericTypeDefinition;

            // todo -- this is really expensive, consider deferring until we actually need it in the template compiler.
            // this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            // this.requiresOnEnable = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnEnable), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            // this.requiresBeforePropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnBeforePropertyBindings), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            // this.requiresAfterPropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnAfterPropertyBindings), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
        }

        [Flags]
        private enum Flags {

            RequiresBeforePropertyUpdates = 1 << 0,
            RequiresAfterPropertyUpdates = 1 << 1,
            RequiresOnEnable = 1 << 2,
            RequiresUpdateFn = 1 << 3,
            IsDynamic = 1 << 4,
            IsUnresolvedGeneric = 1 << 5,
            IsContainerElement = 1 << 6,
            IsTextElement = 1 << 7

        }

        [Flags]
        public enum ElementArchetype {

            Unresolved = 0,
            Template = 1 << 0,
            Meta = 1 << 1,
            SlotDefine = 1 << 2,
            SlotForward = 1 << 3,
            SlotOverride = 1 << 4,
            Container = 1 << 5,
            Text = 1 << 6

        }


        // This is threadsafe
        internal static ProcessedType CreateFromType(Type type, Diagnostics diagnostics) {
            LightList<string> styles = null;
            string[] styleSheets = null;

            string tagName = type.Name;
            string elementPath = null;
            string implicitStyleNames = null;
            string templatePath = null;
            string templateId = null;

            bool templateProvided = false;
            bool isContainer = false;

            if (type.IsGenericTypeDefinition) {
                tagName = tagName.Split('`')[0];
            }


            // this could probably be done the other way around, use type cache to get all types with attributes and blast through each one

            foreach (Attribute attr in type.GetCustomAttributes()) {
                switch (attr) {
                    case TemplateTagNameAttribute templateTagNameAttr when elementPath != null && elementPath != templateTagNameAttr.filePath:
                        diagnostics.LogError($"File paths were different {elementPath}, {templateTagNameAttr.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case TemplateTagNameAttribute templateTagNameAttr:
                        tagName = templateTagNameAttr.tagName;
                        elementPath = templateTagNameAttr.filePath;
                        continue;

                    case TemplateAttribute templateAttribute when elementPath != null && elementPath != templateAttribute.elementPath:
                        diagnostics.LogError($"File paths were different {elementPath}, {templateAttribute.elementPath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case TemplateAttribute templateAttribute: {
                        elementPath = templateAttribute.elementPath;
                        templatePath = templateAttribute.templatePath;
                        templateId = templateAttribute.templateId;

                        if (isContainer) {
                            diagnostics.LogError($"Element cannot be a container and provide a template. {TypeNameGenerator.GetTypeName(type)} is both.");
                            return null;
                        }

                        templateProvided = true;

                        continue;
                    }

                    case RecordFilePathAttribute recordFilePathAttribute when elementPath != null && elementPath != recordFilePathAttribute.filePath:
                        diagnostics.LogError($"File paths were different {elementPath}, {recordFilePathAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case RecordFilePathAttribute recordFilePathAttribute:
                        elementPath = recordFilePathAttribute.filePath;
                        continue;

                    case ImportStyleSheetAttribute importStyleSheetAttribute when elementPath != null && elementPath != importStyleSheetAttribute.filePath:
                        diagnostics.LogError($"File paths were different {elementPath}, {importStyleSheetAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case ImportStyleSheetAttribute importStyleSheetAttribute:
                        s_StyleList = s_StyleList ?? new LightList<string>();
                        s_StyleList.Add(importStyleSheetAttribute.styleSheet);
                        elementPath = importStyleSheetAttribute.filePath;
                        continue;

                    case StyleAttribute styleAttribute when elementPath != null && elementPath != styleAttribute.filePath:
                        diagnostics.LogError($"File paths were different {elementPath}, {styleAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case StyleAttribute styleAttribute:
                        elementPath = styleAttribute.filePath;
                        implicitStyleNames = styleAttribute.styleNames;
                        break;

                    case ContainerElementAttribute containerAttr when elementPath != null && elementPath != containerAttr.filePath:
                        diagnostics.LogError($"File paths were different {elementPath}, {containerAttr.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                        return null;

                    case ContainerElementAttribute containerAttr: {
                        if (templateProvided) {
                            diagnostics.LogError($"Element cannot be a container and provide a template. {TypeNameGenerator.GetTypeName(type)} is both.");
                            return null;
                        }

                        elementPath = containerAttr.filePath;
                        isContainer = true;
                        break;
                    }
                }
            }

            if (s_StyleList != null && s_StyleList.size > 0) {
                styleSheets = s_StyleList.ToArray();
                s_StyleList.size = 0;
            }

            return new ProcessedType(type, elementPath, templatePath, templateId, tagName, implicitStyleNames, styleSheets);
        }

        public bool requiresUpdateFn {
            get => (flags & Flags.RequiresUpdateFn) != 0;
            private set {
                if (value) {
                    flags |= Flags.RequiresUpdateFn;
                }
                else {
                    flags &= ~Flags.RequiresUpdateFn;
                }
            }
        }

        public bool requiresOnEnable {
            get => (flags & Flags.RequiresOnEnable) != 0;
            private set {
                if (value) {
                    flags |= Flags.RequiresOnEnable;
                }
                else {
                    flags &= ~Flags.RequiresOnEnable;
                }
            }
        }

        public bool requiresBeforePropertyUpdates {
            get => (flags & Flags.RequiresBeforePropertyUpdates) != 0;
            private set {
                if (value) {
                    flags |= Flags.RequiresBeforePropertyUpdates;
                }
                else {
                    flags &= ~Flags.RequiresBeforePropertyUpdates;
                }
            }
        }

        public bool isDynamic {
            get => (flags & Flags.IsDynamic) != 0;
            set {
                if (value) {
                    flags |= Flags.IsDynamic;
                }
                else {
                    flags &= ~Flags.IsDynamic;
                }
            }
        }

        public bool requiresAfterPropertyUpdates {
            get => (flags & Flags.RequiresAfterPropertyUpdates) != 0;
            set {
                if (value) {
                    flags |= Flags.RequiresAfterPropertyUpdates;
                }
                else {
                    flags &= ~Flags.RequiresAfterPropertyUpdates;
                }
            }
        }

        public bool IsUnresolvedGeneric {
            get => (flags & Flags.IsUnresolvedGeneric) != 0;
            set {
                if (value) {
                    flags |= Flags.IsUnresolvedGeneric;
                }
                else {
                    flags &= ~Flags.IsUnresolvedGeneric;
                }
            }
        }

        public bool IsContainerElement {
            get => (flags & Flags.IsContainerElement) != 0;
            set {
                if (value) {
                    flags |= Flags.IsContainerElement;
                }
                else {
                    flags &= ~Flags.IsContainerElement;
                }
            }
        }

        public bool IsTextElement {
            get => (flags & Flags.IsTextElement) != 0;
        }

        public bool DeclaresTemplate {
            get => templatePath != null || templateId != null;
        }

        public struct PropertyChangeHandlerDesc {

            public MethodInfo methodInfo;
            public string memberName;

        }

        public void GetChangeHandlers(string memberName, StructList<PropertyChangeHandlerDesc> retn) {
            if (methods == null) {
                // todo -- use type scanner for this probably, not sure how it handles generics though
                MethodInfo[] candidates = ReflectionUtil.GetInstanceMethods(rawType);
                for (int i = 0; i < candidates.Length; i++) {
                    IEnumerable<OnPropertyChanged> attrs = candidates[i].GetCustomAttributes<OnPropertyChanged>();
                    methods = methods ?? new StructList<PropertyChangeHandlerDesc>();
                    foreach (OnPropertyChanged a in attrs) {
                        methods.Add(new PropertyChangeHandlerDesc() {
                            methodInfo = candidates[i],
                            memberName = a.propertyName
                        });
                    }
                }
            }

            if (methods == null) {
                return;
            }

            for (int i = 0; i < methods.size; i++) {
                if (methods.array[i].memberName == memberName) {
                    retn.Add(methods[i]);
                }
            }
        }

        // todo -- remove
        public ProcessedType Reference() {
            return this;
        }

        public Expression GetConstructorExpression() {
            if (ctorExpr == null) {
                ConstructorInfo constructorInfo = rawType.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null) {
                    return null;
                }

                ctorExpr = ExpressionFactory.New(constructorInfo);
            }

            return ctorExpr;
        }

    }

}