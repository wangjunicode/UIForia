using System.Collections.Generic;
using System.Diagnostics;
using UIForia;
using UIForia.Animation;
using UIForia.Input;
using UIForia.Rendering;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Demos {

    public class ResourceDragEvent : DragEvent {

        public readonly UIElement resourceItem;

        public ResourceDragEvent(UIElement resourceItem, UIElement origin) : base(origin) {
            this.resourceItem = resourceItem;
           // resourceItem.style.SetTransformBehavior(TransformBehavior.LayoutOffset, StyleState.Normal);
            resourceItem.style.SetRenderLayer(RenderLayer.Modal, StyleState.Normal);
            resourceItem.style.SetZIndex(5000, StyleState.Normal);
        }

        public override void Update() {
           // resourceItem.style.SetTransformPosition(MousePosition - DragStartPosition, StyleState.Normal);
        }

        public override void Drop(bool success) {
            AnimationOptions options = new AnimationOptions();

            options.duration = 0.3f;
            options.timingFunction = EasingFunction.CubicEaseIn;

            resourceItem.style.PlayAnimation(new AnimationGroup(
                new PropertyAnimation(new StyleProperty(StylePropertyId.TransformPositionX, new UIFixedLength(0)), options),
                new PropertyAnimation(new StyleProperty(StylePropertyId.TransformPositionY, new UIFixedLength(0)), options)
            ));
        }

    }

    [Template("Demos/ResourceListDisplay.xml")]
    public class ResourceListDisplay : UIElement {

        public List<ResourceItem> contents;

        public DragEvent CreateDrag(UIElement element) {
            return new ResourceDragEvent(element, this);
        }

        [OnDragEnter(typeof(ResourceDragEvent))]
        public void OnDragEnter(DragEvent evt) {
            if (evt.origin == this) {
                return;
            }
         
            UnityEngine.Debug.Log("enter");
        }

        private float offset = 0;
        private int lastHover;

        [OnDragDrop(typeof(ResourceDragEvent))]
        public void OnDragDrop(DragEvent evt) {
           
        }

        [OnDragMove(typeof(ResourceDragEvent))]
        [OnDragHover(typeof(ResourceDragEvent))]
        public void OnDragMove(DragEvent evt) {
            
            if (evt.origin == this) {
                return;
            }

//            float growTime = 0.2f;
            
//            UIElement repeat = GetChild(0);
//            for (int i = 0; i < repeat.ChildCount; i++) {
//                UIElement child = repeat.GetChild(i);
//                // for each item
//                    // if hovered
//                        // grow to target size
//                        // if top half push subsequent children down
//                        // if bottom half push children i + 1 down
//                if (child.layoutResult.ScreenRect.Contains(evt.MousePosition)) {
//
//                    if (lastHover == i) {
//                        offset = Mathf.Clamp(offset + 1, 0, 32f);
//                    }
//
//                    if (lastHover > i) {
//                        // moving upwards
//                        // reset everything below back towards center
//                        for (int j = 0; j < lastHover; j++) {
//                            repeat.GetChild(j).style.SetTransformBehaviorY(TransformBehavior.LayoutOffset, StyleState.Normal);
//                            repeat.GetChild(j).style.SetTransformPositionY(0, StyleState.Normal);
//                        }
//                        
//                    }
//                    else if (lastHover < i) {
//                        // moving downwards
//                        for (int j = lastHover; j < repeat.ChildCount; j++) {
//                            repeat.GetChild(j).style.SetTransformBehaviorY(TransformBehavior.LayoutOffset, StyleState.Normal);
//                            repeat.GetChild(j).style.SetTransformPositionY(0, StyleState.Normal);
//                        }
//                    }
//                    
//                    lastHover = i;
//                    
//                    float halfSpace = child.layoutResult.ScreenPosition.y + (child.layoutResult.ActualHeight * 0.5f);
//                    if (evt.MousePosition.y < halfSpace) {
//                        for (int j = 0; j < i; j++) {
//                            repeat.GetChild(j).style.SetTransformBehaviorY(TransformBehavior.LayoutOffset, StyleState.Normal);
//                            repeat.GetChild(j).style.SetTransformPositionY(-offset, StyleState.Normal);
//                        }
//                    }
//                    else {
//                        for (int j = 0; j < i; j++) {
//                            repeat.GetChild(j).style.SetTransformBehaviorY(TransformBehavior.LayoutOffset, StyleState.Normal);
//                            repeat.GetChild(j).style.SetTransformPositionY(offset, StyleState.Normal);
//                        }
//                    }
//
//                    break;
//                }
//            }
//            
        }

        [OnDragExit(typeof(ResourceDragEvent))]
        public void OnDragExit(DragEvent evt) {
            if (evt.origin == this) {
                return;
            }
            UnityEngine.Debug.Log("exit");
        }

    }

}