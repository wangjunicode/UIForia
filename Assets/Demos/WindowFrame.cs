using System.Collections.Generic;
using Rendering;
using Src.Input;
using Src.Layout;
using Src.Rendering;
using UnityEngine;

namespace Src {

    [Template("Demos/AddChatButton.xml")]
    public class AddChatButton : UIElement { }

    [Template("Demos/WindowFrame.xml")]
    public class WindowFrame : UIElement {

        public List<ChatGroup> chatGroups = new List<ChatGroup>();

        private UIElement shuttle;
        private ChatGroup selected;

        private float startY;
        private float targetY;
        private float animationTime;

        public override void OnReady() {
            chatGroups.Add(new ChatGroup("icon_1", "Matt 0", 3));
            chatGroups.Add(new ChatGroup("icon_2", "Matt 1", 1));
            chatGroups.Add(new ChatGroup("icon_3", "Matt 2", 5));
            chatGroups.Add(new ChatGroup("icon_4", "Matt 3", 0));
            shuttle = FindById("shuttle");
        }

        public override void OnUpdate() {
            animationTime += Time.deltaTime;
            float t = Easing.Interpolate(animationTime, EasingFunction.QuadraticEaseIn) / 0.1f;
            if (t <= 1f) {
                shuttle.style.SetTransformPositionY(Mathf.Lerp(startY, targetY, t), StyleState.Normal);
            }
            else {
                shuttle.style.SetTransformPositionY(targetY, StyleState.Normal);
            }
        }

        // Measurement
        // Length
        // Int
        // Float
        // Enum ?
        // Color
        // Texture? 
        // UV Coord

        public void SetCurrentChat(UIElement chatGroupElement) {
            ChatGroupIcon target = (ChatGroupIcon) chatGroupElement;
            if (target.chatGroup == selected) return;
            selected = target.chatGroup;
            startY = shuttle.layoutResult.localPosition.y;
            targetY = target.layoutResult.localPosition.y;
            animationTime = 0;
        }

        public class Styles {

            [ExportStyle("top-bar")]
            public static UIStyle TopBar() {
                return new UIStyle() {
                    BackgroundColor = Color.black,
                    PaddingLeft = 100,
                    LayoutType = LayoutType.Flex,
                    FlexLayoutDirection = LayoutDirection.Column,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween,
                    PreferredWidth = 700f
                };
            }

            [ExportStyle("top-bar-text")]
            public static UIStyle HeaderText() {
                return new UIStyle() {
                    TextColor = Color.white,
                    FontSize = 24,
//                    TextTransform = TextUtil.TextTransform.UpperCase,
                    FontAsset = new FontAssetReference("Gotham-Medium SDF"),
                };
            }

            [ExportStyle("side-bar")]
            public static UIStyle SideBar() {
                return new UIStyle() {
                    TextColor = Color.white,
                    PreferredWidth = 100f,
                    BackgroundColor = Color.black,
                    PaddingBottom = 20f,
                    PreferredHeight = UIMeasurement.Content100 // todo -- add custom layout preference to elements a-la IDrawable 
                };
            }

            [ExportStyle("round-thing")]
            public static UIStyle RoundThing() {
                return new UIStyle() {
                    PreferredWidth = 16,
                    PreferredHeight = 16f,
                    BackgroundColor = Color.black,
                    BorderColor = Color.red,
                    BorderTop = 10f,
                    BorderRadius = 0.5f
                };
            }

            [ExportStyle("header-group")]
            public static UIStyle HeaderGroup() {
                return new UIStyle() {
                    FlexLayoutDirection = LayoutDirection.Column,
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center
                };
            }

            [ExportStyle("side-bar-icon-track")]
            public static UIStyle SidebarIconTrack() {
                return new UIStyle() {
                    PreferredWidth = UIMeasurement.Parent100,
                    PreferredHeight = new UIMeasurement(1.2f, UIUnit.Content),
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween
                };
            }

            [ExportStyle("side-bar-icon")]
            public static UIStyle SidebarIcon() {
                return new UIStyle() {
                    PreferredWidth = 64f,
                    PreferredHeight = 64f,
                    PaddingBottom = 10f,
                };
            }

            [ExportStyle("content-container")]
            public static UIStyle ContentContainer() {
                return new UIStyle() {
                    FlexLayoutDirection = LayoutDirection.Column,
                    PreferredWidth = 800f,
                    PreferredHeight = 600f
                };
            }

            [ExportStyle("message-panel")]
            public static UIStyle MessagePanel() {
                return new UIStyle() {
                    BackgroundColor = new Color32(224, 224, 224, 255),
                    PreferredWidth = 128f,
                    PreferredHeight = UIMeasurement.Parent100,
                };
            }

            [ExportStyle("message-main-area")]
            public static UIStyle MessageMain() {
                return new UIStyle() {
                    BackgroundColor = Color.white,
                    FlexItemGrowthFactor = 1,
                    PreferredHeight = UIMeasurement.Parent100,
                };
            }

            [ExportStyle("message-panel-secondary")]
            public static UIStyle MessageSecondary() {
                return new UIStyle() {
                    PreferredWidth = 128f,
                    PreferredHeight = UIMeasurement.Parent100,
                    BackgroundColor = new Color32(224, 224, 224, 255)
                };
            }

            [ExportStyle("shuttle")]
            public static UIStyle Shuttle() {
                return new UIStyle() {
                    LayoutBehavior = LayoutBehavior.Ignored,
                    PreferredWidth = 8f,
                    PreferredHeight = 64f,
                    BackgroundColor = Color.white,
                };
            }

        }

    }

}