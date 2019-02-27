using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public enum GradientDirection {

        Horizontal = 0,
        Vertical = 1

    }

    public class SVGXLinearGradient : SVGXGradient {

        public GradientDirection direction;

        public SVGXLinearGradient(GradientDirection direction, IList<ColorStop> colorStops) : base(colorStops) {
            this.direction = direction;
        }
        
        public SVGXLinearGradient( IList<ColorStop> colorStops) : base(colorStops) {
            this.direction = GradientDirection.Horizontal;
        }

    }
    
    public abstract class SVGXGradient {

        public readonly int id;
        public LightList<ColorStop> stops;
        private static int s_IdGenerator;

        protected SVGXGradient(IList<ColorStop> colorStops) {
            this.id = ++s_IdGenerator;
            this.stops = new LightList<ColorStop>(colorStops.Count);
            for (int i = 0; i < colorStops.Count; i++) {
                stops.Add(colorStops[i]);
            }

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