using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Parsing.Expression;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Elements {

    public interface ISelectOption<out T> {

        string Label { get; }
        T Value { get; }

    }

    [Template(TemplateType.Internal, "Elements/Select.xml")]
    public class Select<T> : UIElement, IFocusable {

        public int selectedIndex;
        public T defaultValue { get; set; }

        public T selectedValue;

        public RepeatableList<ISelectOption<T>> options;
        private RepeatableList<ISelectOption<T>> previousOptions;
        private Action<ISelectOption<T>, int> onInsert;

        public bool selecting = false;
        internal UIChildrenElement childrenElement;

        [WriteBinding(nameof(selectedValue))]
        public event Action<T> onValueChanged;

        [WriteBinding(nameof(selectedIndex))]
        public event Action<int> onIndexChanged;

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

                if (selectedIndex == -1) {
                    for (int i = 0; i < options.Count; i++) {
                        if (options[i].Value.Equals(selectedValue)) {
                            selectedIndex = i;
                            selectedValue = options[selectedIndex].Value;
                            onIndexChanged?.Invoke(selectedIndex);
                            onValueChanged?.Invoke(selectedValue);
                            return;
                        }
                    }
                }
            }
        }

        [OnPropertyChanged(nameof(selectedValue))]
        private void OnSelectedValueChanged(string propertyName) {
            if (options == null) return;
            for (int i = 0; i < options.Count; i++) {
                if (options[i].Value.Equals(selectedValue)) {
                    if (selectedIndex != i) {
                        selectedIndex = i;
                        selectedValue = options[selectedIndex].Value;
                        onValueChanged?.Invoke(selectedValue);
                        onIndexChanged?.Invoke(selectedIndex);
                    }

                    return;
                }
            }

            selectedIndex = -1;
        }

        [OnPropertyChanged(nameof(selectedIndex))]
        private void OnSelectedIndexChanged(string propertyName) {
            if (options == null) return;

            if (selectedIndex < 0 || selectedIndex >= options.Count) {
                selectedIndex = -1;
                selectedValue = defaultValue;
            }
            else {
                selectedValue = options[selectedIndex].Value;
            }

            onIndexChanged?.Invoke(selectedIndex);
            onValueChanged?.Invoke(selectedValue);
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

        public void SelectElement(MouseInputEvent evt) {
            UIElement[] childrenArray = childrenElement.children.Array;
            int count = childrenElement.children.Count;
            for (int i = 0; i < count; i++) {
                if (childrenArray[i].layoutResult.ScreenRect.Contains(evt.MousePosition)) {
                    selectedIndex = i;
                    selectedValue = options[selectedIndex].Value;
                    onValueChanged?.Invoke(selectedValue);
                    onIndexChanged?.Invoke(selectedIndex);
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
            resolvers.Add(new SelectOptionAliasResolver<T>("$option", false));
            resolvers.Add(new SelectOptionAliasResolver<T>("$option__internal", true));
        }

        public void Focus() { }

        public void Blur() {
            selecting = false;
        }

    }

}