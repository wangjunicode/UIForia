using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;
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

        public readonly float time;
        public readonly Color32 color;

        public ColorStop(float time, Color32 color) {
            this.time = time;
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
        public LightList<ColorStop> stops;
        public SVGXGradientType type;

        public SVGXGradient(SVGXGradientType type, IList<ColorStop> colorStops) {
            this.type = type;
            this.stops = new LightList<ColorStop>(colorStops.Count);
            for (int i = 0; i < colorStops.Count; i++) {
                stops.Add(colorStops[i]);
            }

            hash = GetHashString(this);
        }

        private static string GetHashString(SVGXGradient gradient) {
            int stopCount = gradient.stops.Count;
            ColorStop[] stops = gradient.stops.Array;
            if (gradient.type == SVGXGradientType.Linear) {
                // todo remove allocation, maybe use a char[]
                string hashStr = "L" + stopCount;
                for (int i = 0; i < stopCount; i++) {
                    hashStr += new StyleColor(stops[i].color).rgba.ToString("X");
                }

                return hashStr;
            }

            return null;
        }

        public Color32 Evaluate(float time) {
            time = Mathf.Clamp01(time);
            int stopCount = stops.Count;
            
            if (stopCount == 0) {
                return new Color32(0, 0, 0, 255);
            }

            if (stopCount == 1) {
                return stops[0].color;
            }

            Color32 output = stops[0].color;

            for (int i = 1; i < stopCount; i++) {
                output = Color32.Lerp(output, stops[i].color, SmoothStep(stops[i - 1].time, stops[i].time, time));
            }

            return output;
        }

        private static float SmoothStep(float a, float b, float x) {
            float t = (x - a) / (b - a);
            t = t > 1f ? 1 : t;
            t = t < 0f ? 0 : t;
            return t * t * (3.0f - (2.0f * t));
        }

    }

}