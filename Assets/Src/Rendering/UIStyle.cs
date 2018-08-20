using Src;
using System;
using UnityEngine;
using System.Diagnostics;
using Src.Layout;

namespace Rendering {

    [DebuggerDisplay("{localId}->{filePath}")]
    public class UIStyle {
        
        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        
        private static int NextStyleId;
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);
        public static readonly ContentBoxRect UnsetRectValue = new ContentBoxRect();
        public static readonly Vector2 UnsetVector2Value = new Vector2(float.MaxValue, float.MaxValue);
        public static readonly UIMeasurement UnsetMeasurementValue = new UIMeasurement(float.MaxValue, UIUnit.View);
        
        public readonly string filePath;
        public readonly string localId;

        public PaintDesc paint;
        
        public UILayoutRect rect;
        public UIMeasurement minWidth;
        public UIMeasurement maxWidth;
        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;
        public int growthFactor;
        public int shrinkFactor;
        
        public ContentBoxRect border;
        public ContentBoxRect margin;
        public ContentBoxRect padding;
        
        public LayoutType layoutType;
        public LayoutWrap layoutWrap;
        public LayoutFlowType layoutFlow;
        public LayoutDirection layoutDirection;
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;
        
        public event Action<UIStyle> onChange;

        public UIStyle(string localId, string filePath) {
            this.localId = localId;
            this.filePath = filePath;
            layoutType = LayoutType.Flow;
            paint = new PaintDesc();
            rect = new UILayoutRect();
        }

        public UIStyle() {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            layoutType = LayoutType.Flow;
            paint = new PaintDesc();
            rect = new UILayoutRect();
        }

        public UIStyle(UIStyle toCopy) {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            rect = toCopy.rect.Clone();
            paint = toCopy.paint.Clone();
            margin = toCopy.margin;
            border = toCopy.border;
            padding = toCopy.padding;
            layoutType = toCopy.layoutType;
        }

        public string Id => filePath == string.Empty ? localId : localId + "->" + filePath;

        public void ApplyChanges() {
            onChange?.Invoke(this);
        }
        
        public static readonly UIStyle Default = new UIStyle() {
            layoutType = LayoutType.Flex,
            layoutDirection = LayoutDirection.Column,
            layoutFlow  = LayoutFlowType.InFlow,
            layoutWrap =  LayoutWrap.None,
            
            rect = new UILayoutRect() {
                x = new UIMeasurement(),
                y = new UIMeasurement(),
                width = new UIMeasurement(100f, UIUnit.Parent),
                height = new UIMeasurement(100f, UIUnit.Parent)
            },
            paint = new PaintDesc() {
                borderColor = new Color(1f, 1f, 1f, 1f),
                backgroundColor = new Color(1f, 1f, 1f, 1f)
            } 
        };


    }

}