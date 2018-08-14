using Rendering;
using Src.Layout;
using UnityEngine;

namespace Src {

    public class StyleTemplate {

        public string id;
        public string extendsId;
        public string extendsPath;
        
        // text
        public int fontSize = UIStyle.UnsetIntValue;
        public string fontAssetName = string.Empty;
        public FontStyle fontStyle = FontStyle.Normal; // todo -- should be unset
        public TextAnchor textAnchor = TextAnchor.MiddleLeft;
        public Color fontColor = UIStyle.UnsetColorValue;
        public TextOverflow overflowType = TextOverflow.Unset;

        // paint
        public Visibility visibility = Visibility.Unset;
        public Color backgroundColor = UIStyle.UnsetColorValue;
        public Color borderColor = UIStyle.UnsetColorValue;
        public BorderStyle borderStyle = BorderStyle.Unset;
        public float borderRadiusTopLeft = UIStyle.UnsetFloatValue;
        public float borderRadiusTopRight = UIStyle.UnsetFloatValue;
        public float borderRadiusBottomLeft = UIStyle.UnsetFloatValue;
        public float borderRadiusBottomRight = UIStyle.UnsetFloatValue;
        public float opacity = UIStyle.UnsetFloatValue;
        public Material material = null;
        public Texture2D backgroundImage = null;

        // rect
        public ContentBoxRect margin;
        public ContentBoxRect padding;
        public ContentBoxRect border;
        public float width = UIStyle.UnsetFloatValue;
        public float height = UIStyle.UnsetFloatValue;

        public UIMeasurement rectX = UIStyle.UnsetMeasurementValue;
        public UIMeasurement rectY = UIStyle.UnsetMeasurementValue;
        public UIMeasurement rectW = UIStyle.UnsetMeasurementValue;
        public UIMeasurement rectH = UIStyle.UnsetMeasurementValue;

        public UIMeasurement rectMinW;
        public UIMeasurement rectMaxW;
        public UIMeasurement rectMinH;
        public UIMeasurement rectMaxH;

        public UIMeasurement paddingTop;
        public UIMeasurement paddingRight;
        public UIMeasurement paddingBottom;
        public UIMeasurement paddingLeft;

        // transform
        public Vector2 scale;
        public Vector2 pivot;
        public Vector2 position;

        // layout
        public LayoutType layoutType = LayoutType.Unset;
        public CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Default;
        public MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Default;
        public LayoutWrap layoutWrap = LayoutWrap.Unset;
        public LayoutDirection layoutDirection = LayoutDirection.Unset;

        // layout item
        public int growthFactor = UIStyle.UnsetIntValue;
        public int shrinkFactor = UIStyle.UnsetIntValue;
        public float minWidth = UIStyle.UnsetFloatValue;
        public float maxWidth = UIStyle.UnsetFloatValue;
        public float minHeight = UIStyle.UnsetFloatValue;
        public float maxHeight = UIStyle.UnsetFloatValue;
        public float basisWidth = UIStyle.UnsetFloatValue;
        public float basisHeight = UIStyle.UnsetFloatValue;

        public UIStyle Instantiate() {
            UIStyle style = new UIStyle();

            style.contentBox = new ContentBox();
            style.contentBox.border = border;
            style.contentBox.margin = margin;
            style.contentBox.padding = padding;

            style.rect.x = rectX;
            style.rect.y = rectY;
            style.rect.width = rectW;
            style.rect.height = rectH;

            return style;
        }

    }

}