using System;
using Src;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    public struct UILayoutRect {

        public UIMeasurement x;
        public UIMeasurement y;
        public UIMeasurement width;
        public UIMeasurement height;

    }

    public class UIStyle {

        public ContentBox contentBox;
        public LayoutParameters layoutParameters;
        public PaintDesc paint;
        public TextStyle textStyle = new TextStyle();

        public UILayoutRect rect;
        public LayoutType layoutType;

        public event Action<UIStyle> onChange;

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);
        public static readonly ContentBoxRect UnsetRectValue = new ContentBoxRect();
        public static readonly Vector2 UnsetVector2Value = new Vector2(float.MaxValue, float.MaxValue);

        public UIStyle() {
            layoutType = LayoutType.Flow;
            contentBox = new ContentBox();
            paint = new PaintDesc();
            layoutParameters = new LayoutParameters();
            rect = new UILayoutRect();
        }

        public static UIMeasurement UnsetMeasurementValue = new UIMeasurement(float.MaxValue, UIUnit.View);

        public void ApplyChanges() {
            onChange?.Invoke(this);
        }

        public bool RequiresRendering() {
            return paint.backgroundImage    != null
                   || paint.backgroundColor != UnsetColorValue
                   || paint.borderStyle     != BorderStyle.Unset
                   || paint.borderColor     != UnsetColorValue;
        }

        public static readonly UIStyle Default = new UIStyle() {
            rect = new UILayoutRect() {
                x = new UIMeasurement(),
                y = new UIMeasurement(),
                width = new UIMeasurement(),
                height = new UIMeasurement()
            }
        };

    }

}