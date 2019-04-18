
using System;
using UIForia.Attributes;
using UIForia.Elements;

namespace UI {

    [Template("KlangWindow/KlangWindow.xml")]
    public class KlangWindow : UIElement {

        public bool isOpen;

        public event Action onOpen;
        public event Action onClose;

        public void Open() {
            isOpen = true;
            onOpen?.Invoke();
        }
        
        public void Close() {
            isOpen = false;
            onClose?.Invoke();
        }

    }

}