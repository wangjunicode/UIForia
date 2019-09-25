using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace Demo {
    
    public class MenuItemData {
        public Action OnClick;
        public string ImageUrl;
        public string Label;
        public int NotificationCount;
    }

    [Template("Demo/Dock/Dock.xml")]
    public class Dock : UIElement {
        
        public RepeatableList<MenuItemData> MenuItems;

        public bool isActive;
        private bool showParticles;

        [OnPropertyChanged("isActive")]
        public void OnActiveChanged(string propertyName) {
            SetAttribute("placement", isActive ? "show" : "hide");
        }

        public void ToggleParticles() {
            showParticles = !showParticles;
        }
        
        public override void OnCreate() {
            MenuItems = new RepeatableList<MenuItemData>() {
                    new MenuItemData() { ImageUrl = "dock/Zones@2x", Label = "Zones" },
                    new MenuItemData() { ImageUrl = "dock/Wiki@2x", Label = "Wiki" },
                    new MenuItemData() { ImageUrl = "dock/Schedule@2x", Label = "Schedule" },
                    new MenuItemData() { ImageUrl = "dock/Objects@2x", Label = "Objects" },
                    new MenuItemData() { ImageUrl = "dock/ActivityLog@2x", Label = "Activity Log" },
                    new MenuItemData() { ImageUrl = "dock/Chat@2x", Label = "Chat", NotificationCount = 3, OnClick = OpenChat},
                    new MenuItemData() { ImageUrl = "dock/Construction@2x", Label = "Construction", OnClick = OpenBuildMenu },
                    new MenuItemData() { ImageUrl = "dock/Farming@2x", Label = "Farming" },
            };
        }

        private void OpenBuildMenu() {
            TriggerEvent(new UIPanelEvent(UIPanel.Building));
        }

        private void OpenChat() {
            TriggerEvent(new UIWindowEvent(UIWindow.Chat));
        }

        public void EnterMode(MenuItemData item) {
            item.OnClick?.Invoke();
        }
    }
}
