using System.IO;
using SVGX;
using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public class GradientTest {

        [MenuItem("UIForia/GradientTest")]
        public static void MakeGradient() {
            SVGXGradient g = new SVGXGradient(SVGXGradientType.Linear, new[] {
                new ColorStop(0f, Color.blue),
                new ColorStop(0.5f, Color.yellow),
                new ColorStop(0.75f, Color.white),
                new ColorStop(1f, Color.red),
            });

            Texture2D texture2D = new Texture2D(128, 20);

            for (int i = 0; i < 128; i++) {
                Color32 c = g.Evaluate(i / (float) 128);
                for (int j = 0; j < 20; j++) {
                    texture2D.SetPixel(i, j, c);
                }
            }

            texture2D.Apply(false);
            Debug.Log(UnityEngine.Application.dataPath + "/TEMPGradient.png");
            File.WriteAllBytes(UnityEngine.Application.dataPath + "/TEMPGradient.png", texture2D.EncodeToPNG());
        }

    }

}