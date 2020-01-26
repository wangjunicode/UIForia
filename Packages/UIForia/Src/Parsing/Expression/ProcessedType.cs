using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly bool requiresUpdateFn;
        private object rawCtorFn;
        private StructList<PropertyChangeHandlerDesc> methods;
        private Func<ProcessedType, UIElement, UIElement> constructionFn;
        public readonly string tagName;
        public string namespaceName;
        public int id;
        public int references;
        public bool requiresBeforePropertyUpdates;
        public bool requiresAfterPropertyUpdates;

        public ProcessedType(Type rawType, TemplateAttribute templateAttr, string tagName = null) {
            this.id = -1; // set by TypeProcessor
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.tagName = tagName;
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate)));
            this.requiresBeforePropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnBeforePropertyBindings)));
            this.requiresAfterPropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnAfterPropertyBindings)));
            this.namespaceName = rawType.Namespace;
        }

        public struct PropertyChangeHandlerDesc {

            public MethodInfo methodInfo;
            public string memberName;

        }

        public bool IsUnresolvedGeneric { get; set; }

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

    }

}