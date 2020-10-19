using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace SeedLib {

    [Template("SeedLib/ContextMenu/ContextMenu.xml#item")]
    public class ContextMenuItem : UIElement {

        public ContextMenuItemData item;

        [OnMouseEnter]
        public void MouseEnter() {
            SetAttribute("mouse-over", "true");
        }

        [OnMouseExit]
        public void MouseExit(MouseInputEvent evt) {
            Rect rect = layoutResult.ScreenRect;

            // if exiting to the horizontal axis we want to retain hover state
            // todo -- need a controller to handle selection path per level
            // todo -- emit enter/exit events to better manage selection state
            if (!(evt.MousePosition.y >= rect.y) || !(evt.MousePosition.y <= rect.yMax)) {
                SetAttribute("mouse-over", "false");
            }
            
        }

    }

    [Template("SeedLib/ContextMenu/ContextMenu.xml#group")]
    public class ContextMenuItemGroup : UIElement {

        public IList<ContextMenuItemData> items;

    }

}