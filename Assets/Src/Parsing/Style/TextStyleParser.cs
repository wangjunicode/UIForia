using System.Collections.Generic;
using System.Xml.Linq;
using Rendering;
using UnityEngine;

namespace Src.Style {

    public static class TextStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;
            IEnumerable<XElement> textNodes = element.Elements("Text");
            foreach (var textNode in textNodes) {
                XElement fontSize = textNode.GetChild("FontSize");
                if (fontSize != null) {
                    template.fontSize = fontSize.GetAttribute("value").GetValueAsInt();
                }

                XElement fontAsset = textNode.GetChild("FontAsset");
                if (fontAsset != null) {
                    template.fontAssetName = fontAsset.GetAttribute("url").Value.Trim();
                }

                XElement fontStyle = textNode.GetChild("FontStyle");
                if (fontStyle != null) {
                    template.fontStyle = (FontStyle) fontStyle.GetAttribute("value").GetValueAsInt();
                }

                XElement textOverflow = textNode.GetChild("TextOverflow");
                if (textOverflow != null) {
                    template.overflowType = (TextOverflow) textOverflow.GetAttribute("value").GetValueAsInt();
                }

                XElement textAnchor = textNode.GetChild("TextAnchor");
                if (textAnchor != null) {
                    template.textAnchor = (TextAnchor) textAnchor.GetAttribute("value").GetValueAsInt();
                }
            }
        }

    }

}