using System;
using Src;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    [Flags]
    public enum StyleStateType {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1   << 0,
        Hover = 1    << 1,
        Active = 1   << 2,
        Disabled = 1 << 3,
        Focused = 1  << 4,

        Instance = 1 << 5,
        Base = 1     << 6,

        IsState = Hover | Active | Disabled | Focused,
        InstanceNormal = Instance           | Normal,
        InstanceHover = Instance            | Hover,
        InstanceActive = Instance           | Active,
        InstanceDisabled = Instance         | Disabled,
        InstanceFocused = Instance          | Focused,

        BaseNormal = Base   | Normal,
        BaseHover = Base    | Hover,
        BaseActive = Base   | Active,
        BaseDisabled = Base | Disabled,
        BaseFocused = Base  | Focused,

    }

    public static class BitUtil {

        public static int SetHighLowBits(int high, int low) {
            return (high << 16) | (low & 0xffff);
        }

        public static int GetHighBits(int input) {
            return (input >> 16) & (1 << 16) - 1;
        }

        public static int GetLowBits(int input) {
            return input & 0xffff;
        }

    }

    public class UIStyle {

        public UILayout layout;
        public ContentBox contentBox;
        public TransformDesc transform;
        public LayoutParameters layoutParameters;
        public PaintDesc paint;
        public TextStyle textStyle = new TextStyle();
        public event Action<UIStyle> onChange;

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);
        public static readonly UIStyleRect UnsetRectValue = new UIStyleRect(float.MaxValue);
        public static readonly Vector2 UnsetVector2Value = new Vector2(float.MaxValue, float.MaxValue);

        public UIStyle() {
            layout = new UILayout_Auto();
            contentBox = new ContentBox();
            paint = new PaintDesc();
            layoutParameters = new LayoutParameters();
        }

        public static UIMeasurement UnsetMeasurementValue { get; set; }


        public void ApplyChanges() {
            onChange?.Invoke(this);
        }

        public bool RequiresRendering() {
            return paint.backgroundImage    != null
                   || paint.backgroundColor != UnsetColorValue
                   || paint.borderStyle     != BorderStyle.Unset
                   || paint.borderColor     != UnsetColorValue;
        }

    }

}