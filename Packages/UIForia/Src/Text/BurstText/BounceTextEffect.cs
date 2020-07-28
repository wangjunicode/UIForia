using System;
using UIForia.Elements;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public class BounceTextEffect : TextEffect {

        public float amplitude = 0.8f;
        public float frequency = 1f;
        public float waveSize = 0.8f;
        public float effectIntensity = 10f;

        internal const int fakeRandomsCount = 25; //18° angle difference
        internal static Vector3[] fakeRandoms;

        static bool initialized = false;

        public static void Initialize() {
            if (initialized)
                return;

            initialized = true;

            //Creates fake randoms from a list of directions (with an incremental angle of 360/fakeRandomsCount between each)
            //and then sorts them randomly, avoiding repetitions (which could have occurred using Random.insideUnitCircle)
            System.Collections.Generic.List<Vector3> randomDirections = new System.Collections.Generic.List<Vector3>();

            for (float i = 0; i < 360; i += 14) {
                float angle = i * Mathf.Deg2Rad;
                randomDirections.Add(new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)).normalized);
            }

            fakeRandoms = new Vector3[fakeRandomsCount];
            int randomIndex;
            for (int i = 0; i < fakeRandoms.Length; i++) {
                randomIndex = UnityEngine.Random.Range(0, randomDirections.Count);
                fakeRandoms[i] = randomDirections[randomIndex];
                randomDirections.RemoveAt(randomIndex);
            }
        }

        float BounceTween(float t) {
            const float stillTime = .2f;
            const float easeIn = .2f;
            const float bounce = 1 - stillTime - easeIn;

            if (t <= easeIn)
                return Tween.EaseInOut(t / easeIn);
            t -= easeIn;

            if (t <= bounce)
                return 1 - Tween.BounceOut(t / bounce);

            return 0;
        }

        private float timePassed;
        private float shakeDelay = 0.04f;
        private int randIndex;
        private int lastRandomIndex;
        private float shakeStrength = 0.085f;

        public override void OnPush(float4x4 worldMatrix, UITextElement element) {
            Initialize();
            timePassed += Time.deltaTime;
            if (timePassed >= shakeDelay) {
                timePassed = 0;

                randIndex = UnityEngine.Random.Range(0, fakeRandomsCount);

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

        public override void ApplyEffect(ref CharacterInterface characterInterface) {
            
            characterInterface.ResetVertices();

            Vector3 random = fakeRandoms[
                Mathf.RoundToInt((characterInterface.charIndex + randIndex) % (fakeRandomsCount - 1))
            ] * (shakeStrength * effectIntensity);

            // float val = (effectIntensity * BounceTween((Mathf.Repeat(elapsed * frequency - waveSize * characterInterface.charIndex, 1))) * amplitude);
            // characterInterface.Translate(random);

            if (characterInterface.character == 'U') {
                characterInterface.SetFaceDilate(Time.realtimeSinceStartup % 1);
                //    characterInterface.SetScale(10); //Time.realtimeSinceStartup % 1);
            }
            //float sin = Mathf.Sin(frequency * elapsed + characterInterface.charIndex * waveSize) * amplitude * effectIntensity;
            //float3 right = new float3(1, 0, 0);
            // characterInterface.SetVertexOffsets(
            //     right * sin,
            //     right * sin,
            //     right * -sin,
            //     right * -sin
            // );
            //bottom, torwards one direction

            // todo -- use better time value
            // if (characterInterface.isRevealing) {
            //     return;
            // }

            // characterInterface.GetDefaultVertices(out float3 topLeft, out float3 topRight, out float3 bottomRight, out float3 bottomLeft);
            // characterInterface.SetVertexMode(CharacterInterface.CharacterVertexMode.Offset);
            // characterInterface.SetVertexSpace(CharacterInterface.CharacterVertexSpace.Local);
            // characterInterface.ResetVertices(); // drops allocated data
            // characterInterface.Translate(new float3()); // gets new data slot 
            // characterInterface.Rotate2D(45 * Mathf.Deg2Rad);
            // characterInterface.RotateAround(new float3(), 45 * Mathf.Deg2Rad);
            // characterInterface.SetVertices(topLeft, topRight, bottomRight, bottomLeft,CharacterInterface.CharacterVertexMode.Offset);
            // characterInterface.SkewX(topLeft, topRight, bottomRight, bottomLeft,CharacterInterface.CharacterVertexMode.Offset);
            // characterInterface.SetFaceColor(Color.red);

            //    characterInterface.RestoreVertices();
            //    characterInterface.MoveVertices(Vector3.up * Mathf.Lerp(0, 20, Time.realtimeSinceStartup % 1)); //effectIntensity * BounceTween((Mathf.Repeat(Time.realtimeSinceStartup * frequency - waveSize * characterInterface.charIndex, 1))) * amplitude));
        }

    }

}