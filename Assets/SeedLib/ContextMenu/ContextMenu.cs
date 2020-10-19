using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    public class ContextMenuTree {

        public string header;
        public IList<IList<ContextMenuItemData>> groups;

    }

    public struct ContextMenuItemData {

        public ImageLocator icon;
        public string label;
        public IList<ContextMenuItemData> subMenu;
        public Action clickHandler;

    }

    public class ContextSubMenuBuilder {

        internal List<ContextMenuItemData> menu;

        internal ContextSubMenuBuilder() {
            this.menu = new List<ContextMenuItemData>();
        }

        public void AddMenuItem(string label, ImageLocator icon, Action<ContextSubMenuBuilder> subMenu = null) {
            if (subMenu == null) {
                return;
            }

            ContextSubMenuBuilder childBuilder = new ContextSubMenuBuilder();

            subMenu.Invoke(childBuilder);

            menu.Add(new ContextMenuItemData() {
                label = label,
                icon = icon,
                clickHandler = null,
                subMenu = childBuilder.menu
            });
        }

        public void AddMenuItem(string label, ImageLocator icon, Action onClick) {
            menu.Add(new ContextMenuItemData() {
                label = label,
                icon = icon,
                clickHandler = onClick
            });
        }

    }

    public class ContextMenuBuilder {

        private ContextMenuTree tree;

        internal ContextMenuBuilder(ContextMenuTree tree) {
            this.tree = tree;
        }

        public void SetHeader(string label) {
            tree.header = label;
        }

        public void AddMenuGroup(Action<ContextSubMenuBuilder> group) {
            if (group == null) {
                return;
            }

            ContextSubMenuBuilder builder = new ContextSubMenuBuilder();

            group.Invoke(builder);

            List<ContextMenuItemData> items = builder.menu;
            tree.groups = tree.groups ?? new List<IList<ContextMenuItemData>>();
            tree.groups.Add(items);
        }

    }

    public class FakeContextData {

        public string label;

    }

    [Template("SeedLib/ContextMenu/ContextMenu.xml")]
    public class ContextMenu : UIElement {

        public bool HasHeader => !string.IsNullOrEmpty(registry.GetMenuHeader());

        public ContextMenuRegistry registry;

    }

}