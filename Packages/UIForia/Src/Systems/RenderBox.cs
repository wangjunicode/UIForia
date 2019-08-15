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
        Path,
        Texture

    }

    public enum RenderBoxTypeFlag {

        CustomClipper,
        TextPrePass,
        TexturePrePass,
        TextureCachePrePass,
        Opaque,
        Transparent

    }

    public struct PolyRect {

        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        public PolyRect(in Vector2 p0, in Vector2 p1, in Vector2 p2, in Vector2 p3) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

    }

    public class ClipData {

        public bool isRect;
        public bool isCulled;
        public Rect screenSpaceBounds;
        public ClipData parent;
        public int visibleBoxCount;
        public StructList<Vector2> intersected;
        public PolyRect worldBounds;
        public RenderBox renderBox;

        public ClipData() {
            intersected = new StructList<Vector2>();
        }

        public void Clear() {
            parent = null;
            isCulled = false;
            visibleBoxCount = 0;
            isRect = false;
            renderBox = null;
            intersected.size = 0;
            worldBounds = default;
        }
    }

    public abstract class RenderBox {

        internal string uniqueId;

        protected internal UIElement element;

        public Visibility visibility;
        public Overflow overflowX;
        public Overflow overflowY;
        public ClipBehavior clipBehavior = ClipBehavior.Normal;
        public bool culled;
        public Vector4 clipRect;
        public bool hasForeground;
        public int zIndex;
        public int layer;

        internal ClipData clipper;
        internal int clippedBoxCount;
        public Texture clipTexture;
        public Vector4 clipUVs;
        public bool didRender;
        protected ClipShape clipShape;

        public virtual Rect RenderBounds => new Rect(
            element.layoutResult.localPosition.x,
            element.layoutResult.localPosition.y,
            element.layoutResult.actualSize.width,
            element.layoutResult.actualSize.height
        );

        public virtual Rect ClipBounds => RenderBounds;

        public virtual void OnInitialize() {
            overflowX = element.style.OverflowX;
            overflowY = element.style.OverflowY;
        }

        public virtual void OnDestroy() { }

        public virtual void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            for (int i = 0; i < propertyList.size; i++) {
                ref StyleProperty property = ref propertyList.array[i];
                switch (property.propertyId) {
                    case StylePropertyId.OverflowX:
                        overflowX = property.AsOverflow;
                        break;
                    case StylePropertyId.OverflowY:
                        overflowY = property.AsOverflow;
                        break;
                }
            }
        }

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

        public virtual ClipShape GetClipShape() {
            clipShape = clipShape ?? new ClipShape();

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