using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class ClipShape {

        public int id;
        public int version;
        public bool invert;

        public int width;
        public int height;
        
        public virtual bool ShouldCull(in Bounds bounds) {
            return false;
        }

        public ClipShapeType type;
        public Mesh geometry;

    }

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
        public Rect clipRect;
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

        public virtual void PrePaintText() {
            
        }

        public virtual void PrePaintTexture() {
            
        }

        public virtual void PaintForeground(RenderContext ctx) {
            
        }

        public virtual bool ShouldCull(in Rect bounds) {
            // can probably optimize rounded case & say if not in padding bounds, fail
            return false;//RectExtensions.ContainOrOverlap(this.RenderBounds, bounds);
        }

        public virtual ClipShape CreateClipShape() {

            // if is Rect
            // clipShape = ClipShape.FromRect(new RenderBounds());
            
//            renderContext.DrawMaskPath();
//            renderContext.DrawMaskSDF();
            
            // you can draw what you want in a painter. if you want to affect children they need to a. have a painter that responds do this one
            // or b. push a post effect
            // or c. 
            
            // geometry.SetClipMask(texture);
            // current
            
//            clipPath.Reset();
//            clipped.Stroke();
//            // fill / whatever
//            clipPath.canDownSample = true;
//            
//            // clipDb.GetAddOrCreateShape(path);
//            return clipPath;
            return null;

        }
        
      

    }

}