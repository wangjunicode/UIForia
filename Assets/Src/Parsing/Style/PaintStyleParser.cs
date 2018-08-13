using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Src.Style;
using UnityEngine;

namespace Src.Parsing.Style {

    public static class PaintStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;
            IEnumerable<XElement> nodes = element.Elements("Paint");
            foreach (var node in nodes) {
                XElement backgroundColor = node.GetChild("BackgroundColor");
                XElement backgroundImage = node.GetChild("BackgroundImage");

                if (backgroundColor != null) {
                    template.backgroundColor = ParseColor(node.GetAttribute("value").Value);
                }

                if (backgroundImage != null) {
                    template.backgroundImage = ParseResourcePath(node.GetAttribute("value").Value);
                }
            }
        }

        private static Texture2D ParseResourcePath(string input) {
            return Resources.Load<Texture2D>(input);
        }

        private static Color ParseColor(string input) {
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