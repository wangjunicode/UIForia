using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Rendering;
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

        private const string disabledAttributeValue = "select-disabled";
        
        public int selectedIndex = -1;
        public T defaultValue { get; set; }

        public T selectedValue;

        public string selectedElementIcon;

        public bool disabled;

        public RepeatableList<ISelectOption<T>> options;
        private RepeatableList<ISelectOption<T>> previousOptions;
        private Action<ISelectOption<T>, int> onInsert;
        private Action<ISelectOption<T>, int> onRemove;
        private Action onClear;

        public bool selecting = false;
        internal UIChildrenElement childrenElement;
        internal UIElement optionList;

        [WriteBinding(nameof(selectedValue))]
        public event Action<T> onValueChanged;

        [WriteBinding(nameof(selectedIndex))]
        public event Action<int> onIndexChanged;

        [OnPropertyChanged(nameof(options))]
        private void OnSelectionChanged(string propertyName) {
            if (previousOptions != options) {
                if (previousOptions != null) {
                    previousOptions.onItemInserted -= onInsert;
                    options.onItemRemoved -= onRemove;
                    options.onClear -= onClear;
                }
            }

            if (options != null) {
                options.onItemInserted += onInsert;
                options.onItemRemoved += onRemove;
                options.onClear += onClear;
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
            onClear = OnClear;
            onRemove = OnRemove;
            childrenElement = FindById<UIChildrenElement>("option-children");
            optionList = FindById<ScrollView>("option-list");
            if (disabled) {
                SetAttribute("disabled", disabledAttributeValue);
                DisableAllChildren(this);
            } else if (GetAttribute("disabled") != null) {
                DisableAllChildren(this);
            }
        }

        private void DisableAllChildren(UIElement element) {
            for (int index = 0; index < element.children.Count; index++) {
                UIElement child = element.children[index];
                if (!child.HasAttribute("disabled")) {
                    child.SetAttribute("disabled", disabledAttributeValue);
                    DisableAllChildren(child);
                }
            }
        }
        private void EnableAllChildren(UIElement element) {
            for (int index = 0; index < element.children.Count; index++) {
                UIElement child = element.children[index];
                if (child.GetAttribute("disabled") == disabledAttributeValue) {
                    child.SetAttribute("disabled", null);
                    DisableAllChildren(child);
                }
            }
        }

        public override void OnUpdate() {
            if (!disabled && HasAttribute("disabled")) {
                SetAttribute("disabled", null);
                EnableAllChildren(this);
            } 
            else if (disabled && !HasAttribute("disabled")) {
                SetAttribute("disabled", disabledAttributeValue);
                DisableAllChildren(this);
            }

            if (selecting) {
                AdjustOptionPosition();
            }
        }

        public void BeginSelecting(MouseInputEvent evt) {
            if (HasAttribute("disabled")) {
                return;
            }

            if (selecting) {
                Application.InputSystem.ReleaseFocus(this);
                selecting = false;
            }
            else {
                Application.InputSystem.RequestFocus(this);
                selecting = true;
            }

            evt.StopPropagation();
            evt.Consume();
        }

        public void AdjustOptionPosition() {
            float offset = 0;
            float maxOffset = layoutResult.screenPosition.y;
            float minOffset = optionList.layoutResult.screenPosition.y - optionList.style.TransformPositionY.value + optionList.layoutResult.AllocatedHeight - Screen.height;
            UIElement[] childrenArray = childrenElement.children.Array;
            for (int i = 0; i < selectedIndex; i++) {
                offset += childrenArray[i].layoutResult.ActualHeight;
            }

            optionList.style.SetTransformPositionY(-Math.Min(maxOffset, Math.Max(offset, minOffset)), StyleState.Normal);
            optionList.style.SetTransformBehaviorY(TransformBehavior.AnchorMinOffset, StyleState.Normal);
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
            resolvers.Add(new SelectOptionAliasResolver<T>("$option", false));
            resolvers.Add(new SelectOptionAliasResolver<T>("$option__internal", true));
        }

        public bool Focus() {
            return GetAttribute("disabled") == null;
        }

        public void Blur() {
            selecting = false;
        }

        public override void OnDestroy() {
            if (options != null) {
                options.onItemInserted -= onInsert;
                options.onItemRemoved -= onRemove;
                options.onClear -= onClear;
            }
        }

        public bool DisplaySelectedIcon(ISelectOption<T> option) {
            return selectedElementIcon != null && selectedIndex > -1 && (options[selectedIndex].Label == option.Label);
        }

        private void OnClear() {
            Application.DestroyChildren(childrenElement);
        }

        private void OnRemove(ISelectOption<T> selectOption, int index) {
            childrenElement.children[index].Destroy();
        }
    }

}