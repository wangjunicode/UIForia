using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    public interface ISelectOption<out T> {

        string Label { get; }
        T Value { get; }

    }

    public class SelectOption<T> : ISelectOption<T> {

        public string Label { get; set; }
        public T Value { get; set; }

        public SelectOption(string label, T value) {
            this.Label = label;
            this.Value = value;
        }

    }

    [Template(TemplateType.Internal, "Elements/Select.xml")]
    public class Select<T> : UIElement, IFocusable {

        private const string disabledAttributeValue = "select-disabled";

        private float debounce;

        public int selectedIndex = -1;

        public int keyboardNavigationIndex = -1;

        public T defaultValue { get; set; }

        public T selectedValue;

        public string selectedElementIcon = "icons/ui_icon_popover_checkmark@2x";

        public bool disabled;

        public bool disableOverflowX;
        public bool disableOverflowY;

        public IList<ISelectOption<T>> options;

        public bool selecting;
        internal UIChildrenElement childrenElement;
        internal UIElement optionList;
        internal UIElement repeat;

        public event Action<T> onValueChanged;

        public event Action<int> onIndexChanged;

        public bool validSelection => options != null && selectedIndex >= 0 && selectedIndex < options.Count;

        [OnPropertyChanged(nameof(options))]
        public void OnSelectionChanged() {
            // if (previousOptions != options) {
            //     if (previousOptions != null) {
            //     }
            // }
            //
            // if (options != null) {
            //     options.onItemInserted += onInsert;
            //     options.onItemRemoved += onRemove;
            //     options.onClear += onClear;
            //     for (int i = 0; i < options.Count; i++) {
            //         childrenElement.AddChild(childrenElement.InstantiateTemplate());
            //     }
            //
            //     if (selectedIndex == -1) {
            //         for (int i = 0; i < options.Count; i++) {
            //             if (options[i].Value.Equals(selectedValue)) {
            //                 selectedIndex = i;
            //                 selectedValue = options[selectedIndex].Value;
            //                 onIndexChanged?.Invoke(selectedIndex);
            //                 onValueChanged?.Invoke(selectedValue);
            //                 return;
            //             }
            //         }
            //     }
            // }
        }

        [OnPropertyChanged(nameof(selectedValue))]
        public void OnSelectedValueChanged() {
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
        public void OnSelectedIndexChanged() {
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

        public override void OnCreate() {
            childrenElement = FindById<UIChildrenElement>("option-children");
            optionList = this["option-list"];
            repeat = this["repeated-options"];
            application.InputSystem.RegisterFocusable(this);
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

            if (selecting) { }

            // optionList.style.SetVisibility(selecting ? Visibility.Visible : Visibility.Hidden, StyleState.Normal);
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
                    EnableAllChildren(child);
                }
            }
        }

        private void SetSelectedValue(int index) {
            selecting = false;
            selectedIndex = index;
            selectedValue = options[selectedIndex].Value;
            onValueChanged?.Invoke(selectedValue);
            onIndexChanged?.Invoke(selectedIndex);
        }

        [OnKeyDownWithFocus]
        public void OnKeyDownNavigate(KeyboardInputEvent evt) {
            if (disabled) {
                return;
            }

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
            if (disabled) {
                return;
            }

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
            if (keyboardNavigationIndex < 0 || keyboardNavigationIndex >= options.Count) {
                return;
            }

            // repeat[keyboardNavigationIndex].ScrollIntoView();
        }

        [OnMouseClick]
        public void BeginSelecting(MouseInputEvent evt) {
            if (disabled) {
                return;
            }

            if (selecting) {
                InputSystem.ReleaseFocus(this);
                selecting = false;
            }
            else {
                selecting = InputSystem.RequestFocus(this);
            }

            AdjustOptionPosition();

            evt.StopPropagation();
            evt.Consume();
        }

        [OnMouseMove]
        public void OnMouseMove() {
            if (keyboardNavigationIndex > 0) {
                childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
            }
        }

        public void AdjustOptionPosition() {
            if (validSelection) {
                // repeat[selectedIndex].ScrollIntoView();

                application.RegisterBeforeUpdateTask(new CallbackTaskNoArg(() => {
                    
                    if (!isEnabled || !validSelection) {
                        return UITaskResult.Completed;
                    }
                    
                    float y = repeat[selectedIndex].layoutResult.alignedPosition.y
                              - repeat.FindParent<ScrollView>().ScrollOffsetY
                              - layoutResult.VerticalPaddingBorderStart
                              + optionList.layoutResult.VerticalPaddingBorderStart;
                    optionList.style.SetAlignmentOriginY(new OffsetMeasurement(-y), StyleState.Normal);
                    return UITaskResult.Completed;
                }));
            }
            else {
                optionList.style.SetAlignmentOriginY(null, StyleState.Normal);
            }
        }

        public void SelectElement(MouseInputEvent evt, int index) {
            selectedIndex = index;
            selectedValue = options[selectedIndex].Value;
            onValueChanged?.Invoke(selectedValue);
            onIndexChanged?.Invoke(selectedIndex);
            selecting = false;
            evt.StopPropagation();
            evt.Consume();
        }

        public bool Focus() {
            return GetAttribute("disabled") == null;
        }

        public void Blur() {
            selecting = false;
        }

        public bool DisplaySelectedIcon(int index) {
            return selectedElementIcon != null && selectedIndex == index;
        }

    }

}