using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UIForia.Text {

    public class RotateTextEffect : TextEffect<RotateTextEffect.EffectParameters>, IUIForiaRichTextEffect {

        private EffectParameters parameters;

        public struct EffectParameters {

            public float angleSpeed;
            public float angleDiffBetweenChars;

        }

        private float elapsed;

        public override void OnPop() {
            elapsed += Time.deltaTime;
        }

        public override void SetParameters(EffectParameters parameters) {
            this.parameters = parameters;
        }

        public override void ApplyEffect(ref CharacterInterface characterInterface) {
            float rotation = -elapsed * parameters.angleSpeed + parameters.angleDiffBetweenChars * characterInterface.charIndex;
            if (rotation > 360) rotation = 0;
            characterInterface.ResetVertices();
            characterInterface.RotateLocalAngleAxis(rotation, new Vector3(0, 0, 1));
        }

        public bool TryParseRichTextAttributes(CharStream stream) {
            stream.ParseFloatAttr("speed", out parameters.angleSpeed, 180f);
            stream.ParseFloatAttr("diff", out parameters.angleDiffBetweenChars, 10f);
            return true;
        }

    }

    public class BounceTextEffect : TextEffect<BounceTextEffect.EffectParameters>, IUIForiaRichTextEffect {

        public struct EffectParameters {

            public float amplitude;
            public float frequency;
            public float waveSize;
            public float effectIntensity;

        }

        public Color32 glowColor;
        private float timePassed;
        private float shakeDelay = 0.04f;
        private int randIndex;
        private int lastRandomIndex;
        private float shakeStrength = 0.085f;
        private float angleSpeed;
        private float angleDiffBetweenChars;

        internal const int fakeRandomsCount = 25; //18° angle difference
        internal static Vector3[] fakeRandoms;

        public EffectParameters parameters;

        static bool initialized = false;

        public static void Initialize() {
            if (initialized)
                return;

            initialized = true;

            //Creates fake randoms from a list of directions (with an incremental angle of 360/fakeRandomsCount between each)
            //and then sorts them randomly, avoiding repetitions (which could have occurred using Random.insideUnitCircle)
            List<Vector3> randomDirections = new List<Vector3>();

            for (float i = 0; i < 360; i += 14) {
                float angle = i * Mathf.Deg2Rad;
                randomDirections.Add(new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)).normalized);
            }

            fakeRandoms = new Vector3[fakeRandomsCount];
            int randomIndex;
            for (int i = 0; i < fakeRandoms.Length; i++) {
                randomIndex = Random.Range(0, randomDirections.Count);
                fakeRandoms[i] = randomDirections[randomIndex];
                randomDirections.RemoveAt(randomIndex);
            }
        }

        private static float BounceTween(float t, float pauseTime = 0.2f, float easeIn = 0.2f) {

            float bounce = 1 - pauseTime - easeIn;
            if (t <= easeIn) {
                return EaseInOut(t / easeIn);
            }

            t -= easeIn;

            return t <= bounce ? 1 - BounceOut(t / bounce) : 0;

        }

        public override void SetParameters(EffectParameters parameters) {
            this.parameters = parameters;
        }

        public override void OnPush(float4x4 worldMatrix, UITextElement element) {
            Initialize();
            parameters.waveSize = 1.8f;
            parameters.amplitude = 3;
            parameters.frequency = 3;
            parameters.effectIntensity = 3;
            timePassed += Time.deltaTime;
            if (timePassed >= shakeDelay) {
                timePassed = 0;

                randIndex = Random.Range(0, fakeRandomsCount);

                //Avoids repeating the same index twice 
                if (lastRandomIndex == randIndex) {
                    randIndex++;
                    if (randIndex >= fakeRandomsCount) {
                        randIndex = 0;
                    }
                }

                lastRandomIndex = randIndex;
            }
        }

        public override void OnPop() {
            elapsed += Time.deltaTime;
            loopCount++;
        }

        private float elapsed;
        private int loopCount;

        public static float EaseInOut(float t) {
            return Mathf.Lerp(t * t, 1 - ((1 - t) * (1 - t)), t);
        }

        public static float BounceOut(float t) {

            if (t < (1f / 2.75f)) {
                return 7.5625f * t * t;
            }

            if (t < (2f / 2.75f)) {
                return 7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f;
            }

            if (t < (2.5f / 2.75f)) {
                return 7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f;
            }

            return 7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f;
        }

        public override void ApplyEffect(ref CharacterInterface characterInterface) {
            characterInterface.ResetVertices();
//            Vector3 random = fakeRandoms[
//                Mathf.RoundToInt((characterInterface.charIndex + randIndex) % (fakeRandomsCount - 1))
//            ] * (shakeStrength * effectIntensity);

            float t = (Mathf.Repeat(elapsed * parameters.frequency - parameters.waveSize * characterInterface.charIndex, 1));
            float val = (parameters.effectIntensity * BounceTween(t) * parameters.amplitude);

            characterInterface.Translate(new float3(0, val, 0));

            characterInterface.SetGlowColor(glowColor);

        }

        public bool TryParseRichTextAttributes(CharStream stream) {

            parameters.amplitude = stream.TryParseFloatAttr("amp", out float value) ? value : parameters.amplitude;
            parameters.waveSize = stream.TryParseFloatAttr("waveSize", out value) ? value : parameters.waveSize;
            parameters.effectIntensity = stream.TryParseFloatAttr("factor", out value) ? value : parameters.effectIntensity;
            parameters.frequency = stream.TryParseFloatAttr("freq", out value) ? value : parameters.frequency;

            return true;
        }

    }

}