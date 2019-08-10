using System;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public enum ClipShapeType {

        SDFFill,
        SDFStroke,
        SDFComposite,
        Geometry

    }

    public enum RenderBoxTypeFlag {

        CustomClipper,
        TextPrePass,
        TexturePrePass,
        TextureCachePrePass,
        Opaque,
        Transparent

    }

    public abstract class RenderBox {

        internal string uniqueId;

        protected internal UIElement element;

        public Visibility visibility;
        public Overflow overflowX;
        public Overflow overflowY;
        public ClipBehavior clipBehavior = ClipBehavior.Normal;
        public bool clipped;
        public Vector4 clipRect;
        public bool hasForeground;
        public int zIndex;
        public int layer;

        internal RenderBox clipper;
        internal int clippedBoxCount;
        public Texture clipTexture;
        public Vector4 clipUVs;

        public abstract Rect RenderBounds { get; }

        public virtual void OnInitialize() {
            overflowX = element.style.OverflowX;
            overflowY = element.style.OverflowY;
        }

        public virtual void OnDestroy() { }

        public virtual void OnStylePropertyChanged(StructList<StyleProperty> propertyList) { }

        public abstract void PaintBackground(RenderContext ctx);

        public virtual void PrePaintText() { }

        public virtual void PrePaintTexture() { }

        public virtual void PaintForeground(RenderContext ctx) { }

        public virtual bool ShouldCull(in Rect bounds) {
            // can probably optimize rounded case & say if not in padding bounds, fail
            return false; //RectExtensions.ContainOrOverlap(this.RenderBounds, bounds);
        }

        public static float ResolveFixedSize(UIElement element, float baseSize, UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return length.value;
                case UIFixedUnit.Percent:
                    return baseSize * length.value;
                case UIFixedUnit.Em:
                    return element.style.GetResolvedFontSize() * length.value;
                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * length.value;
                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * length.value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual ClipShape CreateClipShape(ClipShape clipShape) {
            // corner clip
            // radii
            // width
            // height
            // texture
            // type 

//            clipShape.SetCornerClip();
//            clipShape.SetCornerRadii();
//            clipShape.SetFromMesh(mesh);
//            clipShape.SetFromElement(element);
//            clipShape.SetFromEllipse();
//            clipShape.SetFromRect();
//            clipShape.SetFromCircle();
//            clipShape.SetFromDiamond();
//            clipShape.SetFromTriangle();
//            clipShape.SetTexture(texture, channel);

            clipShape.SetFromElement(element);

            return clipShape;
        }

    }

}