using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia.Parsing {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public readonly Type rawType;

        internal int id;
        internal int references;
        internal Type moduleType;

        internal string tagName;
        internal string elementPath;
        internal string namespaceName;
        internal string templatePath;
        internal string templateId;
        internal string templateSource;
        internal string implicitStyles;
        internal string[] importedStyleSheets;

        private Flags flags;
        internal StructList<PropertyChangeHandlerDesc> methods;
        private static int currentTypeId = -1;

        [Flags]
        private enum Flags {

            RequiresBeforePropertyUpdates = 1 << 0,
            RequiresAfterPropertyUpdates = 1 << 1,
            RequiresOnEnable = 1 << 2,
            RequiresUpdateFn = 1 << 3,
            IsDynamic = 1 << 4,
            IsUnresolvedGeneric = 1 << 5

        }

        internal ProcessedType(Type rawType, string elementPath, string templatePath, string templateId, string tagName, string implicitStyles, string[] styleSheets) {
            this.id = Interlocked.Add(ref currentTypeId, 1);
            this.rawType = rawType;
            this.tagName = tagName;
            this.elementPath = elementPath;
            this.templatePath = templatePath;
            this.templateId = templateId;
            this.implicitStyles = implicitStyles;
            this.importedStyleSheets = styleSheets;
            this.namespaceName = rawType.Namespace;

            // todo -- this is really expensive, consider deferring until we actually need it in the template compiler.
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            this.requiresOnEnable = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnEnable), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            this.requiresBeforePropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnBeforePropertyBindings), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            this.requiresAfterPropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnAfterPropertyBindings), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));

        }

        public bool requiresUpdateFn {
            get { return (flags & Flags.RequiresUpdateFn) != 0; }
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
            get { return (flags & Flags.RequiresOnEnable) != 0; }
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
            get { return (flags & Flags.RequiresBeforePropertyUpdates) != 0; }
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
            get { return (flags & Flags.IsDynamic) != 0; }
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
            get { return (flags & Flags.RequiresAfterPropertyUpdates) != 0; }
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
            get { return (flags & Flags.IsUnresolvedGeneric) != 0; }
            set {
                if (value) {
                    flags |= Flags.IsUnresolvedGeneric;
                }
                else {
                    flags &= ~Flags.IsUnresolvedGeneric;
                }
            }
        }

        
        public struct PropertyChangeHandlerDesc {

            public MethodInfo methodInfo;
            public string memberName;

        }


        public void GetChangeHandlers(string memberName, StructList<PropertyChangeHandlerDesc> retn) {
            if (methods == null) {
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

        public void ValidateAttributes(StructList<AttributeDefinition> attributes) { }

        public ProcessedType Reference() {
            references++;
            return this;
        }

        public static ProcessedType CreateFromGenericDefinition(Type currentType) {
            Assert.IsTrue(currentType.IsGenericTypeDefinition);
            throw new NotImplementedException();
        }

        public static ProcessedType ResolveGeneric(Type newType, ProcessedType generic) {
            throw new NotImplementedException();
        }

        public static ProcessedType CreateFromDynamicType(Type type, string templateShellFilePath, string nodeTemplateId) {
            throw new NotImplementedException();
        }

        public static ProcessedType CreateFromType(Type type) {
            LightList<string> styles = null;
            string[] styleSheets = null;

            string tagName = type.Name;
            string elementPath = null;
            string implicitStyleNames = null;
            string templatePath = null;
            string templateId = null;

            foreach (Attribute attr in type.GetCustomAttributes()) {
                if (attr is TemplateTagNameAttribute templateTagNameAttr) {
                    if (elementPath != null && elementPath != templateTagNameAttr.filePath) {
                        throw new Exception($"File paths were different {elementPath}, {templateTagNameAttr.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                    }

                    tagName = templateTagNameAttr.tagName;
                    elementPath = templateTagNameAttr.filePath;
                    continue;
                }

                if (attr is TemplateAttribute templateAttribute) {
                    if (elementPath != null && elementPath != templateAttribute.filePath) {
                        throw new Exception($"File paths were different {elementPath}, {templateAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                    }

                    elementPath = templateAttribute.elementPath;
                    templatePath = templateAttribute.templatePath;
                    int idx = templatePath.IndexOf('#');
                    if (idx >= 0) {
                        templateId = templatePath.Substring(idx + 1);
                        templatePath = templatePath.Substring(0, idx);
                    }

                    continue;
                }

                if (attr is RecordFilePathAttribute recordFilePathAttribute) {
                    if (elementPath != null && elementPath != recordFilePathAttribute.filePath) {
                        throw new Exception($"File paths were different {elementPath}, {recordFilePathAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                    }

                    elementPath = recordFilePathAttribute.filePath;
                    continue;
                }

                if (attr is ImportStyleSheetAttribute importStyleSheetAttribute) {
                    if (elementPath != null && elementPath != importStyleSheetAttribute.filePath) {
                        throw new Exception($"File paths were different {elementPath}, {importStyleSheetAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                    }

                    styles = styles ?? LightList<string>.Get();
                    styles.Add(importStyleSheetAttribute.styleSheet);
                    elementPath = importStyleSheetAttribute.filePath;
                    continue;
                }

                if (attr is StyleAttribute styleAttribute) {
                    if (elementPath != null && elementPath != styleAttribute.filePath) {
                        throw new Exception($"File paths were different {elementPath}, {styleAttribute.filePath} for element type {TypeNameGenerator.GetTypeName(type)}");
                    }

                    implicitStyleNames = styleAttribute.styleNames;
                }
            }


            if (styles != null) {
                styleSheets = styles.ToArray();
                styles.Release();
            }

            return new ProcessedType(type, elementPath, templatePath, templateId, tagName, implicitStyleNames, styleSheets);
        }

    }

}