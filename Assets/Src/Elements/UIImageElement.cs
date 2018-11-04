using System;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UIImageElement : UIElement {

        public string src;
        public Texture2D texture;
        
        public UIImageElement() {
            flags |= UIElementFlags.Image;
            flags |= UIElementFlags.Primitive;
        }

        public override void OnReady() {
            
        }
        
        [OnPropertyChanged(nameof(src))]
        public void OnSrcChanged(string name) {
            texture = UIForia.ResourceManager.GetTexture(src);
            style.SetBackgroundImage(texture, StyleState.Normal);
        }

    }

}