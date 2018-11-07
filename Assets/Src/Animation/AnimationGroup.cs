using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

    public class AnimationGroup : StyleAnimation {

        private StyleAnimation[] animations;
        private AnimationStatus[] state;
        
        public AnimationGroup(params StyleAnimation[] animations) {
            this.animations = animations;
            this.state = ArrayPool<AnimationStatus>.GetExactSize(animations.Length);
            for (int i = 0; i < state.Length; i++) {
                state[i] = AnimationStatus.Pending;
            }
        }

        public override void OnStart(UIStyleSet styleSet, Rect viewport) {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].OnStart(styleSet, viewport);
                state[i] = AnimationStatus.Running;
            }
        }

        public override AnimationStatus Update(UIStyleSet styleSet, Rect viewport, float deltaTime) {
            for (int i = 0; i < animations.Length; i++) {
                if (state[i] == AnimationStatus.Running) {
                    state[i] = animations[i].Update(styleSet, viewport, deltaTime);
                }
            }

            for (int i = 0; i < state.Length; i++) {
                if (state[i] == AnimationStatus.Running) {
                    return AnimationStatus.Running;
                }
            }

            return AnimationStatus.Completed;
        }

    }

//    public class CurveAnimation : PropertyAnimation {
//
//        private AnimationCurve curve;
//
//        public CurveAnimation(StyleProperty m_TargetValue, AnimationCurve curve, AnimationOptions options)
//            : base(m_TargetValue, options) { }
//
//        public override bool Update(UIStyleSet styleSet, Rect viewport, float deltaTime) { }
//
//    }
//
//    public class ResponseCurve {
//
//        // todo make this class also wrap AnimationCurve & Easing types and use this as baseline for PropertyAnimation
//        public enum ResponseCurveType {
//
//            Constant,
//            Polynomial,
//            Logistic,
//            Logit,
//            Threshold,
//            Quadratic,
//            Parabolic,
//            NormalDistribution,
//            Bounce,
//            Sin
//
//        }
//
//        public ResponseCurveType curveType;
//        public float slope; //(m)
//        public float exp; //(k)
//        public float vShift; //vertical shift (b)
//        public float hShift; //horizonal shift (c)
//        public float threshold;
//        public bool invert;
//
//        public ResponseCurve() {
//            curveType = ResponseCurveType.Polynomial;
//            slope = 1;
//            exp = 1;
//            vShift = 0;
//            hShift = 0;
//            threshold = 0;
//            invert = false;
//        }
//
//        public ResponseCurve(ResponseCurveType type, float slope = 1, float exp = 1, bool invert = false) {
//            this.curveType = type;
//            this.slope = slope;
//            this.exp = exp;
//            vShift = 0;
//            hShift = 0;
//            threshold = 0;
//            this.invert = invert;
//        }
//
//        public ResponseCurve(ResponseCurveType type, float slope = 1, float exp = 1, float vShift = 0, float hShift = 0, float threshold = 0, bool invert = false) {
//            this.curveType = type;
//            this.slope = slope;
//            this.exp = exp;
//            this.vShift = vShift;
//            this.hShift = hShift;
//            this.threshold = threshold;
//            this.invert = invert;
//        }
//
//        public float Evaluate(float input) {
//            input = Mathf.Clamp01(input);
//            float output;
//            if (input < threshold && curveType != ResponseCurveType.Constant) return 0f;
//            switch (curveType) {
//                case ResponseCurveType.Constant:
//                    output = threshold;
//                    break;
//                case ResponseCurveType.Polynomial: // y = m(x - c)^k + b 
//                    output = slope * (Mathf.Pow((input - hShift), exp)) + vShift;
//                    break;
//                case ResponseCurveType.Logistic: // y = (k * (1 / (1 + (1000m^-1*x + c))) + b
//                    output = (exp * (1.0f / (1.0f + Mathf.Pow(Mathf.Abs(1000.0f * slope), (-1.0f * input) + hShift + 0.5f)))) + vShift; // Note, addition of 0.5 to keep default 0 hShift sane
//                    break;
//                case ResponseCurveType.Logit: // y = -log(1 / (x + c)^K - 1) * m + b
//                    output = (-Mathf.Log((1.0f / Mathf.Pow(Mathf.Abs(input - hShift), exp)) - 1.0f) * 0.05f * slope) + (0.5f + vShift); // Note, addition of 0.5f to keep default 0 XIntercept sane
//                    break;
//                case ResponseCurveType.Quadratic: // y = mx * (x - c)^K + b
//                    output = ((slope * input) * Mathf.Pow(Mathf.Abs(input + hShift), exp)) + vShift;
//                    break;
//                case ResponseCurveType.Sin: //sin(m * (x + c) ^ K + b
//                    output = (Mathf.Sin((2 * Mathf.PI * slope) * Mathf.Pow(input + (hShift - 0.5f), exp)) * 0.5f) + vShift + 0.5f;
//                    break;
//                case ResponseCurveType.Parabolic:
//                    output = Mathf.Pow(slope * (input + hShift), 2) + (exp * (input + hShift)) + vShift;
//                    break;
//                case ResponseCurveType.Bounce:
//                    output = Mathf.Abs(Mathf.Sin((2f * Mathf.PI * exp) * (input + hShift + 1f) * (input + hShift + 1f)) * (1f - input) * slope) + vShift;
//                    break;
//                case ResponseCurveType.NormalDistribution: // y = K / sqrt(2 * PI) * 2^-(1/m * (x - c)^2) + b
//                    output = (exp / (Mathf.Sqrt(2 * Mathf.PI))) * Mathf.Pow(2.0f, (-(1.0f / (Mathf.Abs(slope) * 0.01f)) * Mathf.Pow(input - (hShift + 0.5f), 2.0f))) + vShift;
//                    break;
//                case ResponseCurveType.Threshold:
//                    output = input > hShift ? (1.0f - vShift) : (0.0f - (1.0f - slope));
//                    break;
//                default:
//                    return 0f; // throw new Exception($"{curveType} curve has not been implemented yet");
//            }
//
//            if (invert) output = 1f - output;
//            return Mathf.Clamp01(output);
//        }
//
//    }

}