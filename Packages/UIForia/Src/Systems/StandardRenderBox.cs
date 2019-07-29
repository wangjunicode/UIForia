using SVGX;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;
using Vertigo;

namespace UIForia.Rendering {

    public class StandardRenderBox : RenderBox {

        protected bool geometryNeedsUpdate;
        protected bool dataNeedsUpdate;

        protected float borderTop;
        protected float borderRight;
        protected float borderBottom;
        protected float borderLeft;
        protected Size lastSize;
        protected GeometryRange range;
        protected UIForiaGeometry geometry;

        public StandardRenderBox() {
            this.uniqueId = "UIForia::StandardRenderBox";
            this.geometry = new UIForiaGeometry();
        }

        public override Rect RenderBounds => new Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

        public override void OnInitialize() {
            geometry.Clear();
            lastSize = new Size(-1, -1);
            geometryNeedsUpdate = true;
            dataNeedsUpdate = true;
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] properties = propertyList.array;
            int count = propertyList.size;

            bool uvsDirty;
            bool colorsDirty;
            bool geometryDirty;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.BackgroundColor:
                        break;

                    case StylePropertyId.BorderColorTop:

                    case StylePropertyId.BorderColorRight:

                    case StylePropertyId.BorderColorBottom:

                    case StylePropertyId.BorderColorLeft:
                        break;
                }
            }
        }

        private void UpdateGeometry(in Size size) {
            geometryNeedsUpdate = false;

            bool hasBorder = element.layoutResult.border.IsZero;

            Color c;
            geometry.Clear();
            geometry.FillRectUniformBorder_Miter(size.width, size.height);
            range = new GeometryRange(0, geometry.positionList.size, 0, geometry.triangleList.size);
            
            // corner join: Bevel | Miter | Round
            // border only
            // border uniform
            // border rounded
            // border fill only
            // background cover
            // background box (border, padding, content)
            // uv transform
        }

        public override void PaintBackground(RenderContext ctx) {
            Size newSize = element.layoutResult.actualSize;

            if (geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            Color32 backgroundColor = element.style.BackgroundColor;
            Color32 backgroundTint = element.style.BackgroundTint;
            Texture backgroundImage = element.style.BackgroundImage;

           // Debug.Log("paint: " + element.GetAttribute("id"));
            if (backgroundColor.a <= 0) {
                return;
            }
            
            float packedBackgroundColor = VertigoUtil.ColorToFloat(element.style.BackgroundColor);
            float packedBackgroundTint = VertigoUtil.ColorToFloat(element.style.BackgroundTint);
            PaintMode colorMode = PaintMode.None;

            if (!ReferenceEquals(backgroundImage, null)) {
                colorMode |= PaintMode.Texture;
            }

            if (backgroundTint.a > 0) {
                colorMode |= PaintMode.TextureTint;
            }

            if (backgroundColor.a > 0) {
                colorMode |= PaintMode.Color;
            }

            // todo -- put this back to packed! shader too
            geometry.packedColors = backgroundColor;//new Color(packedBackgroundColor, packedBackgroundTint, (int) colorMode, 0);
            geometry.mainTexture = backgroundImage;
            // y and rotation are inverted!
            // element.layoutResult.matrix = SVGXMatrix.TRS(new Vector2(100, -100), -45, Vector2.one);
            ctx.DrawBatchedGeometry(geometry, range, element.layoutResult.matrix.ToMatrix4x4());
            
            
        }

    }

}