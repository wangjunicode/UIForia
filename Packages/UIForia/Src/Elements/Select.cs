using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/Option.xml")]
    public class Option<T> : UIElement {

        public T value;

    }

    public interface ISelectOption<T> {

        string Label { get; }
        T Value { get; }

    }

    public enum TemplateCompilePhase {

        BeforeChildren,
        AfterChildren,
        BeforeSelf,
        AfterSelf

    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TemplateCompilePlugin : Attribute {

        public readonly string methodName;
        public readonly TemplateCompilePhase phase;
        
        public TemplateCompilePlugin(TemplateCompilePhase phase, string methodName) {
            this.methodName = methodName;
            this.phase = phase;
        }

    }

    [Template(TemplateType.Internal, "Elements/Select.xml")]
    [TemplateCompilePlugin(TemplateCompilePhase.BeforeChildren, nameof(BeforeCompileChildren))]
    [TemplateCompilePlugin(TemplateCompilePhase.AfterChildren, nameof(AfterCompileChildren))]
    public class Select<T> : UIElement {

        public int selectedIndex { get; protected set; }
        public T defaultValue { get; set; }

        public T currentValue { get; }

        public RepeatableList<ISelectOption<T>> options;
        private RepeatableList<ISelectOption<T>> previousOptions;
        private Action<ISelectOption<T>, int> onInsert;

        public bool selecting;
        private UIChildrenElement childrenElement;

        [OnPropertyChanged(nameof(options))]
        private void OnSelectionChanged(string propertyName) {
            if (previousOptions != options) {
                if (previousOptions != null) {
                    previousOptions.onItemInserted -= onInsert;
                }
            }
            
            if (options != null) {
                options.onItemInserted += onInsert;
                for (int i = 0; i < options.Count; i++) {
                    childrenElement.AddChild(childrenElement.InstantiateTemplate());
                }
            }

            Debug.Log(propertyName + " Selection set to: " + selectedIndex);
        }

        private void OnInsert(ISelectOption<T> option, int index) {
            childrenElement.InsertChild((uint)index, childrenElement.InstantiateTemplate());
        }

        public override void OnCreate() {
            onInsert = OnInsert;
            childrenElement = FindFirstByType<UIChildrenElement>();
        }

        public static void BeforeCompileChildren(ExpressionCompiler compiler) {
            compiler.AddAliasResolver(new SelectOptionAliasResolver<T>("$option"));
        }
        
        public static void BeforeCompileChild(ParsedTemplate template) {
            template.compiler.AddAliasResolver(new RepeatItemAliasResolver("$item", typeof(T)));
        }
        
        public static void AfterCompileChildren(ParsedTemplate template) {
            template.compiler.RemoveAliasResolver("$item");
        }

    }

}