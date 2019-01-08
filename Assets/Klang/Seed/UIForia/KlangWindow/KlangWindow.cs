using System.Collections.Generic;
using UIForia;
using UIForia.Extensions;
using UIForia.Input;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/KlangWindow/KlangWindow.xml")]
    public class KlangWindow : UIElement {

        private static readonly List<KlangWindow> windowStack = new List<KlangWindow>();

        private bool isPinned;
        
        public override void OnDestroy() {
            windowStack.Remove(this);
        }

        public override void OnReady() {
            windowStack.Add(this);
            style.SetRenderLayer(RenderLayer.View, StyleState.Normal);
        }

        public void Pin() {
            isPinned = !isPinned;
        }

        public void Minimize() { }

        public void Maximize() { }

        public void Close() {
            
        }

        [OnMouseDown]
        public void OnMouseDown(MouseInputEvent evt) {
            windowStack.Remove(this);
            windowStack.Insert(0, this);
            for (int i = windowStack.Count - 1; i >= 0; i--) {
                windowStack[i].style.SetZIndex(windowStack.Count - i, StyleState.Normal);
            }
            evt.StopPropagation();
        }

        [OnDragCreate]
        public DragEvent PositionResizeWindow(MouseInputEvent evt) {
            
            if (isPinned) return null;
            
            Rect screenRect = layoutResult.ScreenRect;
            Vector2 mouse = evt.MouseDownPosition;

            Rect top = new Rect(screenRect) {
                height = 5f
            };

            Rect right = new Rect(screenRect) {
                x = screenRect.xMax - 5f,
                width = 5f
            };

            Rect left = new Rect(screenRect) {
                width = 5f
            };

            Rect bottom = new Rect(screenRect) {
                y = screenRect.yMax - 5f,
                height = 5f
            };

            WindowSide side = 0;

            if (top.ContainOrOverlap(mouse)) side |= WindowSide.Top;
            if (right.ContainOrOverlap(mouse)) side |= WindowSide.Right;
            if (bottom.ContainOrOverlap(mouse)) side |= WindowSide.Bottom;
            if (left.ContainOrOverlap(mouse)) side |= WindowSide.Left;

            if (side != 0) {
                return new WindowResizeEvent(this, side, evt.MousePosition - layoutResult.screenPosition);
            }

            Rect header = FindById("header").layoutResult.ScreenRect;

            if (header.Contains(mouse)) {
                return new WindowDragEvent(this, evt.MousePosition - layoutResult.localPosition);
            }

            return null;
        }

    }

}