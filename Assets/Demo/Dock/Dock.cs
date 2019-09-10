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

    public class DockEvent : UIEvent {

        public readonly UIPanel Panel;
        
        public DockEvent(UIPanel panel) : base("dockEvent") {
            this.Panel = panel;
        }

        public DockEvent(UIPanel panel, KeyboardInputEvent keyboardInputEvent) : base("dockEvent", keyboardInputEvent) {
            this.Panel = panel;
        }
    }

    [Template("Demo/Dock/Dock.xml")]
    public class Dock : UIElement {
        
        public RepeatableList<MenuItemData> MenuItems;

        public override void OnCreate() {
            MenuItems = new RepeatableList<MenuItemData>() {
                    new MenuItemData() { ImageUrl = "Prototyping/icon__menu_chat", Label = "Chat", NotificationCount = 3 },
                    new MenuItemData() { ImageUrl = "Prototyping/icon_plant_64", Label = "Plants" },
                    new MenuItemData() { ImageUrl = "Prototyping/icon__menu_build", Label = "Build things", OnClick = OpenBuildMenu},
                    new MenuItemData() { ImageUrl = "Prototyping/icon_chop_64", Label = "Chop the wood" },
                    new MenuItemData() { ImageUrl = "Prototyping/icon__menu_schedule", Label = "Schedules" },
                    new MenuItemData() { ImageUrl = "Prototyping/icon__menu_stockpile", Label = "More things" },
                    new MenuItemData() { ImageUrl = "Prototyping/icon__menu_build", Label = "Wiki and help" }
            };
        }

        private void OpenBuildMenu() {
            TriggerEvent(new DockEvent(UIPanel.Building));
        }

        public void EnterMode(MenuItemData item) {
            item.OnClick?.Invoke();
        }
    }
}
