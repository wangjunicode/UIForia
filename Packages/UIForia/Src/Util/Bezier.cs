using UIForia.Rendering;
using Unity.Mathematics;

namespace UIForia {

    /// <summary>
    /// Ported from https://github.com/gre/bezier-easing/blob/master/src/index.js
    /// MIT License 
    /// </summary>
    public unsafe struct Bezier {

        public readonly float mX1;
        public readonly float mX2;
        public readonly float mY1;
        public readonly float mY2;
        
        public const int kSplineTableSize = 11;
        public const float kSampleStepSize = 1.0f / (kSplineTableSize - 1.0f);

        public const int NEWTON_ITERATIONS = 4;
        public const float NEWTON_MIN_SLOPE = 0.001f;
        public const float SUBDIVISION_PRECISION = 0.0000001f;
        public const int SUBDIVISION_MAX_ITERATIONS = 10;
        
        public fixed float sampleValues[kSplineTableSize];
                
        public Bezier(float x1, float y1, float x2, float y2) {
            this.mX1 = x1;
            this.mX2 = x2;
            this.mY1 = y1;
            this.mY2 = y2;
                
            for (int i = 0; i < kSplineTableSize; ++i) {
                sampleValues[i] = CalcBezier(i * kSampleStepSize, mX1, mX2);
            }

        }

        public static readonly Bezier EaseInSine = new Bezier(0.12f, 0, 0.39f, 0);
        public static readonly Bezier EaseOutSine = new Bezier(0.61f, 1, 0.88f, 1);
        public static readonly Bezier EaseInOutSine = new Bezier(0.37f, 0, 0.63f, 1);
        public static readonly Bezier EaseInQuad = new Bezier(0.11f, 0, 0.5f, 0);
        public static readonly Bezier EaseOutQuad = new Bezier(0.5f, 1, 0.89f, 1);
        public static readonly Bezier EaseInOutQuad = new Bezier(0.45f, 0, 0.55f, 1);
        public static readonly Bezier EaseInCubic = new Bezier(0.32f, 0, 0.67f, 0);
        public static readonly Bezier EaseOutCubic = new Bezier(0.33f, 1, 0.68f, 1);
        public static readonly Bezier EaseInOutCubic = new Bezier(0.65f, 0, 0.35f, 1);
        public static readonly Bezier EaseInQuart = new Bezier(0.5f, 0, 0.75f, 0);
        public static readonly Bezier EaseOutQuart = new Bezier(0.25f, 1, 0.5f, 1);
        public static readonly Bezier EaseInOutQuart = new Bezier(0.76f, 0, 0.24f, 1);
        public static readonly Bezier EaseInQuint = new Bezier(0.64f, 0, 0.78f, 0);
        public static readonly Bezier EaseOutQuint = new Bezier(0.22f, 1, 0.36f, 1);
        public static readonly Bezier EaseInOutQuint = new Bezier(0.83f, 0, 0.17f, 1);
        public static readonly Bezier EaseInExpo = new Bezier(0.7f, 0, 0.84f, 0);
        public static readonly Bezier EaseOutExpo = new Bezier(0.16f, 1, 0.3f, 1);
        public static readonly Bezier EaseInOutExpo = new Bezier(0.87f, 0, 0.13f, 1);
        public static readonly Bezier EaseInCirc = new Bezier(0.55f, 0, 1, 0.45f);
        public static readonly Bezier EaseOutCirc = new Bezier(0, 0.55f, 0.45f, 1);
        public static readonly Bezier EaseInOutCirc = new Bezier(0.85f, 0, 0.15f, 1);
        public static readonly Bezier EaseInBack = new Bezier(0.36f, 0, 0.66f, -0.56f);
        public static readonly Bezier EaseOutBack = new Bezier(0.34f, 1.56f, 0.64f, 1);
        public static readonly Bezier EaseInOutBack = new Bezier(0.68f, -0.6f, 0.32f, 1.6f);
        
        public static readonly Bezier easeInElastic = new Bezier(0, 0, 0, 0);
        public static readonly Bezier easeOutElastic = new Bezier(0, 0, 0, 0);
        public static readonly Bezier easeInOutElastic = new Bezier(0, 0, 0, 0);
        public static readonly Bezier easeInBounce = new Bezier(0, 0, 0, 0);
        public static readonly Bezier easeOutBounce = new Bezier(0, 0, 0, 0);
        public static readonly Bezier easeInOutBounce = new Bezier(0, 0, 0, 0);
        
        public float Evaluate(float progress) {
            
            progress = math.clamp(progress, 0f, 1f);
            
            if (progress == 0 || progress == 1) {
                return progress;
            }

            return CalcBezier(GetTForX(progress), mY1, mY2);
        }

        private float GetTForX(float aX) {
            float intervalStart = 0.0f;
            int currentSample = 1;
            const int lastSample = kSplineTableSize - 1;

            for (; currentSample != lastSample && sampleValues[currentSample] <= aX; ++currentSample) {
                intervalStart += kSampleStepSize;
            }

            --currentSample;

            // Interpolate to provide an initial guess for t
            float dist = (aX - sampleValues[currentSample]) / (sampleValues[currentSample + 1] - sampleValues[currentSample]);
            float guessForT = intervalStart + dist * kSampleStepSize;

            float initialSlope = GetSlope(guessForT, mX1, mX2);
            if (initialSlope >= NEWTON_MIN_SLOPE) {
                return NewtonRaphsonIterate(aX, guessForT, mX1, mX2);
            }

            return initialSlope == 0f ? guessForT : BinarySubdivide(aX, intervalStart, intervalStart + kSampleStepSize, mX1, mX2);

        }


        private static float A(float aA1, float aA2) {
            return 1.0f - 3.0f * aA2 + 3.0f * aA1;
        }

        private static float B(float aA1, float aA2) {
            return 3.0f * aA2 - 6.0f * aA1;
        }

        private static float C(float aA1) {
            return 3.0f * aA1;
        }

        private static float CalcBezier(float aT, float aA1, float aA2) {
            return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT;
        }

        private static float GetSlope(float aT, float aA1, float aA2) {
            return 3.0f * A(aA1, aA2) * aT * aT + 2.0f * B(aA1, aA2) * aT + C(aA1);
        }

        private static float BinarySubdivide(float aX, float aA, float aB, float mX1, float mX2) {
            float currentX;
            float currentT;
            int i = 0;

            do {
                currentT = aA + (aB - aA) / 2.0f;
                currentX = CalcBezier(currentT, mX1, mX2) - aX;
                if (currentX > 0.0) {
                    aB = currentT;
                }
                else {
                    aA = currentT;
                }
            } while (math.abs(currentX) > SUBDIVISION_PRECISION && ++i < SUBDIVISION_MAX_ITERATIONS);

            return currentT;
        }

        private static float NewtonRaphsonIterate(float aX, float aGuessT, float mX1, float mX2) {
            for (int i = 0; i < NEWTON_ITERATIONS; ++i) {
                float currentSlope = GetSlope(aGuessT, mX1, mX2);
                if (currentSlope == 0f) {
                    return aGuessT;
                }

                float currentX = CalcBezier(aGuessT, mX1, mX2) - aX;
                aGuessT -= currentX / currentSlope;
            }

            return aGuessT;
        }

    }

}