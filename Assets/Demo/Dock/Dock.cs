using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;

namespace Demo {
    
    public class MenuItemData {
        public Action OnClick;
        public string ImageUrl;
        public string Label;
        public int NotificationCount;
    }

    public class UIPanelEvent : UIEvent {

        public readonly UIPanel Panel;
        
        public UIPanelEvent(UIPanel panel) : base("panelEvent") {
            this.Panel = panel;
        }

        public UIPanelEvent(UIPanel panel, KeyboardInputEvent keyboardInputEvent) : base("panelEvent", keyboardInputEvent) {
            this.Panel = panel;
        }
    }

    [Template("Demo/Dock/Dock.xml")]
    public class Dock : UIElement {
        
        public RepeatableList<MenuItemData> MenuItems;

        public bool isActive;

        [OnPropertyChanged("isActive")]
        public void OnActiveChanged(string propertyName) {
            SetAttribute("placement", isActive ? "show" : "hide");
        }

        public override void OnCreate() {
            MenuItems = new RepeatableList<MenuItemData>() {
                    new MenuItemData() { ImageUrl = "dock/Zones@2x", Label = "Zones" },
                    new MenuItemData() { ImageUrl = "dock/Wiki@2x", Label = "Wiki" },
                    new MenuItemData() { ImageUrl = "dock/Schedule@2x", Label = "Schedule" },
                    new MenuItemData() { ImageUrl = "dock/Objects@2x", Label = "Objects" },
                    new MenuItemData() { ImageUrl = "dock/ActivityLog@2x", Label = "Activity Log" },
                    new MenuItemData() { ImageUrl = "dock/Chat@2x", Label = "Chat", NotificationCount = 3 },
                    new MenuItemData() { ImageUrl = "dock/Construction@2x", Label = "Construction", OnClick = OpenBuildMenu },
                    new MenuItemData() { ImageUrl = "dock/Farming@2x", Label = "Farming" },
            };
        }

        private void OpenBuildMenu() {
            TriggerEvent(new UIPanelEvent(UIPanel.Building));
        }

        public void EnterMode(MenuItemData item) {
            item.OnClick?.Invoke();
        }
    }
}
