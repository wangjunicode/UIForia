using System.Collections.Generic;
using UIForia.Rendering;
using UnityEngine;

namespace SVGX {

    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public LineJoin lineJoin;
        public LineCap lineCap;

        public float dashOffset;
        public float[] dashArray; // id
        public float miterLimit;
        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public float opacity;
        public Color strokeColor; // or gradient id
        public Color fillColor; // or gradient id

        public Vector2 uv;
        public int textureMode;
        public float dropShadow;

        public SVGXStrokeStyle strokeStyle;

    }

    public struct SVGXStrokeStyle {

        public Color color;
        public float alpha;
        public float width;
        public float[] dashArray;
        public LineCap lineCap;
        public LineJoin lineJoin;
        public float miterLimit;

    }

    public struct ColorStop {

        public readonly float percent;
        public readonly Color32 color;

        public ColorStop(float percent, Color32 color) {
            this.percent = percent;
            this.color = color;
        }

    }

    public enum SVGXGradientType {

        Linear,
        Radial,
        Cylindrical

    }

    public class SVGXGradient {

        public string hash;
        public int stopCount;         //max 10 stops? inline array?
        public ColorStop[] stops;
        public SVGXGradientType type;

        public string GetHashString() {
            if (type == SVGXGradientType.Linear) {
                // todo remove allocation, maybe use a char[]
                string hashStr = "L" + stopCount;
                for (int i = 0; i < stopCount; i++) {
                    hashStr += new StyleColor(stops[i].color).rgba.ToString("X");
                }
                return hashStr;
            }

            return null;
        }

    }

}