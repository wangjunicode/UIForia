using Src;
using UnityEngine;
using System.Diagnostics;
using Src.Layout;
using Src.Rendering;
using Src.Systems;

namespace Rendering {

    public struct StyleProperty {

        public readonly int category;
        public readonly int value;

    }

    public struct StyleValue {

        public int firstWord;
        public int secondWord;

    }
    
    
    
    [DebuggerDisplay("{localId}->{filePath}")]
    public class UIStyle {        
       
        private static int NextStyleId;
        
        public readonly string filePath;
        public readonly string localId;

        public Paint paint;
        public Dimensions dimensions;
        public Overflow overflowX;
        public Overflow overflowY;
        public LayoutParameters layoutParameters;
        public LayoutConstraints layoutConstraints;
        
        public BorderRadius borderRadius;
        
        public ContentBoxRect border;
        public ContentBoxRect margin;
        public ContentBoxRect padding;

        public TextStyle textStyle;
        public UITransform transform;
        
        public UIStyle(string localId, string filePath) {
            this.localId = localId;
            this.filePath = filePath;
            Initialize();
        }

        public UIStyle() {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            dimensions = new Dimensions();
            Initialize();
        }

        public UIStyle(UIStyle toCopy) {
            localId = "AnonymousStyle[" + (NextStyleId++) + "]";
            filePath = string.Empty;
            dimensions = toCopy.dimensions;
            paint = toCopy.paint;
            margin = toCopy.margin;
            border = toCopy.border;
            padding = toCopy.padding;
            borderRadius = toCopy.borderRadius;
            layoutParameters = toCopy.layoutParameters;
            layoutConstraints = toCopy.layoutConstraints;
            textStyle = toCopy.textStyle;
        }

        public string Id => filePath == string.Empty ? localId : localId + "->" + filePath;

        private void Initialize() {
            dimensions = Dimensions.Unset;
            layoutConstraints = LayoutConstraints.Unset;
            margin = ContentBoxRect.Unset;
            padding = ContentBoxRect.Unset;
            border = ContentBoxRect.Unset;
            paint = Paint.Unset;
            borderRadius = BorderRadius.Unset;
            layoutParameters = LayoutParameters.Unset;
            textStyle = TextStyle.Unset;
            transform = new UITransform();
        }
        
        public static readonly UIStyle Default = new UIStyle("Default", string.Empty) {
            transform = new UITransform() {
                position = new MeasurementVector2(new UIMeasurement(0), new UIMeasurement()),
                pivot = new Vector2(),
                scale = new Vector2(1, 1),
                rotation = 0f
            },
            dimensions = new Dimensions() {
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
            textStyle = new TextStyle(
                Color.black,
                null, 
                12, 
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                WhitespaceMode.Wrap,
                HorizontalWrapMode.Overflow,
                VerticalWrapMode.Overflow
            )
        };

    }

}