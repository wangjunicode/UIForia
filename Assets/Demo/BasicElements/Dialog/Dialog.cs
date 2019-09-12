using System;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Demo.BasicElements.Dialog {
    
    [Template("Demo/BasicElements/Dialog/Dialog.xml")]
    public class Dialog : UIElement {

        public Action onCancel;
        public Action onConfirm;
        
        public bool isVisible;
        public Color confirmColor;

        [WriteBinding(nameof(isVisible))]
        public event Action<bool> onVisibilityChanged;
        
        [OnPropertyChanged(nameof(isVisible))]
        public void OnVisibilityChanged(string propertyName) {
            SetAttribute("placement", isVisible ? "show" : "hide");
        }

        public void Confirm() {
            isVisible = false;
            onVisibilityChanged?.Invoke(isVisible);
            SetAttribute("placement", isVisible ? "show" : "hide");

            onConfirm?.Invoke();
        }

        public void Cancel() {
            isVisible = false;
            onVisibilityChanged?.Invoke(isVisible);
            SetAttribute("placement", isVisible ? "show" : "hide");

            onCancel?.Invoke();
        }
    }
}
