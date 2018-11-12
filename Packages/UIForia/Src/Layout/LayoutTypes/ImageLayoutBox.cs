using System;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class ImageLayoutBox : LayoutBox {

        private readonly UIImageElement image;

        public ImageLayoutBox(LayoutSystem layoutSystem, UIElement element) : base(layoutSystem, element) {
            image = (UIImageElement) element;
        }

        public override void RunLayout() {
            Texture2D texture = image.texture;
            if (style.PreferredWidth.unit == UIMeasurementUnit.Content) {
                if (texture != null) {
                    actualWidth = texture.width;
                }
                else {
                    actualWidth = 0;
                }
            }
            else {
                actualWidth = GetWidths().clampedSize;
            }

            if (style.PreferredHeight.unit == UIMeasurementUnit.Content) {
                if (texture != null) {
                    actualHeight = texture.height;
                }
                else {
                    actualHeight = 0;
                }
            }
            else {
                actualHeight = GetHeights(actualWidth).clampedSize;
            }

//            actualWidth += PaddingBorderHorizontal;
//            actualHeight += PaddingBorderVertical;
        }

        protected override float ComputeContentWidth() {
            return (image.texture == null ? 0 : image.texture.width);
        }

        protected override float ComputeContentHeight(float width) {
            // todo -- preserve aspect ratio
//            float scale = width / image.texture.width;
//            float aspect = image.texture.width / image.texture.height;
            return (image.texture == null ? 0 : image.texture.height);
        }

    }

}