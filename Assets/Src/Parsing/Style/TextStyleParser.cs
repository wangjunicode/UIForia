using System.Collections.Generic;
using System.Xml.Linq;
using Rendering;
using Src.Parsing.Style;
using UnityEngine;

namespace Src.Style {

    public static class TextStyleParser {

        public static void ParseStyle(XElement element, UIStyle template) {
            if (element == null) return;
//            XElement fontSize = element.GetChild("FontSize");
//            if (fontSize != null) {
//                template.fontSize = fontSize.GetAttribute("value").GetValueAsInt();
//            }
//
//            XElement fontAsset = element.GetChild("FontAsset");
//            if (fontAsset != null) {
//                template.fontAssetName = fontAsset.GetAttribute("url").Value.Trim();
//            }
//
//            XElement fontStyle = element.GetChild("FontStyle");
//            if (fontStyle != null) {
//                template.fontStyle = (FontStyle) fontStyle.GetAttribute("value").GetValueAsInt();
//            }
//
//            XElement fontColor = element.GetChild("FontColor");
//            if (fontColor != null) {
//                template.fontColor = StyleParseUtil.ParseColor(fontColor.GetAttribute("value").Value);
//            }
//
//            XElement textOverflow = element.GetChild("TextOverflow");
//            if (textOverflow != null) {
//                template.overflowType = (TextOverflow) textOverflow.GetAttribute("value").GetValueAsInt();
//            }
//
//            XElement textAnchor = element.GetChild("TextAnchor");
//            if (textAnchor != null) {
//                template.textAnchor = (TextAnchor) textAnchor.GetAttribute("value").GetValueAsInt();
//            }

        }

    }

}