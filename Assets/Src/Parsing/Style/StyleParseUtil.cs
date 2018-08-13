using System;
using UnityEngine;

namespace Src.Parsing.Style {

    public static class StyleParseUtil {

        public static Color ParseColor(string input) {
            Color retn = new Color();
            input = input.Replace(" ", string.Empty);

            if (input.StartsWith("rgb(")) {
                string[] values = input.Substring(4).Split(',');
                if (values.Length == 3) {
                    retn.r = Mathf.Clamp(float.Parse(values[0]), 0, 255);
                    retn.g = Mathf.Clamp(float.Parse(values[1]), 0, 255);
                    retn.b = Mathf.Clamp(float.Parse(values[2]), 0, 255);
                    retn.a = 1f;
                }
            }
            else if (input.StartsWith("rgba(")) {
                string[] values = input.Substring(5).Split(',');
                if (values.Length == 4) {
                    retn.r = Mathf.Clamp(float.Parse(values[0]), 0, 255);
                    retn.g = Mathf.Clamp(float.Parse(values[1]), 0, 255);
                    retn.b = Mathf.Clamp(float.Parse(values[2]), 0, 255);
                    retn.a = Mathf.Clamp(float.Parse(values[3]), 0, 255);
                }
            }
            else {
                throw new Exception("Cannot parse color: " + input);
            }

            return retn;
        }

    }

}