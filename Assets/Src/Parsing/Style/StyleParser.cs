using System.Xml.Linq;
using Rendering;
using Src.Style;
using UnityEngine;

namespace Src.Parsing.Style {

    public static class StyleParser {

        public static void ParseStyle(XElement root, UIStyle style) {
            if (root == null) return;

            StyleParseUtil.ParseMeasurement(ref style.dimensions.width, root.GetChild("Rect.W"));
            StyleParseUtil.ParseMeasurement(ref style.dimensions.height, root.GetChild("Rect.H"));

//            Color borderColor = UIStyle.UnsetColorValue;
//            Color backgroundColor = UIStyle.UnsetColorValue;
//            Texture2D backgroundImage = null;
//            
//            StyleParseUtil.ParseColor(ref backgroundColor, root.GetChild("Paint.BackgroundColor"));
        }

    }

}