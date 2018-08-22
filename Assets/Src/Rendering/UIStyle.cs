using Src;
using System;
using UnityEngine;
using System.Diagnostics;
using Src.Layout;
using Src.Systems;

namespace Rendering {

    [DebuggerDisplay("{localId}->{filePath}")]
    public class UIStyle {

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatThreshold = 900f;
        public const float UnsetFloatValue = 1000f;
        
        
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);
        public static readonly UIMeasurement UnsetMeasurementValue = new UIMeasurement(UnsetFloatValue, UIUnit.View);

        private static int NextStyleId;
        
        public readonly string filePath;
        public readonly string localId;

        public Paint paint;
        public LayoutRect rect;

        public LayoutParameters layoutParameters;
        public LayoutConstraints layoutConstraints;

        public ContentBoxRect border;
        public ContentBoxRect margin;
        public ContentBoxRect padding;

        public event Action<UIStyle> onChange;

        public UIStyle(string localId, string filePath) {
            this.localId = localId;
            this.filePath = filePath;
            Initialize();
        }

        public UIStyle() {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            rect = new LayoutRect();
            Initialize();
        }

        public UIStyle(UIStyle toCopy) {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            rect = toCopy.rect;
            paint = toCopy.paint;
            margin = toCopy.margin;
            border = toCopy.border;
            padding = toCopy.padding;
            layoutParameters = toCopy.layoutParameters;
            layoutConstraints = toCopy.layoutConstraints;
        }

        public string Id => filePath == string.Empty ? localId : localId + "->" + filePath;

        public void ApplyChanges() {
            onChange?.Invoke(this);
        }

        private void Initialize() {
            rect = new LayoutRect() {
                x = UnsetMeasurementValue,
                y = UnsetMeasurementValue,
                width = UnsetMeasurementValue,
                height = UnsetMeasurementValue
            };
            layoutConstraints = new LayoutConstraints() {
                minWidth = UnsetFloatValue,
                maxWidth = UnsetFloatValue,
                minHeight = UnsetFloatValue,
                maxHeight = UnsetFloatValue,
                growthFactor = 0,
                shrinkFactor = 0
            };
            layoutParameters = new LayoutParameters() {
                type = LayoutType.Flex,
                direction = LayoutDirection.Column,
                flow = LayoutFlowType.InFlow,
                crossAxisAlignment = CrossAxisAlignment.Default,
                mainAxisAlignment = MainAxisAlignment.Default,
                wrap = LayoutWrap.None
            };
            margin = ContentBoxRect.Unset;
            padding = ContentBoxRect.Unset;
            border = ContentBoxRect.Unset;
            paint = Paint.Unset;
        }
        
        public static readonly UIStyle Default = new UIStyle("Default", string.Empty) {
            
            rect = new LayoutRect() {
                x = new UIMeasurement(),
                y = new UIMeasurement(),
                width = UIMeasurement.Content100,
                height = UIMeasurement.Content100
            },
            layoutConstraints = new LayoutConstraints() {
                minWidth = UnsetFloatValue,
                maxWidth = UnsetFloatValue,
                minHeight = UnsetFloatValue,
                maxHeight = UnsetFloatValue,
                growthFactor = 0,
                shrinkFactor = 0
            },
            layoutParameters = new LayoutParameters() {
                type = LayoutType.Flex,
                direction = LayoutDirection.Column,
                flow = LayoutFlowType.InFlow,
                crossAxisAlignment = CrossAxisAlignment.Default,
                mainAxisAlignment = MainAxisAlignment.Default,
                wrap = LayoutWrap.None
            },
            margin = new ContentBoxRect() {
                top = 0,
                right = 0,
                left = 0,
                bottom = 0
            },
            padding = new ContentBoxRect() {
                top = 0,
                right = 0,
                left = 0,
                bottom = 0
            },
            border = new ContentBoxRect() {
                top = 0,
                right = 0,
                left = 0,
                bottom = 0
            },
            paint = new Paint(
                new Color(1f, 1f, 1f, 1f),
                new Color(1f, 1f, 1f, 1f)
            )
        };

    }

}