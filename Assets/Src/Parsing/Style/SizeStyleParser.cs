using System.Xml.Linq;
using Src.Style;

namespace Src.Parsing.Style {

    public static class SizeStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;

            XElement margin = element.GetChild("Margin");
            XElement padding = element.GetChild("Padding");
            XElement border = element.GetChild("Border");
            XElement contentWidth = element.GetChild("ContentWidth");
            XElement contentHeight = element.GetChild("ContentHeight");

            if (margin != null) {
                template.margin = StyleParseUtil.ParseContentBox(margin.GetAttribute("value").Value);
            }

            if (padding != null) {
                template.padding = StyleParseUtil.ParseContentBox(padding.GetAttribute("value").Value);
            }

            if (border != null) {
                template.border = StyleParseUtil.ParseContentBox(border.GetAttribute("value").Value);
            }

            if (contentWidth != null) {
                template.width = ParseUnitValue(contentWidth.GetAttribute("value").Value);
            }

            if (contentHeight != null) {
                template.height = ParseUnitValue(contentHeight.GetAttribute("value").Value);
            }

        }

        private static float ParseUnitValue(string value) {
            return float.Parse(value);
        }

        
    }

}