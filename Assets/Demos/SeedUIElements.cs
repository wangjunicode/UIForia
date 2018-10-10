using JetBrains.Annotations;
using Rendering;
using UnityEngine;

namespace Src {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Style classPath='Label+Style'/>
        <Contents>
            <Text style='label'>
                {text}
            </Text>
        </Contents>
    </UITemplate>
    ")]
    public class Label : UIElement {

        [UsedImplicitly] public string text;

        public static class Style {

            [ExportStyle("label")]
            public static UIStyle Label() {
                return new UIStyle() {
                    TextColor = new Color32(138, 138, 138, 255),
                    FontAsset = new FontAssetReference("Gotham-Medium SDF"),
                    FontSize = 20
                };
            }

        }

    }

}