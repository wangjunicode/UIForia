using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering {

    public class ContentBox {

        public RectOffset padding;
        public RectOffset margin;
        public RectOffset border;
        public float contentWidth;
        public float contentHeight;

    }
    
    [Flags]
    public enum UIFlags {
        TransformChanged = 1 << 0,
        InLayoutFlow = 1 << 1,
    }

    public enum UIUnit {
        Pixel,
        Percent,
        Content,
        Parent,
        View
    }

    public struct UnitValue {

        public float value;
        public UIUnit unit;

    }
    
    // transform.isInLayoutFlow = false;
    // transform.layoutParameters.grow = 1;
    // transform.layoutParameters.shrink = 1;
    // transform.layoutType = LayoutType.None;
    
    // if parent is set to fill content
    // and child set to fill parent
    // child = 0 && log error
    
    // style.SizeToContent(percent);
    // style.FillParentWidth();
    // style.FillParentHeight();
    // style.FillParent();
    // style.FitContentWidth();
    // style.FitContentHeight();
    // style.FitContent();
    // style.FillView();
    
    public class UITransform {
        
        internal UIFlags flags;
        public readonly UITransform parent;
        public readonly List<UITransform> children;
        public readonly UIView view;

        // width and height from transform are only in relation to actual pixel size
        // they are readonly unless not in flow
        
        internal UITransform(UITransform parent, UIView view) {
            this.parent = parent;
            this.view = view;
            flags = 0;
            children = new List<UITransform>();
        }

        public void SetPosition(Vector2 position, UIUnit unit) {
            // if isInFlow && !layouttype == layout.none return;
            flags |= UIFlags.TransformChanged;
            view.MarkTransformDirty(this);
        }

        public bool IsInLayoutFlow {
            get { return (flags & UIFlags.InLayoutFlow) != 0; }
            set { flags |= value ? UIFlags.InLayoutFlow : UIFlags.InLayoutFlow; }
        }

        public float GetPixelWidth() {
            // element.style.GetContentBox();
        }
    }
    
}