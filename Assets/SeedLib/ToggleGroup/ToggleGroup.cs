using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {
    
    public interface ISelectable {
    }

    [Template("SeedLib/ToggleGroup/ToggleGroup.xml")]
    public class ToggleGroup : UIElement {

        public List<int> selectedIndices = new List<int>();
        public bool multiSelectMode;

        protected bool allowDeselect;

        public override void OnEnable() {
            string variant = GetAttribute("variant");
            switch (variant) {
                case "primary":
                    break;
                case "secondary":
                    break;
                default:
                    SetAttribute("variant", "primary");
                    variant = "primary";
                    break;
            }
            
            if (children.Count > 0) {
                selectedIndices.Add(0);
            }
        }

        [OnMouseClick(EventPhase.Capture)]
        public void OnMouseClick(MouseInputEvent evt) {
            evt.StopPropagation();
            
            ISelectable item = evt.Origin.FindNextWithInterface<ISelectable>();
            var itemSiblingIndex = ((UIElement) item).siblingIndex;
            
            if (!multiSelectMode) {
                int? selectedIndex = selectedIndices.Count > 0 ? selectedIndices[0] : (int?) null;
                if (!allowDeselect && IsSelected(itemSiblingIndex)) {
                    return;
                }
                
                selectedIndices.Clear();

                if (!selectedIndex.HasValue || selectedIndex.Value != itemSiblingIndex) {
                    selectedIndices.Add(itemSiblingIndex);
                }
            }
            else {
                if (!IsSelected(itemSiblingIndex)) {
                    selectedIndices.Add(itemSiblingIndex);
                }
                else {
                    selectedIndices.Remove(itemSiblingIndex);
                }
            }
        }

        internal bool IsSelected(int siblingIndex) {
            for (int i = 0; i < selectedIndices.Count; ++i) {
                if (selectedIndices[i] == siblingIndex) {
                    return true;
                }
            }

            return false;
        }
    }
    
    [Template("SeedLib/ToggleGroup/ToggleGroup.xml#item")]
    public class ToggleGroupItem : UIElement, ISelectable {

        public bool selected;
        public string text;

        private ToggleGroup toggleGroup;
        private string variant;

        public override void OnEnable() {
            toggleGroup = FindParent<ToggleGroup>();
            if (toggleGroup == null) {
                throw new Exception(@"{nameof(ToggleGroupItem)} must only be used inside of {nameof(ToggleGroup)} element");
            }

            variant = toggleGroup.GetAttribute("variant");
            SetAttribute("variant", variant);
        }

        public override void OnUpdate() {
            if (siblingIndex == 0) {
                SetAttribute("round-left", "true");
            }
            else {
                SetAttribute("round-left", null);
            }

            if (siblingIndex == parent.children.size - 1) {
                SetAttribute("round-right", "true");
            }
            else {
                SetAttribute("round-right", null);
            }

            selected = toggleGroup.IsSelected(siblingIndex);
            if (selected) {
                SetAttribute("selected", variant);
            }
            else {
                SetAttribute("selected", null);
            }
        }
    }
    
}