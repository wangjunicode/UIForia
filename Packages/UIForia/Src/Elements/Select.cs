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

        private float debounce;
        
        public int selectedIndex = -1;

        public int keyboardNavigationIndex = -1;

        public T defaultValue { get; set; }

        public T selectedValue;

        public string selectedElementIcon;

        public bool disabled;

        public bool disableOverflowX;
        public bool disableOverflowY;

        private UIElement clippingElement;

        public RepeatableList<ISelectOption<T>> options;
        private RepeatableList<ISelectOption<T>> previousOptions;
        private Action<ISelectOption<T>, int> onInsert;
        private Action<ISelectOption<T>, int> onRemove;
        private Action onClear;

        public bool selecting = false;
        internal UIChildrenElement childrenElement;
        internal ScrollView optionList;

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

            Application.InputSystem.RegisterFocusable(this);

            UIElement potentialClippingParent = parent;
            while (clippingElement == null) {
                if (potentialClippingParent == View.RootElement || potentialClippingParent.style.OverflowY != Overflow.Visible) {
                    clippingElement = potentialClippingParent;
                }

                potentialClippingParent = potentialClippingParent.parent;
            }

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

        private void SetSelectedValue(int index) {
            selecting = false;
            selectedIndex = index;
            selectedValue = options[selectedIndex].Value;
            onValueChanged?.Invoke(selectedValue);
            onIndexChanged?.Invoke(selectedIndex);
        }

        [OnKeyDownWithFocus()]
        public void OnKeyDownNavigate(KeyboardInputEvent evt) {
            
            if (selecting && evt.keyCode == KeyCode.Escape) {
                selecting = false;
                return;
            }

            if (selecting && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)) {
                
                // space and return should only choose the currently keyboard-selected item
                if (keyboardNavigationIndex > -1) {
                    SetSelectedValue(keyboardNavigationIndex);
                    childrenElement.children[selectedIndex].style.ExitState(StyleState.Hover);
                }
                return;
            }
            
            // just submit the form if we have focus and are not in selection mode
            if (!selecting && evt.keyCode == KeyCode.Return) {
                Input.DelayEvent(this, new SubmitEvent());
                return;
            }
            
            // enable tab navigation
            if (evt.keyCode == KeyCode.Tab) {
                Input.DelayEvent(this, new TabNavigationEvent(evt));
                return;
            }

            if (!selecting) {
                // enter selection mode if we have focus and press space
                if (evt.keyCode == KeyCode.Space) {
                    selecting = true;

                    keyboardNavigationIndex = selectedIndex;
                    if (keyboardNavigationIndex == -1) {
                        keyboardNavigationIndex = 0;
                    }

                    childrenElement.children[keyboardNavigationIndex].style.EnterState(StyleState.Hover);
                }
            }
        }
        
        [OnKeyHeldDownWithFocus()]
        public void OnKeyboardNavigate(KeyboardInputEvent evt) {

            if (debounce - Time.realtimeSinceStartup > -0.1) {
                return;
            }

            debounce = Time.realtimeSinceStartup;
            
            if (!selecting) {
                // if we are NOT in selection mode using the arrow keys should just cycle through the options and set them immediately
                if (evt.keyCode == KeyCode.UpArrow) {
                    selectedIndex--;
                    if (selectedIndex < 0) {
                        selectedIndex = options.Count - 1;
                    }

                    SetSelectedValue(selectedIndex);
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.DownArrow) {
                    selectedIndex++;
                    if (selectedIndex == options.Count) {
                        selectedIndex = 0;
                    }

                    SetSelectedValue(selectedIndex);
                    evt.StopPropagation();
                }
            }
            else {
                // use the up/down arrows to navigate through the options but only visually. pressing space or return will set the new value for real.
                
                if (evt.keyCode == KeyCode.UpArrow) {
                    if (keyboardNavigationIndex > -1) {
                        childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
                    }
                    keyboardNavigationIndex--;
                    if (keyboardNavigationIndex < 0) {
                        keyboardNavigationIndex = options.Count - 1;
                    }
                    ScrollElementIntoView();
                    evt.StopPropagation();
                } 
                else if (evt.keyCode == KeyCode.DownArrow) {
                    if (keyboardNavigationIndex > -1) {
                        childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
                    }
                    keyboardNavigationIndex++;
                    if (keyboardNavigationIndex == options.Count) {
                        keyboardNavigationIndex = 0;
                    }
                    ScrollElementIntoView();
                    evt.StopPropagation();
                }
            }
        }

        private void ScrollElementIntoView() {
            if (keyboardNavigationIndex < 0 || keyboardNavigationIndex >= childrenElement.children.Count) {
                return;
            }
            UIElement element = childrenElement.children[keyboardNavigationIndex];
            element.style.EnterState(StyleState.Hover);
            float localPositionY = element.layoutResult.localPosition.y;
            float elementHeight = element.layoutResult.ActualHeight;
            float elementBottom = localPositionY + elementHeight;

            float trackHeight = optionList.layoutResult.ActualHeight;
            float scrollViewHeight = optionList.layoutResult.AllocatedHeight;
            float minY = childrenElement.children[0].layoutResult.localPosition.y;
            
            if (localPositionY >= 0 && elementBottom < scrollViewHeight) {
                return;
            }

            if (localPositionY < 0) {
                // scrolls up to the upper edge of the element
                float normalizedScrollY = (localPositionY - minY) / (trackHeight - scrollViewHeight);
                element.TriggerEvent(new UIScrollEvent(0, normalizedScrollY));
            }
            else {
                // scrolls down but keeps the element at the lower edge of the scrollView
                float normalizedScrollY = (elementBottom - scrollViewHeight - minY) / (trackHeight - scrollViewHeight);
                element.TriggerEvent(new UIScrollEvent(0, normalizedScrollY));
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
            float maxOffset = layoutResult.screenPosition.y - clippingElement.layoutResult.screenPosition.y;
            float minOffset = optionList.layoutResult.screenPosition.y - optionList.style.TransformPositionY.value + optionList.layoutResult.AllocatedHeight - (clippingElement.layoutResult.screenPosition.y + clippingElement.layoutResult.AllocatedHeight);
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