using System.Collections.Generic;
using System.Diagnostics;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Rendering {

    public struct StyleProperty {

        public readonly StylePropertyId propertyId;
        public readonly int valuePart0;
        public readonly int valuePart1;
        public readonly object objectField;

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int value0, int value1 = 0, object objectField = null) {
            this.propertyId = propertyId;
            this.valuePart0 = value0;
            this.valuePart1 = value1;
            this.objectField = objectField;
        }

        public bool IsDefined {
            [DebuggerStepThrough] get { return IntUtil.IsDefined(valuePart0) && IntUtil.IsDefined(valuePart1); }
        }

        public int AsInt => valuePart0;
        public float AsFloat => FloatUtil.DecodeToFloat(valuePart0);
        public UIMeasurement AsMeasurement => UIMeasurement.Decode(valuePart0, valuePart1);
        public CrossAxisAlignment AsCrossAxisAlignment => (CrossAxisAlignment) valuePart0;
        public MainAxisAlignment AsMainAxisAlignment => (MainAxisAlignment) valuePart0;
        public Overflow AsOverflow => (Overflow) valuePart0;
        public Color AsColor => new StyleColor(valuePart0);
        
        // todo move these to use objectField
        public AssetPointer<TMP_FontAsset> AsFontAsset => new AssetPointer<TMP_FontAsset>((AssetType) valuePart0, valuePart1);
        public AssetPointer<Texture2D> AsTextureAsset => new AssetPointer<Texture2D>((AssetType) valuePart0, valuePart1);

        public TextUtil.FontStyle AsFontStyle => (TextUtil.FontStyle) valuePart0;
        public TextUtil.TextAlignment AsTextAlignment => (TextUtil.TextAlignment) valuePart0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) valuePart0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) valuePart0;
        public GridTrackSize AsGridTrackSize => new GridTrackSize(FloatUtil.DecodeToFloat(valuePart0), (GridTemplateUnit) valuePart1);

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue);
        }

    }

}