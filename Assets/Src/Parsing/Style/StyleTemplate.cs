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
        public BorderStyle borderStyle = BorderStyle.Unset;
        public float borderRadius = UIStyle.UnsetFloatValue;
        public float opacity = UIStyle.UnsetFloatValue;
        public Material material = null;
        public Texture2D backgroundImage = null;

        // size
        public float paddingLeft = UIStyle.UnsetFloatValue;
        public float paddingRight = UIStyle.UnsetFloatValue;
        public float paddingTop = UIStyle.UnsetFloatValue;
        public float paddingBottom = UIStyle.UnsetFloatValue;
        public float marginLeft = UIStyle.UnsetFloatValue;
        public float marginRight = UIStyle.UnsetFloatValue;
        public float marginTop = UIStyle.UnsetFloatValue;
        public float marginBottom = UIStyle.UnsetFloatValue;
        public float borderTop = UIStyle.UnsetFloatValue;
        public float borderBottom = UIStyle.UnsetFloatValue;
        public float borderLeft = UIStyle.UnsetFloatValue;
        public float borderRight = UIStyle.UnsetFloatValue;

        public float width = UIStyle.UnsetFloatValue;
        public float height = UIStyle.UnsetFloatValue;
        public FitType widthFit = FitType.Unset;
        public FitType heightFit = FitType.Unset;

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

    }

}