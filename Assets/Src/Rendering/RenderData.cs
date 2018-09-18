using System;
using Src.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class RenderData : IHierarchical {

        public UIElement element;
        public Graphic renderComponent;
        public RectTransform unityTransform;
        public RenderPrimitiveType primitiveType;
        public ScrollBar horizontalScrollbar;
        public ScrollBar verticalScrollbar;

        public RenderData(UIElement element, RenderPrimitiveType primitiveType, RectTransform unityTransform) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.unityTransform = unityTransform;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

    public class ScrollViewData {

        public RectTransform root;
        public RectTransform viewport;
        public RectTransform scrollbarVertical;
        public RectTransform scrollbarHorizontal;

    }

    public struct ReadOnlyVector2 {

        public readonly float x;
        public readonly float y;

        public ReadOnlyVector2(Vector2 vec) {
            this.x = vec.x;
            this.y = vec.y;
        }

        public static implicit operator ReadOnlyVector2(Vector2 vec) {
            return new ReadOnlyVector2(vec);
        }

    }

    [Template(TemplateType.String, @"
        <UITemplate sealed>
            <Contents>
                <Mask x-id='viewport'>
                    <Group x-id='content'>
                        <Children/>
                    </Group>
                </Mask>
                <ScrollBar x-id='horizontalScrollBar'/>
                <ScrollBar x-id='verticalScrollBar'/>
            </Contents/>    
        </UITemplate>
    ")]
    public class ScrollView : UIElement {

        private UIElement content;
        private ScrollBar horizontal;
        private ScrollBar vertical;

        public override void OnCreate() {
            content = FindById("content");
            horizontal = FindById<ScrollBar>("horizontalScrollBar");
            vertical = FindById<ScrollBar>("verticalScrollBar");

            horizontal.onScrollUpdate += (f) => { content.style.positionX = f; };

            vertical.onScrollUpdate += (f) => { content.style.positionY = f; };
        }

        public override void OnUpdate() {
           // float contentWidth = content.lastFrameMeasurements.width;
           // horizontal.UpdateHandleSize(computedDimensions.width, contentWidth);
        }

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Group x-id='track'>
                    <Group x-id='handle' onDragCreate='{HandleDragCreate($event)}'/>
                </Group>
            </Contents/>    
        </UITemplate>
    ")]
    public class ScrollBar : UIElement {

        public event Action<float> onScrollUpdate;

        private DragEvent HandleDragCreate() {
            return null;
        }

        private void HandleDragUpdate(DragEvent evt) {
            onScrollUpdate?.Invoke(0);
        }

    }

}