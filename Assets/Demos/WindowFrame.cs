using System.Collections.Generic;
using Rendering;
using Src.Layout;
using Src.Rendering;
using TMPro;
using UnityEngine;

namespace Src {

    [Template("Demos/AddChatButton.xml")]
    public class AddChatButton : UIElement { }

    [Template("Demos/WindowFrame.xml")]
    public class WindowFrame : UIElement {

        public List<ChatGroup> chatGroups = new List<ChatGroup>();
        public List<ChatData> activeChats = new List<ChatData>();

        private UIElement shuttle;
        private ChatGroup selected;

        private float startY;
        private float targetY;
        private float animationTime;
        private int selectedChatIndex;

        public override void OnReady() {
            chatGroups.Add(new ChatGroup("icon_1", "Matt 0", 3));
            chatGroups.Add(new ChatGroup("icon_2", "Matt 1", 1));
            chatGroups.Add(new ChatGroup("icon_3", "Matt 2", 5));
            chatGroups.Add(new ChatGroup("icon_4", "Matt 3", 0));
            activeChats.Add(new ChatData("Vondi", "icon_3", true, 0));
            activeChats.Add(new ChatData("Byrne", "icon_5", true, 2));
            activeChats.Add(new ChatData("Little", "icon_9", false, 0));
            shuttle = FindById("shuttle");
        }

//style.TransformY.Animate(target, curve, time)
        public override void OnUpdate() {
            animationTime += Time.deltaTime;
            float t = Easing.Interpolate(animationTime, EasingFunction.QuadraticEaseIn) / 0.1f;
            if (shuttle == null) return;
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

        public void AddChatGroup() {
            chatGroups.Add(new ChatGroup("icon_5", "Matt " + chatGroups.Count, Random.Range(0, 10)));
        }

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
                    PaddingTop = 6f,
                    PaddingBottom = 6f,
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
                    PreferredWidth = 200f,
//                    TextTransform = TextUtil.TextTransform.UpperCase,
                    FontAsset = Resources.Load<TMP_FontAsset>("Gotham-Medium SDF"),
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
                    PaddingRight = 12f,
                    PreferredWidth = new UIMeasurement(1.2f, UIMeasurementUnit.Content),
                    FlexLayoutDirection = LayoutDirection.Column,
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween
                };
            }

            [ExportStyle("side-bar-icon-track")]
            public static UIStyle SidebarIconTrack() {
                return new UIStyle() {
                    PreferredWidth = UIMeasurement.Parent100,
                    PreferredHeight = new UIMeasurement(1.2f, UIMeasurementUnit.Content),
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween
                };
            }

            [ExportStyle("side-bar-icon")]
            public static UIStyle SidebarIcon() {
                return new UIStyle() {
                    PreferredWidth = 48,
                    PreferredHeight = 48,
                    PaddingBottom = 10f,
                };
            }

            [ExportStyle("content-container")]
            public static UIStyle ContentContainer() {
                return new UIStyle() {
                    FlexLayoutDirection = LayoutDirection.Column,
                    PreferredWidth = 1200f,
                    PreferredHeight = 600f
                };
            }

            [ExportStyle("message-panel")]
            public static UIStyle MessagePanel() {
                return new UIStyle() {
                    BackgroundColor = new Color32(224, 224, 224, 255),
                    PreferredWidth = new UIMeasurement(0.22f, UIMeasurementUnit.ParentSize),
                    PreferredHeight = UIMeasurement.Parent100,
                };
            }

            [ExportStyle("message-panel-header")]
            public static UIStyle MessagePanelHeader() {
                return new UIStyle() {
                    Padding = new FixedLengthRect(12f),
                    PreferredWidth = UIMeasurement.Parent100,
                    FlexLayoutDirection = LayoutDirection.Column,
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween
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

            [ExportStyle("input-field")]
            public static UIStyle InputField() {
                return new UIStyle() {
                    PreferredWidth = UIMeasurement.ContentArea,
                    BackgroundColor = Color.white,
                };
            }

        }

    }

}