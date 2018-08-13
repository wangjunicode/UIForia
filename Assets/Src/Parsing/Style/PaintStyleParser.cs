using System.Xml.Linq;
using Src.Style;
using UnityEngine;

namespace Src.Parsing.Style {

    public static class PaintStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;

            XElement backgroundColor = element.GetChild("BackgroundColor");
            XElement backgroundImage = element.GetChild("BackgroundImage");
            XElement borderColor = element.GetChild("BorderColor");

            if (backgroundColor != null) {
                template.backgroundColor = StyleParseUtil.ParseColor(element.GetAttribute("value").Value);
            }

            if (backgroundImage != null) {
                template.backgroundImage = ParseResourcePath(element.GetAttribute("value").Value);
            }

            if (borderColor != null) {
                template.borderColor = StyleParseUtil.ParseColor(element.GetAttribute("value").Value);
            }
        }

        private static Texture2D ParseResourcePath(string input) {
            return Resources.Load<Texture2D>(input);
        }

    }

}