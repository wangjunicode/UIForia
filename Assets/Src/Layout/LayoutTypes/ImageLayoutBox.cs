using System;
using Rendering;
using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class ImageLayoutBox : LayoutBox {

        private readonly UIImageElement image;

        public ImageLayoutBox(LayoutSystem layoutSystem, UIElement element) : base(layoutSystem, element) {
            image = (UIImageElement) element;
        }

        public override void RunLayout() { }


    }

}