using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Parsing.Expression;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {
    

    public interface ISelectOption<out T> {

        string Label { get; }
        T Value { get; }

    }

    [Template(TemplateType.Internal, "Elements/Select.xml")]
    public class Select<T> : UIElement, IFocusable {

        public int selectedIndex { get; protected set; }
        public T defaultValue { get; set; }

        public T currentValue { get; }

        public RepeatableList<ISelectOption<T>> options;
        private RepeatableList<ISelectOption<T>> previousOptions;
        private Action<ISelectOption<T>, int> onInsert;

        public bool selecting = false;
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

        }

      
        private void OnInsert(ISelectOption<T> option, int index) {
            childrenElement.InsertChild((uint) index, childrenElement.InstantiateTemplate());
        }

        public override void OnCreate() {
            onInsert = OnInsert;
            childrenElement = FindFirstByType<UIChildrenElement>();
        }

        private void BeginSelecting(MouseInputEvent evt) {
            if (Application.InputSystem.RequestFocus(this)) {
                selecting = true;
            }

            evt.StopPropagation();
            evt.Consume();
        }

        private void SelectElement(MouseInputEvent evt) {
            UIElement[] childrenArray = childrenElement.children.Array;
            int count = childrenElement.children.Count;
            for (int i = 0; i < count; i++) {
                if (childrenArray[i].layoutResult.ScreenRect.Contains(evt.MousePosition)) {
                    selectedIndex = i;
                    break;
                }
            }

            selecting = false;
            evt.StopPropagation();
            evt.Consume();
        }

        private static void GetAliasResolvers(IList<ExpressionAliasResolver> resolvers, AttributeList attributes) {
            AttributeDefinition aliasAttr = attributes.GetAttribute("optionAlias");
            string optionName = aliasAttr?.value ?? "$option";
            resolvers.Add(new SelectOptionAliasResolver<T>("$option"));
        }

        public void Focus() { }

        public void Blur() {
            selecting = false;
        }

    }

}