using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Src;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    [DebuggerDisplay("{localId}->{filePath}")]
    public class UIStyle {

        [PublicAPI]
        public readonly string filePath;
        public readonly string localId;

        public PaintDesc paint;
        public UILayoutRect rect;
        public ContentBox contentBox;
        public LayoutType layoutType;

        public event Action<UIStyle> onChange;

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);
        public static readonly ContentBoxRect UnsetRectValue = new ContentBoxRect();
        public static readonly Vector2 UnsetVector2Value = new Vector2(float.MaxValue, float.MaxValue);
        public static UIMeasurement UnsetMeasurementValue = new UIMeasurement(float.MaxValue, UIUnit.View);
        
        private static int NextStyleId;

        public UIStyle(string localId, string filePath) {
            this.localId = localId;
            this.filePath = filePath;
            layoutType = LayoutType.Flow;
            contentBox = new ContentBox();
            paint = new PaintDesc();
            rect = new UILayoutRect();
        }

        public UIStyle() {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            layoutType = LayoutType.Flow;
            contentBox = new ContentBox();
            paint = new PaintDesc();
            rect = new UILayoutRect();
        }

        public UIStyle(UIStyle toCopy) {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            contentBox = toCopy.contentBox.Clone();
            rect = toCopy.rect.Clone();
            paint = toCopy.paint.Clone();
            layoutType = toCopy.layoutType;
        }

        public string Id => filePath == string.Empty ? localId : localId + "->" + filePath;

        public void ApplyChanges() {
            onChange?.Invoke(this);
        }

        public bool RequiresRendering() {
            return paint.backgroundColor != UnsetColorValue;
        }

        public static readonly UIStyle Default = new UIStyle() {
            rect = new UILayoutRect() {
                x = new UIMeasurement(),
                y = new UIMeasurement(),
                width = new UIMeasurement(),
                height = new UIMeasurement()
            },
            paint = new PaintDesc() {
                borderColor = new Color(1f, 1f, 1f, 1f),
                backgroundColor = new Color(1f, 1f, 1f, 1f)
            } 
        };

    }

}