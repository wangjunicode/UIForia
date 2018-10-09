using Src;
using Src.Layout;
using Src.Rendering;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public static class DefaultStyleValues {

        public static readonly UIMeasurement borderRadiusTopLeft = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusTopRight = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusBottomRight = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusBottomLeft = new UIMeasurement(0);

        public static readonly Color borderColor = ColorUtil.UnsetValue;
        public static readonly Color backgroundColor = ColorUtil.UnsetValue;
        public static readonly Texture2DAssetReference backgroundImage = new Texture2DAssetReference(IntUtil.UnsetValue);

        public static readonly Overflow overflowX = Overflow.None;
        public static readonly Overflow overflowY = Overflow.None;

        public static readonly int flexOrderOverride = ushort.MaxValue;
        public static readonly int flexGrowthFactor = 0;
        public static readonly int flexShrinkFactor = 0;
        public static readonly CrossAxisAlignment flexSelfAlignment = CrossAxisAlignment.Unset;

        public static readonly LayoutWrap flexLayoutWrap = LayoutWrap.None;
        public static readonly LayoutDirection flexLayoutDirection = LayoutDirection.Row;
        public static readonly MainAxisAlignment flexLayoutMainAxisAlignment = MainAxisAlignment.Start;
        public static readonly CrossAxisAlignment flexLayoutCrossAxisAlignment = CrossAxisAlignment.Start;

        public static readonly UIMeasurement minWidth = new UIMeasurement(0);
        public static readonly UIMeasurement maxWidth = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement preferredWidth = UIMeasurement.Content100;
        public static readonly UIMeasurement minHeight = new UIMeasurement(0);
        public static readonly UIMeasurement maxHeight = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement preferredHeight = UIMeasurement.Content100;

        public static readonly UIMeasurement marginTop = new UIMeasurement(0);
        public static readonly UIMeasurement marginRight = new UIMeasurement(0);
        public static readonly UIMeasurement marginLeft = new UIMeasurement(0);
        public static readonly UIMeasurement marginBottom = new UIMeasurement(0);

        public static readonly UIFixedLength paddingTop = new UIFixedLength(0);
        public static readonly UIFixedLength paddingRight = new UIFixedLength(0);
        public static readonly UIFixedLength paddingLeft = new UIFixedLength(0);
        public static readonly UIFixedLength paddingBottom = new UIFixedLength(0);

        public static readonly UIFixedLength borderTop = new UIFixedLength(0);
        public static readonly UIFixedLength borderRight = new UIFixedLength(0);
        public static readonly UIFixedLength borderLeft = new UIFixedLength(0);
        public static readonly UIFixedLength borderBottom = new UIFixedLength(0);

        public static readonly UIFixedLength transformPositionX = new UIFixedLength(0);
        public static readonly UIFixedLength transformPositionY = new UIFixedLength(0);
        public static readonly UIFixedLength transformPivotX = new UIFixedLength(0);
        public static readonly UIFixedLength transformPivotY = new UIFixedLength(0);
        public static readonly float transformScaleX = 1;
        public static readonly float transformScaleY = 1;
        public static readonly float transformRotation = 0;

        public static readonly LayoutType layoutType = LayoutType.Flex;
        public static readonly int fontSize = 12;
        public static readonly Color textColor = Color.black;
        public static readonly TextUtil.FontStyle fontStyle = TextUtil.FontStyle.Normal;
        public static readonly FontAssetReference fontAsset = new FontAssetReference("default");
        public static readonly TextUtil.TextAlignment TextAlignment = TextUtil.TextAlignment.Left;
        public static readonly TextUtil.TextTransform textTransform = TextUtil.TextTransform.None;

        public static readonly LayoutBehavior layoutBehavior = LayoutBehavior.Normal;
        
        //IgnoreTranslationInLayout
        //UseScaledAABB

    }

}