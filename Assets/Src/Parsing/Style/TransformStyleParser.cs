using System.Collections.Generic;
using System.Xml.Linq;
using Src.Style;

namespace Src.Parsing.Style {

    public class TransformStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;

            XElement position = element.GetChild("Position");
            if (position != null) {
                template.position = StyleParseUtil.ParseVector2(position.GetAttribute("value").Value);
            }

            XElement scale = element.GetChild("Scale");
            if (scale != null) {
                template.scale = StyleParseUtil.ParseVector2(scale.GetAttribute("value").Value);
            }
            
            XElement pivot = element.GetChild("Pivot");
            if (pivot != null) {
                template.pivot = StyleParseUtil.ParseVector2(pivot.GetAttribute("value").Value);
            }

        }

    }

}