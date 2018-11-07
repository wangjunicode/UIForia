using UIForia.Rendering;
using UIForia;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;
using UIForia.Text;
using TextAlignment = UIForia.Text.TextAlignment;

namespace Demo {

    [Template("Demos/ChatGroupIcon.xml")]
    public class ChatGroupIcon : UIElement {

        public ChatGroup chatGroup;

        public class Styles {

            [ExportStyle("container")]
            public static UIStyle Container() {
                return new UIStyle() {
                    PreferredWidth = 64f,
                    PreferredHeight = 64f
                };
            }
            
            [ExportStyle("chat-icon")]
            public static UIStyle ChatIcon() {
                return new UIStyle() {
                    PreferredWidth = 64f,
                    PreferredHeight = 64f
                };
            }
             
            [ExportStyle("unread-indicator")]
            public static UIStyle UnreadIndicator() {
                return new UIStyle() {
                    PreferredWidth = 32f,
                    PreferredHeight = 32f,
                    LayoutBehavior = LayoutBehavior.Ignored,
                    TransformPositionX = new UIFixedLength(1.25f, UIFixedUnit.Percent),
                    TransformPositionY = new UIFixedLength(1.25f, UIFixedUnit.Percent),
                    BackgroundColor = new Color32(255, 84, 84, 255),
                    BorderRadius = 0.5f,
                    BorderTop = 10f,
                    BorderColor = Color.black,
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.Center
                };
            }
            
            [ExportStyle("unread-count")]
            public static UIStyle UnreadCount() {
                return new UIStyle() {
                    TextColor = Color.white,
                    TextAlignment = TextAlignment.Center,
                    TextFontSize = 20
                };
            }

        }

    }

}