using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace Demo.DragPanel.DragPanel {

    [Template("Demo/DragPanel/DragPanel/DragPanel.xml")]
    public class DragPanel<T> : UIElement {

        public List<T> items;

        public DragEvent BeginDrag(MouseInputEvent evt) {
            return new DragPanelDrag();
        }

        public void DragUpdate(DragEvent evt) {
            Debug.Log("updating");
        }

        public void DragEnter(DragPanelDrag evt) {
            
        }
        
        public void DragExit(DragPanelDrag evt) {
            
        }
        
        public class DragPanelDrag : DragEvent { }

    }

}