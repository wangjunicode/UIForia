using System.Collections.Generic;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UnityEngine;

namespace SeedLib {

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

    public class EnumSelectOption<T> : SelectOption<T> where T : struct {
        public EnumSelectOption(T value) : base (value.ToString(), value) { }
    }

    [Template("SeedLib/Select/Select.xml")]
    public class Select<T> : UIElement, IFocusable {

        private const string disabledAttributeValue = "select-disabled";

        private float debounce;

        public int selectedIndex { get; private set; } = -1;

        public int keyboardNavigationIndex = -1;

        public T defaultValue;

        public T selectedValue;

        public ImageLocator selectedElementIcon = new ImageLocator("Icons/checkmark");

        public bool disabled;

        public bool disableOverflowX = true;
        public bool disableOverflowY;

        public IList<ISelectOption<T>> options;

        public bool selecting;
        internal UIChildrenElement childrenElement;
        internal UIElement optionList;
        internal UIElement repeat;
        internal UIElement chevronHolderElement;

        public bool validSelection => options != null && selectedIndex >= 0 && selectedIndex < options.Count;

        private bool reselect;

        [OnPropertyChanged(nameof(options))]
        public void OnSelectionChanged() {
            reselect = true;
        }

        [OnPropertyChanged(nameof(selectedValue))]
        public void OnSelectedValueChanged() {
            reselect = true;
        }

        public override void OnCreate() {
            childrenElement = FindById<UIChildrenElement>("option-children");
            chevronHolderElement = FindById<UIGroupElement>("chevron-holder");
            optionList = this["option-list"];
            repeat = this["repeated-options"];
            application.InputSystem.RegisterFocusable(this);
        }

        public override void OnEnable() {
            TryGetAttribute("variant", out string variant);
            chevronHolderElement.SetAttributeRecursive("variant", variant);
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            if (attrName == "variant") {
                TryGetAttribute("variant", out string variant);
                chevronHolderElement.SetAttributeRecursive("variant", variant);
            }
        }

        public override void OnUpdate() {
            UpdateSelection();

            if (!disabled && HasAttribute("disabled")) {
                SetAttribute("disabled", null);
                EnableAllChildren(this);
            }
            else if (disabled && !HasAttribute("disabled")) {
                SetAttribute("disabled", disabledAttributeValue);
                DisableAllChildren(this);
            }

            // optionList.style.SetVisibility(selecting ? Visibility.Visible : Visibility.Hidden, StyleState.Normal);
        }

        private void UpdateSelection() {
            if (!reselect) {
                return;
            }

            reselect = false;

            if (options == null) {
                selectedIndex = -1;
                selectedValue = defaultValue;
                return;
            }

            if (selectedValue == null && defaultValue != null) {
                selectedValue = defaultValue;
            }

            for (int i = 0; i < options.Count; i++) {
                if (options[i].Value.Equals(selectedValue)) {
                    if (selectedIndex != i) {
                        selectedIndex = i;
                        selectedValue = options[selectedIndex].Value;
                    }

                    return;
                }
            }
            
            selectedValue = default;
            selectedIndex = -1;
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
                    // TODO(roman): Fix keyboard navigation.
                    //childrenElement.children[selectedIndex].style.ExitState(StyleState.Hover);
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

                    //childrenElement.children[keyboardNavigationIndex].style.EnterState(StyleState.Hover);
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
                        //childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
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
                        //childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
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
                //childrenElement.children[keyboardNavigationIndex].style.ExitState(StyleState.Hover);
            }
        }

        public void AdjustOptionPosition() {
            if (validSelection) {

                application.RegisterBeforeUpdateTask(new CallbackTaskNoArg(() => {
                    if (!isEnabled || !validSelection) {
                        return UITaskResult.Completed;
                    }

                    if (options == null || options.Count == 0) {
                        return UITaskResult.Completed;
                    }

                    if (repeat.ChildCount == 0) {
                        return UITaskResult.Running;
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
            return selectedElementIcon.isValid && selectedIndex == index;
        }

    }

}