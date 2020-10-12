using System;
using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Notification/Notification.xml")]
    public class Notification : UIElement {

        public bool isClosable => onClose != null;
        public bool isActionable => onActionClicked != null;

        public string title;
        public string description;
        
        public ImageLocator actionIcon;
        public ImageLocator avatarIcon;

        public Action onClose;
        public Action onActionClicked;

        public override void OnEnable() {
            OnSetAttribute("variant", GetAttribute("variant"), null);
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            if (attrName == "variant") {
                for (int i = 0; i < children.size; i++) {
                    SetAttribute(children[i], attrName, newValue);
                }
            }
        }

        private void SetAttribute(UIElement element, string attrName, string newValue) {
            element.SetAttribute(attrName, newValue);
            for (int i = 0; i < element.children.size; i++) {
                SetAttribute(element.children[i], attrName, newValue);
            }
        }

    }

}