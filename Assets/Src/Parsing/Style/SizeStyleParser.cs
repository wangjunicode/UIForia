using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Src.Style;

namespace Src.Parsing.Style {

    public static class SizeStyleParser {

        public static void ParseStyle(XElement element, StyleTemplate template) {
            if (element == null) return;
            IEnumerable<XElement> nodes = element.Elements("Size");
            foreach (var node in nodes) {
                XElement margin = node.GetChild("Margin");
                XElement padding = node.GetChild("Padding");
                XElement border = node.GetChild("Border");
                XElement contentWidth = node.GetChild("ContentWidth");
                XElement contentHeight = node.GetChild("ContentHeight");

                if (margin != null) {
                    template.margin = ParseContentBox(margin.GetAttribute("value").Value);
                }

                if (padding != null) {
                    template.padding = ParseContentBox(padding.GetAttribute("value").Value);
                }

                if (border != null) {
                    template.border = ParseContentBox(border.GetAttribute("value").Value);
                }

                if (contentWidth != null) {
                    template.width = ParseUnitValue(contentWidth.GetAttribute("value").Value);
                }
                
                if (contentHeight != null) {
                    template.height = ParseUnitValue(contentHeight.GetAttribute("value").Value);
                }
                
            }
        }

        private static float ParseUnitValue(string value) {
            return float.Parse(value);
        }

        private static ContentBoxPart ParseContentBox(string value) {
            ContentBoxPart box = new ContentBoxPart();

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

    }

}