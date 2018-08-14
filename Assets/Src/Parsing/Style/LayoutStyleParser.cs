using System.Xml.Linq;
using Src.Style;

namespace Src.Parsing.Style {

    public static class LayoutStyleParser {

        public static void ParseStyle(XElement root, StyleTemplate template) {
            if (root == null) return;
            StyleParseUtil.ParseMeasurement(ref template.rectX, root.GetChild("Rect.X"));
            StyleParseUtil.ParseMeasurement(ref template.rectY, root.GetChild("Rect.Y"));
            StyleParseUtil.ParseMeasurement(ref template.rectW, root.GetChild("Rect.W"));
            StyleParseUtil.ParseMeasurement(ref template.rectH, root.GetChild("Rect.H"));
        }

    }

}