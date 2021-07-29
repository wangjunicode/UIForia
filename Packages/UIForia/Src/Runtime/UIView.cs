using System;
using UnityEngine;

namespace UIForia {

    public class UIView {

        public readonly string name;

        public readonly UIApplication application;

        internal ushort viewId;
        internal object updateContext;
        internal int lastTickFrame;

        internal Func<UIForiaRuntimeSystem, UIElement, object, object> updateFn;
        
        internal UIView(int index, UIApplication application, string name, Rect rect) {
            this.viewId = (ushort) index;
            this.name = name;
            this.application = application;
            this.Viewport = rect;
        }

        public Rect Viewport { get; set; }

        public UIElement RootElement { get; internal set; }

        public void Destroy() {
            // applicationOld.RemoveView(this);
        }

        public void SetSize(int width, int height) {
            Viewport = new Rect(Viewport.x, Viewport.y, width, height);
        }

        /// <returns>true in case the depth has been changed in order to get focus</returns>
        public bool RequestFocus() {
            throw new NotImplementedException();
        }

        public bool HasFocus => false;

    }

}