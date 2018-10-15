using Rendering;
using Src;
using Src.Animation;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using UnityEngine;

using static Rendering.StyleProperty;

public class ChatWindow_Styles {

    //[ExportAnimation("transform")]
    public static StyleAnimation AnimateTransform() {
        return new AnimationGroup(
            new PropertyAnimation(
                PreferredWidth(500),
                new AnimationOptions() {
                    duration = 2f,
                    iterations = 2,
                    delay = 1f,
                    direction = AnimationDirection.Reverse,
                    loopType = AnimationLoopType.PingPong
                }
            )
        );

    }

    public static StyleAnimation KeyFrameAnimateTransform() {
        return new KeyFrameAnimation(
            new AnimationOptions() {
                duration = 5f
            },
            new AnimationKeyFrame(0f,
                BackgroundColor(Color.red),
                PreferredHeight(25f),
                PreferredWidth(100f)
            ),
            new AnimationKeyFrame(0.5f,
                BackgroundColor(Color.white),
                PreferredHeight(300f),
                TransformRotation(180f)
            ),
            new AnimationKeyFrame(0.7f,
                PreferredWidth(100f)
            ),
            new AnimationKeyFrame(0.75f,
                PreferredWidth(300f)
            ),
            new AnimationKeyFrame(1f,
                BackgroundColor(Color.blue),
                PreferredHeight(25f),
                PreferredWidth(25f),
                TransformRotation(0)
            )
        );
    }

    [ExportStyle("grid")]
    public static UIStyle Grid() {
        return new UIStyle() {
            LayoutType = LayoutType.Grid,
            GridLayoutColGapSize = 12f,
            GridLayoutRowGapSize = 12f,
            GridLayoutRowTemplate = new[] {
                GridTrackSize.MaxContent,
                new GridTrackSize(100f),
                new GridTrackSize(100f),
                new GridTrackSize(100f),
                new GridTrackSize(100f),
            },
            GridLayoutColTemplate = new[] {
                GridTrackSize.Flex,
                new GridTrackSize(100f)
            },
            GridLayoutRowAlignment = CrossAxisAlignment.Center,
            GridLayoutColAlignment = CrossAxisAlignment.Stretch,
            GridLayoutColAutoSize = 100f,
        };
    }

    [ExportStyle("grid-item")]
    public static UIStyle GridItem() {
        return new UIStyle() {
            BackgroundColor = Color.red,
            PreferredWidth = 100f,
            PreferredHeight = 100f,
            FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.Center,
        };
    }

    [ExportStyle("overflow")]
    public static UIStyle Overflow() {
        return new UIStyle() {
            BackgroundColor = Color.red,
            OverflowX = Src.Rendering.Overflow.Scroll,
            OverflowY = Src.Rendering.Overflow.Scroll
        };
    }

    [ExportStyle("grid-item-alt1")]
    public static UIStyle GridItemAlt() {
        return new UIStyle() {
//            GridItemColSelfAlignment =CrossAxisAlignment.Center,
            GridItemRowSelfAlignment = CrossAxisAlignment.Center,
            BackgroundColor = Color.blue,
            PreferredWidth = 100f,
            PreferredHeight = 100f,
            FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.Center,
        };
    }

    [ExportStyle("grid-item-alt2")]
    public static UIStyle GridItemAlt2() {
        return new UIStyle() {
//            GridItemColSelfAlignment =CrossAxisAlignment.Center,
            GridItemRowSelfAlignment = CrossAxisAlignment.Center,
            BackgroundColor = Color.green,
            PreferredWidth = 100f,
            PreferredHeight = 100f,
            FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.Center,
            GridItemColStart = 1,
            GridItemColSpan = 3
        };
    }

    [ExportStyle("container")]
    public static UIStyle Container() {
        return new UIStyle() {
            FlexLayoutDirection = LayoutDirection.Column,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.Start,
            PreferredWidth = 800f,
            PreferredHeight = 600f
        };
    }

    [ExportStyle("clipped-panel")]
    public static UIStyle ClippedPanel() {
        return new UIStyle() {
            BackgroundColor = Color.white,
            PreferredHeight = 300f
        };
    }

    [ExportStyle("direct-messages")]
    public static UIStyle DirectMessages() {
        return new UIStyle() {
            BackgroundColor = new Color(224f, 0, 0, 1f),
            PreferredWidth = 200f,
            PreferredHeight = UIMeasurement.Parent100,
            LayoutType = LayoutType.Flex,
            FlexLayoutDirection = LayoutDirection.Row
        };
    }

    [ExportStyle("direct-messages-header")]
    public static UIStyle DirectMessagesHeader() {
        return new UIStyle() {
            BackgroundColor = new Color(175, 175, 175, 1f),
            TextColor = Color.black
        };
    }

    [ExportStyle("window-header")]
    public static UIStyle WindowHeader() {
        return new UIStyle() {
            FlexLayoutDirection = LayoutDirection.Row,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween,
            PreferredWidth = new UIMeasurement(0.25f, UIUnit.ParentSize),
            PreferredHeight = new UIMeasurement(40f),
            BackgroundColor = Color.red
        };
    }

    [ExportStyle("header-text")]
    public static UIStyle HeaderText() {
        return new UIStyle() {
            Padding = new PaddingBox(20f),
            FontSize = 32,
            TextColor = Color.white
        };
    }

    [ExportStyle("text-style")]
    public static UIStyle Text() {
        return new UIStyle() {
            PreferredWidth = UIMeasurement.ContentArea
        };
    }

}