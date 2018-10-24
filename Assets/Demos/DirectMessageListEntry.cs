using Rendering;
using Src.Elements;
using Src.Layout;
using UnityEngine;

namespace Src {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Style classPath='DirectMessageListEntry+Style'/>
        <Import value='UnityEngine.Color' as='Color'/>

        <Contents style='container'>
    
            <Image style='icon' src='{url(chatData.iconUrl)}'/>
            <Text style='name'>{chatData.name}</Text>
            <Div style.growthFactor='1'/>
            <Circle color='{rgb(255, 0, 255)}'/>

        </Contents>
    </UITemplate>
    ")]
    public class DirectMessageListEntry : UIElement {

        public bool isSelected;
        public ChatData chatData;

        public static class Style {

            [ExportStyle("container")]
            public static UIStyle Container() {
                return new UIStyle() {
                    FlexLayoutDirection = LayoutDirection.Column,
                    FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Center,
                    Padding = new FixedLengthRect(12f),
                    PreferredWidth = UIMeasurement.Parent100,
                };
            }

            [ExportStyle("icon")]
            public static UIStyle Icon() {
                return new UIStyle() {
                    PaddingRight = 12f,
                    PaddingLeft = 12f,
                    PreferredWidth = 32f,
                    PreferredHeight = 32f,
                };
            }

            [ExportStyle("name")]
            public static UIStyle Name() {
                return new UIStyle() {
//                    FontAsset = new FontAssetReference("Gotham-Medium SDF"),
                    FontSize = 18
                };
            }

        }

    }

    public class Circle : UIContainerElement, IPropertyChangedHandler {

        public float size = 16f;
        public Color color = Color.red;
        public Color outlineColor = ColorUtil.UnsetValue;
        public float outlineSize = 0f;
        
        public override void OnReady() {
            style.SetBorderRadius(new BorderRadius(0.5f), StyleState.Normal);
            style.SetPreferredWidth(size, StyleState.Normal);
            style.SetPreferredHeight(size, StyleState.Normal);
            style.SetBackgroundColor(color, StyleState.Normal);
            style.SetBorderColor(outlineColor, StyleState.Normal);
            style.SetBorderTop(outlineSize, StyleState.Normal);
        }
        
        public void OnPropertyChanged(string propertyName, object oldValue) {
            if (propertyName == nameof(color)) {
                style.SetBackgroundColor(color, StyleState.Normal);
            }
            else if (propertyName == nameof(size)) {
                style.SetPreferredWidth(size, StyleState.Normal);
                style.SetPreferredHeight(size, StyleState.Normal);
            }
            else if (propertyName == nameof(outlineColor)) {
                style.SetBorderColor(outlineColor, StyleState.Normal);
            }
            else if (propertyName == nameof(outlineSize)) {
                style.SetBorderTop(outlineSize, StyleState.Normal);
            }
        }

    }

}