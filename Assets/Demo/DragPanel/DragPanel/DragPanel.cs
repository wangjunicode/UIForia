using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace Demo.DragPanel.DragPanel {

    [Template("DragPanel/DragPanel/DragPanel.xml")]
    public class DragPanel<T> : UIElement {

        public List<T> items;

        public DragEvent BeginDrag() {
            return new DragPanelDrag();
        }

        public void DragUpdate(DragEvent evt) {
            Debug.Log("updating");
        }

        public void DragEnter(DragPanelDrag evt) {
            float top = evt.element.layoutResult.screenPosition.y;
            float height = evt.element.layoutResult.actualSize.height;
            if (evt.MousePosition.y < top + (height * 0.5f)) {
                evt.element.SetAttribute("drag-over-top", "true");
            }
            else {
                evt.element.SetAttribute("drag-over-bottom", "true");
            }
        }

        public void DragExit(DragPanelDrag evt) {
            // evt.element.SetAttribute("drag-over-top", null);
        }

        public class DragPanelDrag : DragEvent {

            public override void Begin() {
                origin.SetAttribute("dragging", "true");
            }

            public override void Drop(bool success) { }

            public override void OnComplete() {
                origin.SetAttribute("dragging", null);
            }

        }

    }

}