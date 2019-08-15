using System.Collections.Generic;
using System.Linq;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using ShapeType = Vertigo.ShapeType;

namespace UIForia.Rendering {

    public class Polygon {

        public StructList<Vector2> pointList;

        public Polygon Clip(Polygon subject) {
            Polygon retn = new Polygon();
            retn.pointList = new StructList<Vector2>();
            SutherlandHodgman.GetIntersectedPolygon(subject.pointList, pointList, ref retn.pointList);
            return retn;
        }

        public Rect GetBounds() {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int i = 0; i < pointList.size; i++) {
                Vector2 point = pointList.array[i];
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public void Rotate(float angle) {
            Rect bounds = GetBounds();
            Vector2 pivot = bounds.center;

            for (int i = 0; i < pointList.size; i++) {
                pointList[i] = pointList[i].Rotate(pivot, angle);
            }
        }

        public Polygon GetScreenRect() {
            Polygon retn = new Polygon();
            Rect bounds = GetBounds();
            retn.pointList = new StructList<Vector2>();
            retn.pointList.Add(new Vector2(bounds.x, bounds.y));
            retn.pointList.Add(new Vector2(bounds.xMax, bounds.y));
            retn.pointList.Add(new Vector2(bounds.xMax, bounds.yMax));
            retn.pointList.Add(new Vector2(bounds.x, bounds.yMax));
            return retn;
        }

    }

    public class ClipShape {

        public int id;
        public int version;
        public bool invert;

        public int width;
        public int height;

        public Texture texture;

        public readonly UIForiaGeometry geometry;

        public ClipShape() {
            geometry = new UIForiaGeometry();
        }

        public virtual bool ShouldCull(in Bounds bounds) {
            return false;
        }

        public ClipShapeType type;

        public void SetFromElement(UIElement element) {
            Size size = element.layoutResult.actualSize;

            width = (int) size.width;
            height = (int) size.height;
            geometry.Clear();

            float elementWidth = size.width;
            float elementHeight = size.height;
            float min = Mathf.Min(elementWidth, elementHeight);
            float halfMin = min * 0.5f;

            float bevelTopLeft = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelTopLeft);
            float bevelTopRight = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelTopRight);
            float bevelBottomRight = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelBottomLeft);

            float radiusTopLeft = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);

            radiusTopLeft = math.clamp(radiusTopLeft, 0, halfMin) / min;
            radiusTopRight = math.clamp(radiusTopRight, 0, halfMin) / min;
            radiusBottomRight = math.clamp(radiusBottomRight, 0, halfMin) / min;
            radiusBottomLeft = math.clamp(radiusBottomLeft, 0, halfMin) / min;

            byte b0 = (byte) (((radiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((radiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((radiusBottomRight * 1000)) * 0.5f);
            byte b3 = (byte) (((radiusBottomLeft * 1000)) * 0.5f);

            float packedBorderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);

            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
                geometry.ClipCornerRect(new Size(width, height), new CornerDefinition() {
                    topLeftX = bevelTopLeft,
                    topLeftY = bevelTopLeft,
                    topRightX = bevelTopRight,
                    topRightY = bevelTopRight,
                    bottomRightX = bevelBottomRight,
                    bottomRightY = bevelBottomRight,
                    bottomLeftX = bevelBottomLeft,
                    bottomLeftY = bevelBottomLeft,
                });
            }
            else {
                geometry.FillRect(size.width, size.height);
            }

            PaintMode paintMode = PaintMode.None;

            if (texture != null) {
                paintMode = PaintMode.Texture;
            }
            else {
                paintMode = PaintMode.Color;
            }

            geometry.objectData = new Vector4((int) ShapeType.RoundedRect, VertigoUtil.PackSizeVector(size), packedBorderRadii, (int) paintMode);
        }

    }

}