using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics.ShapeKit;
using UIForia.Layout;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public class ShapeContext {

        private UIVertexHelper vh;
        private ThisOtherThing.UI.ShapeUtils.ShapeKit shapeKit;
        private LayoutResult layoutResult;
        private Color color;
        private float2 position;

        public float elementWidth {
            get => layoutResult.ActualWidth;
        }

        public float elementHeight {
            get => layoutResult.ActualHeight;
        }

        public void SetPosition(float x, float y) {
            position = new float2(x, y);
        }

        public void SetColor(Color32 color) {
            this.color = color;
        }

        public void FillRect(float width, float height) {
            shapeKit.AddRect(ref vh, position, width, height, color);
        }

        public void FillSector(float width, float height, float length, ArcDirection direction) {
            float2 p = new float2(width, height);
            shapeKit.AddSector(ref vh, position, p, new ArcProperties() {
                direction = direction,
                length = length,
                resolution = default,
                baseAngle = 0
            }, color);

        }

        public static Color32 clear => new Color32(0, 0, 0, 0);
        public static Color32 transparent => new Color32(0, 0, 0, 0);
        public static Color32 black => new Color32(0, 0, 0, 255);
        public static Color32 indianred => new Color32(205, 92, 92, 255);
        public static Color32 lightcoral => new Color32(240, 128, 128, 255);
        public static Color32 salmon => new Color32(250, 128, 114, 255);
        public static Color32 darksalmon => new Color32(233, 150, 122, 255);
        public static Color32 lightsalmon => new Color32(255, 160, 122, 255);
        public static Color32 crimson => new Color32(220, 20, 60, 255);
        public static Color32 red => new Color32(255, 0, 0, 255);
        public static Color32 firebrick => new Color32(178, 34, 34, 255);
        public static Color32 darkred => new Color32(139, 0, 0, 255);
        public static Color32 pink => new Color32(255, 192, 203, 255);
        public static Color32 lightpink => new Color32(255, 182, 193, 255);
        public static Color32 hotpink => new Color32(255, 105, 180, 255);
        public static Color32 deeppink => new Color32(255, 20, 147, 255);
        public static Color32 mediumvioletred => new Color32(199, 21, 133, 255);
        public static Color32 palevioletred => new Color32(219, 112, 147, 255);
        public static Color32 coral => new Color32(255, 127, 80, 255);
        public static Color32 tomato => new Color32(255, 99, 71, 255);
        public static Color32 orangered => new Color32(255, 69, 0, 255);
        public static Color32 darkorange => new Color32(255, 140, 0, 255);
        public static Color32 orange => new Color32(255, 165, 0, 255);
        public static Color32 gold => new Color32(255, 215, 0, 255);
        public static Color32 yellow => new Color32(255, 255, 0, 255);
        public static Color32 lightyellow => new Color32(255, 255, 224, 255);
        public static Color32 lemonchiffon => new Color32(255, 250, 205, 255);
        public static Color32 lightgoldenrodyellow => new Color32(250, 250, 210, 255);
        public static Color32 papayawhip => new Color32(255, 239, 213, 255);
        public static Color32 moccasin => new Color32(255, 228, 181, 255);
        public static Color32 peachpuff => new Color32(255, 218, 185, 255);
        public static Color32 palegoldenrod => new Color32(238, 232, 170, 255);
        public static Color32 khaki => new Color32(240, 230, 140, 255);
        public static Color32 darkkhaki => new Color32(189, 183, 107, 255);
        public static Color32 lavender => new Color32(230, 230, 250, 255);
        public static Color32 thistle => new Color32(216, 191, 216, 255);
        public static Color32 plum => new Color32(221, 160, 221, 255);
        public static Color32 violet => new Color32(238, 130, 238, 255);
        public static Color32 orchid => new Color32(218, 112, 214, 255);
        public static Color32 fuchsia => new Color32(255, 0, 255, 255);
        public static Color32 magenta => new Color32(255, 0, 255, 255);
        public static Color32 mediumorchid => new Color32(186, 85, 211, 255);
        public static Color32 mediumpurple => new Color32(147, 112, 219, 255);
        public static Color32 blueviolet => new Color32(138, 43, 226, 255);
        public static Color32 darkviolet => new Color32(148, 0, 211, 255);
        public static Color32 darkorchid => new Color32(153, 50, 204, 255);
        public static Color32 darkmagenta => new Color32(139, 0, 139, 255);
        public static Color32 purple => new Color32(128, 0, 128, 255);
        public static Color32 rebeccapurple => new Color32(102, 51, 153, 255);
        public static Color32 indigo => new Color32(75, 0, 130, 255);
        public static Color32 mediumslateblue => new Color32(123, 104, 238, 255);
        public static Color32 slateblue => new Color32(106, 90, 205, 255);
        public static Color32 darkslateblue => new Color32(72, 61, 139, 255);
        public static Color32 greenyellow => new Color32(173, 255, 47, 255);
        public static Color32 chartreuse => new Color32(127, 255, 0, 255);
        public static Color32 lawngreen => new Color32(124, 252, 0, 255);
        public static Color32 lime => new Color32(0, 255, 0, 255);
        public static Color32 limegreen => new Color32(50, 205, 50, 255);
        public static Color32 palegreen => new Color32(152, 251, 152, 255);
        public static Color32 lightgreen => new Color32(144, 238, 144, 255);
        public static Color32 mediumspringgreen => new Color32(0, 250, 154, 255);
        public static Color32 springgreen => new Color32(0, 255, 127, 255);
        public static Color32 mediumseagreen => new Color32(60, 179, 113, 255);
        public static Color32 seagreen => new Color32(46, 139, 87, 255);
        public static Color32 forestgreen => new Color32(34, 139, 34, 255);
        public static Color32 green => new Color32(0, 128, 0, 255);
        public static Color32 darkgreen => new Color32(0, 100, 0, 255);
        public static Color32 yellowgreen => new Color32(154, 205, 50, 255);
        public static Color32 olivedrab => new Color32(107, 142, 35, 255);
        public static Color32 olive => new Color32(128, 128, 0, 255);
        public static Color32 darkolivegreen => new Color32(85, 107, 47, 255);
        public static Color32 mediumaquamarine => new Color32(102, 205, 170, 255);
        public static Color32 darkseagreen => new Color32(143, 188, 143, 255);
        public static Color32 lightseagreen => new Color32(32, 178, 170, 255);
        public static Color32 darkcyan => new Color32(0, 139, 139, 255);
        public static Color32 teal => new Color32(0, 128, 128, 255);
        public static Color32 aqua => new Color32(0, 255, 255, 255);
        public static Color32 cyan => new Color32(0, 255, 255, 255);
        public static Color32 lightcyan => new Color32(224, 255, 255, 255);
        public static Color32 paleturquoise => new Color32(175, 238, 238, 255);
        public static Color32 aquamarine => new Color32(127, 255, 212, 255);
        public static Color32 turquoise => new Color32(64, 224, 208, 255);
        public static Color32 mediumturquoise => new Color32(72, 209, 204, 255);
        public static Color32 darkturquoise => new Color32(0, 206, 209, 255);
        public static Color32 cadetblue => new Color32(95, 158, 160, 255);
        public static Color32 steelblue => new Color32(70, 130, 180, 255);
        public static Color32 lightsteelblue => new Color32(176, 196, 222, 255);
        public static Color32 powderblue => new Color32(176, 224, 230, 255);
        public static Color32 lightblue => new Color32(173, 216, 230, 255);
        public static Color32 skyblue => new Color32(135, 206, 235, 255);
        public static Color32 lightskyblue => new Color32(135, 206, 250, 255);
        public static Color32 deepskyblue => new Color32(0, 191, 255, 255);
        public static Color32 dodgerblue => new Color32(30, 144, 255, 255);
        public static Color32 cornflowerblue => new Color32(100, 149, 237, 255);
        public static Color32 royalblue => new Color32(65, 105, 225, 255);
        public static Color32 blue => new Color32(0, 0, 255, 255);
        public static Color32 mediumblue => new Color32(0, 0, 205, 255);
        public static Color32 darkblue => new Color32(0, 0, 139, 255);
        public static Color32 navy => new Color32(0, 0, 128, 255);
        public static Color32 midnightblue => new Color32(25, 25, 112, 255);
        public static Color32 cornsilk => new Color32(255, 248, 220, 255);
        public static Color32 blanchedalmond => new Color32(255, 235, 205, 255);
        public static Color32 bisque => new Color32(255, 228, 196, 255);
        public static Color32 navajowhite => new Color32(255, 222, 173, 255);
        public static Color32 wheat => new Color32(245, 222, 179, 255);
        public static Color32 burlywood => new Color32(222, 184, 135, 255);
        public static Color32 tan => new Color32(210, 180, 140, 255);
        public static Color32 rosybrown => new Color32(188, 143, 143, 255);
        public static Color32 sandybrown => new Color32(244, 164, 96, 255);
        public static Color32 goldenrod => new Color32(218, 165, 32, 255);
        public static Color32 darkgoldenrod => new Color32(184, 134, 11, 255);
        public static Color32 peru => new Color32(205, 133, 63, 255);
        public static Color32 chocolate => new Color32(210, 105, 30, 255);
        public static Color32 saddlebrown => new Color32(139, 69, 19, 255);
        public static Color32 sienna => new Color32(160, 82, 45, 255);
        public static Color32 brown => new Color32(165, 42, 42, 255);
        public static Color32 maroon => new Color32(128, 0, 0, 255);
        public static Color32 white => new Color32(255, 255, 255, 255);
        public static Color32 snow => new Color32(255, 250, 250, 255);
        public static Color32 honeydew => new Color32(240, 255, 240, 255);
        public static Color32 mintcream => new Color32(245, 255, 250, 255);
        public static Color32 azure => new Color32(240, 255, 255, 255);
        public static Color32 aliceblue => new Color32(240, 248, 255, 255);
        public static Color32 ghostwhite => new Color32(248, 248, 255, 255);
        public static Color32 whitesmoke => new Color32(245, 245, 245, 255);
        public static Color32 seashell => new Color32(255, 245, 238, 255);
        public static Color32 beige => new Color32(245, 245, 220, 255);
        public static Color32 oldlace => new Color32(253, 245, 230, 255);
        public static Color32 floralwhite => new Color32(255, 250, 240, 255);
        public static Color32 ivory => new Color32(255, 255, 240, 255);
        public static Color32 antiquewhite => new Color32(250, 235, 215, 255);
        public static Color32 linen => new Color32(250, 240, 230, 255);
        public static Color32 lavenderblush => new Color32(255, 240, 245, 255);
        public static Color32 mistyrose => new Color32(255, 228, 225, 255);
        public static Color32 gainsboro => new Color32(220, 220, 220, 255);
        public static Color32 lightgray => new Color32(211, 211, 211, 255);
        public static Color32 lightgrey => new Color32(211, 211, 211, 255);
        public static Color32 silver => new Color32(192, 192, 192, 255);
        public static Color32 darkgray => new Color32(169, 169, 169, 255);
        public static Color32 darkgrey => new Color32(169, 169, 169, 255);
        public static Color32 gray => new Color32(128, 128, 128, 255);
        public static Color32 grey => new Color32(128, 128, 128, 255);
        public static Color32 dimgray => new Color32(105, 105, 105, 255);
        public static Color32 dimgrey => new Color32(105, 105, 105, 255);
        public static Color32 lightslategray => new Color32(119, 136, 153, 255);
        public static Color32 lightslategrey => new Color32(119, 136, 153, 255);
        public static Color32 slategray => new Color32(112, 128, 144, 255);
        public static Color32 slategrey => new Color32(112, 128, 144, 255);
        public static Color32 darkslategray => new Color32(47, 79, 79, 255);
        public static Color32 darkslategrey => new Color32(47, 79, 79, 255);

    }

}