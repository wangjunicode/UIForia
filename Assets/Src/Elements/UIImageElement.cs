using System;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UIImageElement : UIElement {

        public string src;

        public UIImageElement() {
            flags |= UIElementFlags.Image;
            flags |= UIElementFlags.Primitive;
        }

        [OnPropertyChanged(nameof(src))]
        public void OnSrcChanged() {
            style.SetBackgroundImage(UIForia.ResourceManager.GetTexture(src), StyleState.Normal);
        }

    }

}