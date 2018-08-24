using Src;
using System;
using UnityEngine;
using System.Diagnostics;
using Src.Layout;
using Src.Systems;

namespace Rendering {

    [DebuggerDisplay("{localId}->{filePath}")]
    public class UIStyle {        
       
        private static int NextStyleId;
        
        public readonly string filePath;
        public readonly string localId;

        public Paint paint;
        public LayoutRect rect;

        public LayoutParameters layoutParameters;
        public LayoutConstraints layoutConstraints;
        
        public BorderRadius borderRadius;
        
        public ContentBoxRect border;
        public ContentBoxRect margin;
        public ContentBoxRect padding;

        public TextStyle text;
        
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
            borderRadius = toCopy.borderRadius;
            layoutParameters = toCopy.layoutParameters;
            layoutConstraints = toCopy.layoutConstraints;
        }

        public string Id => filePath == string.Empty ? localId : localId + "->" + filePath;

        public void ApplyChanges() {
            onChange?.Invoke(this);
        }

        private void Initialize() {
            rect = LayoutRect.Unset;
            layoutConstraints = LayoutConstraints.Unset;
            margin = ContentBoxRect.Unset;
            padding = ContentBoxRect.Unset;
            border = ContentBoxRect.Unset;
            paint = Paint.Unset;
            borderRadius = BorderRadius.Unset;
            layoutParameters = new LayoutParameters() {
                type = LayoutType.Flex,
                direction = LayoutDirection.Column,
                flow = LayoutFlowType.InFlow,
                crossAxisAlignment = CrossAxisAlignment.Default,
                mainAxisAlignment = MainAxisAlignment.Default,
                wrap = LayoutWrap.None
            };
        }
        
        public static readonly UIStyle Default = new UIStyle("Default", string.Empty) {
            rect = new LayoutRect() {
                x = new UIMeasurement(),
                y = new UIMeasurement(),
                width = UIMeasurement.Auto,
                height = UIMeasurement.Auto
            },
            layoutConstraints = new LayoutConstraints() {
                minWidth = UIMeasurement.Unset,
                maxWidth = UIMeasurement.Unset,
                minHeight = UIMeasurement.Unset,
                maxHeight = UIMeasurement.Unset,
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
            borderRadius = BorderRadius.Unset,
            paint =  Paint.Unset,
            text = new TextStyle(
                Color.black,
                null, 
                12, 
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                HorizontalWrapMode.Overflow,
                VerticalWrapMode.Overflow
            )
        };

    }

}