using Rendering;
using Src;
using Src.Layout;
using UnityEngine;

public class ChatWindow_Styles {

    [ExportStyle("container")]
    public static UIStyle Container() {
        return new UIStyle() {
            paint = new Paint() {
                backgroundColor = Color.blue
            }
        };
    }

    [ExportStyle("window-header")]
    public static UIStyle WindowHeader() {
        return new UIStyle() {
            layoutParameters = new LayoutParameters() {
                direction = LayoutDirection.Row,
                mainAxisAlignment = MainAxisAlignment.SpaceBetween
            },
            dimensions = new Dimensions() {
                width = new UIMeasurement(1f, UIUnit.ParentSize),
                height = new UIMeasurement(40f)
            },
            paint = new Paint() {
                backgroundColor = Color.red
            }
        };
    }

    [ExportStyle("header-text")]
    public static UIStyle HeaderText() {
        return new UIStyle() {
            padding = new ContentBoxRect(20f),
            textStyle = new TextStyle() {
                fontSize = 32,
                color = Color.white
            }
        };
    }

}