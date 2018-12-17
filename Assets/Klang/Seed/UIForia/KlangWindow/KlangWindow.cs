using System;
using UIForia;
using UIForia.Input;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    public class WindowDragEvent : DragEvent {

        private readonly Vector2 offset;

        public WindowDragEvent(UIElement origin, Vector2 offset) : base(origin) {
            this.offset = offset;
        }

        public override void Update() {
            origin.style.SetTransformPosition(MousePosition - offset, StyleState.Normal);
        }

    }

    public enum WindowSide {

        Top,
        Bottom,
        Left,
        Right

    }

    public class WindowResizeEvent : DragEvent {

        private Vector2 offset;
        private WindowSide windowSide;
        private Vector2 originPosition;
        private float start;
        private float maxX;
        private float minX;

        public WindowResizeEvent(UIElement origin, WindowSide side, Vector2 offset) : base(origin) {
            this.windowSide = side;
            start = origin.layoutResult.ScreenRect.width;
            this.offset = offset;
            Rect screenRect = origin.layoutResult.ScreenRect;
            float maxWidth = 800f;
            this.originPosition = screenRect.position;
            float diff = maxWidth - screenRect.width;
            minX = originPosition.x - diff;
            Debug.Log(minX);
            // todo -- calc min rect it could be, drag to constraints
        }

        public override void Update() {
            switch (windowSide) {
                case WindowSide.Top:
                    break;
                case WindowSide.Bottom:
                    break;

                case WindowSide.Left: {
                    float x = MousePosition.x - offset.x;
                    float totalX = originPosition.x - x;
                    if (totalX < minX) {
                        // clamp
                    }
                    else {
                        origin.style.SetTransformPositionX(x, StyleState.Normal);
                        origin.style.SetPreferredWidth(start + totalX, StyleState.Normal);
                    }    
                    break;
                }
                case WindowSide.Right: {
                    float value = start + (MousePosition.x - DragStartPosition.x);
                    if (value < 300) value = 300;
                    if (value > 800) value = 800;
                    origin.style.SetPreferredWidth(value, StyleState.Normal);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

    [Template("Klang/Seed/UIForia/KlangWindow/KlangWindow.xml")]
    public class KlangWindow : UIElement {

        public DragEvent PositionWindow(MouseInputEvent evt) {
            return new WindowDragEvent(this, evt.MousePosition - layoutResult.localPosition);
        }

        public DragEvent ResizeWindow(MouseInputEvent evt) {
            UIElement content = FindById("content");
            Vector2 mouse = evt.MouseDownPosition;
            Rect screenRect = content.layoutResult.ScreenRect;
            Rect left = new Rect(screenRect) {
                width = 3
            };

            if (left.Contains(mouse)) {
                return new WindowResizeEvent(this, WindowSide.Left, evt.MousePosition - layoutResult.localPosition);
            }

            Rect right = new Rect(screenRect) {
                x = screenRect.xMax - 5,
                width = 5
            };

            if (right.Contains(mouse)) {
                return new WindowResizeEvent(this, WindowSide.Right, evt.MousePosition - layoutResult.localPosition);
            }

            return null;
        }

    }

}