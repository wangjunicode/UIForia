using Rendering;
using Src;
using Src.Layout;
using UnityEngine;

public class ChatWindow_Styles {

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