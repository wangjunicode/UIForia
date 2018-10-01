using Rendering;
using Src;
using Src.Layout;
using UnityEngine;

public class ChatWindow_Styles {

    [ExportStyle("container")]
    public static UIStyle Container() {
        return new UIStyle() {
                BackgroundColor = Color.blue
            
        };
    }

    [ExportStyle("window-header")]
    public static UIStyle WindowHeader() {
        return new UIStyle() {
            FlexLayoutDirection = LayoutDirection.Row,
            FlexLayoutMainAxisAlignment = MainAxisAlignment.SpaceBetween,
            PreferredWidth = new UIMeasurement(1f, UIUnit.ParentSize),
            PreferredHeight = new UIMeasurement(40f),
            BackgroundColor = Color.red
        };
    }

    [ExportStyle("header-text")]
    public static UIStyle HeaderText() {
        return new UIStyle() {
            Padding = new ContentBoxRect(20f),
            FontSize = 32,
            TextColor =  Color.white
        };
    }

}