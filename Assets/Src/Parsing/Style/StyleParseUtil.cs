using System;
using System.Xml.Linq;
using Rendering;
using Src.Style;
using UnityEngine;

namespace Src.Parsing.Style {

    public static class StyleParseUtil {

        public static Vector2 ParseVector2(string input) {
            input = input.Replace(" ", string.Empty);
            string[] values = input.Split(',');
            if (values.Length != 2) {
                throw new Exception("Unable to parse Vector2: " + input);
            }

            return new Vector2(
                float.Parse(values[0]),
                float.Parse(values[1])
            );
        }

        public static ContentBoxRect ParseContentBox(string value) {
            ContentBoxRect box = new ContentBoxRect();

            if (!value.Contains(",")) {
                float m = float.Parse(value);
                box.top = m;
                box.right = m;
                box.bottom = m;
                box.right = m;
                return box;
            }

            string[] values = value.Split(',');

            if (values.Length == 2) {
                float topBottom = float.Parse(values[0]);
                float leftRight = float.Parse(values[1]);
                box.top = topBottom;
                box.bottom = topBottom;
                box.left = leftRight;
                box.right = leftRight;
                return box;
            }

            if (values.Length == 3) {
                box.top = float.Parse(values[0]);
                box.left = float.Parse(values[1]);
                box.right = box.left;
                box.bottom = float.Parse(values[2]);
                return box;
            }

            if (values.Length == 4) {
                box.top = float.Parse(values[0]);
                box.bottom = float.Parse(values[1]);
                box.left = float.Parse(values[2]);
                box.right = float.Parse(values[3]);
                return box;
            }

            throw new Exception("Bad content box input");
        }

        public static Color ParseColor(string input) {
            Color retn = new Color();
            ParseColor(ref retn, input);
            return retn;
        }

        public static void ParseColor(ref Color color, XElement element) {
            if (element == null) return;
            ParseColor(ref color, element.GetAttribute("value").Value);
        }

        public static void ParseColor(ref Color color, string input) {
            input = input.Replace(" ", string.Empty);
            input = input.Replace(")", string.Empty);
            if (input.StartsWith("rgb(")) {
                string[] values = input.Substring(4).Split(',');
                if (values.Length == 3) {
                    color.r = Mathf.Clamp(float.Parse(values[0]), 0, 255);
                    color.g = Mathf.Clamp(float.Parse(values[1]), 0, 255);
                    color.b = Mathf.Clamp(float.Parse(values[2]), 0, 255);
                    color.a = 1f;
                }
            }
            else if (input.StartsWith("rgba(")) {
                string[] values = input.Substring(5).Split(',');
                if (values.Length == 4) {
                    color.r = Mathf.Clamp(float.Parse(values[0]), 0, 255);
                    color.g = Mathf.Clamp(float.Parse(values[1]), 0, 255);
                    color.b = Mathf.Clamp(float.Parse(values[2]), 0, 255);
                    color.a = Mathf.Clamp(float.Parse(values[3]), 0, 255);
                }
            }
            else {
                throw new Exception("Cannot parse color: " + input);
            }
        }

        public static UIMeasurement ParseMeasurement(string value) {
            return new UIMeasurement(float.Parse(value), UIUnit.Pixel);
        }

        public static void ParseMeasurement(ref UIMeasurement measurement, XElement element) {
            if (element == null) return;
            measurement = new UIMeasurement(float.Parse(element.GetAttribute("value").Value), UIUnit.Pixel);
        }

    }

}