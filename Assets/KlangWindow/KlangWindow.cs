using System;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UI {

    [Template("KlangWindow/KlangWindow.xml")]
    public class KlangWindow : UIElement, IPointerQueryHandler {

        public bool isOpen;

        private UIElement titlebarButtons;
        private UIElement runner;
        private UIElement sidebar;
        private UIElement currentSidebarItem;

        public bool hideSideBar;

        public bool Draggable = true;

        public bool DarkTheme;

        public bool OverflowHidden;

        public bool Resizable = true;

        public string windowTitle = "Window Title";

        public WindowSide allowedDragSides = WindowSide.Top | WindowSide.Right | WindowSide.Bottom | WindowSide.Left;

        public event Action onOpen;
        public event Action onClose;

        private CursorStyle cursorResizeH;
        private CursorStyle cursorResizeV;
        private CursorStyle cursorResizeD;
        private CursorStyle cursorResizeDiagonalTLBR;
        
        public override void OnCreate() {
            if (!hideSideBar) {
                allowedDragSides &= ~(WindowSide.Left);
            }
            runner = FindById("side-bar-runner");
            sidebar = FindById("side-bar");

            cursorResizeH = new CursorStyle(
                "resizeH", 
                application.ResourceManager.GetTexture("Client/UI/Sprites/Cursors/ui_mouse_cursor_resize_horizontal"),
                Vector2.zero);
            cursorResizeV = new CursorStyle(
                "resizeH", 
                application.ResourceManager.GetTexture("Client/UI/Sprites/Cursors/ui_mouse_cursor_resize_vertical"),
                Vector2.zero);
            cursorResizeD = new CursorStyle(
                "resizeH", 
                application.ResourceManager.GetTexture("Client/UI/Sprites/Cursors/ui_mouse_cursor_resize_diagonal"),
                Vector2.zero);
            cursorResizeDiagonalTLBR = new CursorStyle(
                "resizeH", 
                application.ResourceManager.GetTexture("Client/UI/Sprites/Cursors/ui_mouse_cursor_resize_diagonal_inverse"),
                Vector2.zero);
        }

        public override void OnEnable() {
            SidebarReset();
            View.RequestFocus();
        }

        public void Open() {
            onOpen?.Invoke();
        }

        public void Close() {
            onClose?.Invoke();
        }

        public void Maximize() {
            if (GetAttribute("minmax") == "maximize") {
                SetAttribute("minmax", "restore");
            }
            else {
                SetAttribute("minmax", "maximize");
            }
        }
        
        public void Minimize() {
            if (GetAttribute("minmax") == "minimize") {
                SetAttribute("minmax", "restore");
            }
            else {
                SetAttribute("minmax", "minimize");
            }
        }

        [OnMouseMove]
        public void SetResizeCursor(MouseInputEvent evt) {
            if (Resizable == false) {
                return;
            }

            const float growFactor = 3f;

            Rect screenRect = layoutResult.ScreenRect.Grow(growFactor);
            Vector2 mouse = evt.MousePosition;

            WindowSide side = 0;

            if ((allowedDragSides & WindowSide.Top) != 0 && MathUtil.Between(mouse.y, layoutResult.ScreenRect.yMin - (growFactor * 2), screenRect.yMin + (growFactor * 2))) {
                side |= WindowSide.Top;
                style.SetCursor(cursorResizeV, StyleState.Normal);
            }

            if ((allowedDragSides & WindowSide.Bottom) != 0 && MathUtil.Between(mouse.y, screenRect.yMax - (growFactor * 2), screenRect.yMax + (growFactor * 2))) {
                side |= WindowSide.Bottom;
                style.SetCursor(cursorResizeV, StyleState.Normal);
            }

            if ((allowedDragSides & WindowSide.Right) != 0 && MathUtil.Between(mouse.x, screenRect.xMax - (growFactor * 2), screenRect.xMax + (growFactor * 2))) {
                if (side == 0) {
                    style.SetCursor(cursorResizeH, StyleState.Normal);
                }
                else if ((side & WindowSide.Top) != 0) {
                    style.SetCursor(cursorResizeD, StyleState.Normal);
                }
                else {
                    style.SetCursor(cursorResizeDiagonalTLBR, StyleState.Normal);
                }
                side |= WindowSide.Right;
            }

            if ((allowedDragSides & WindowSide.Left) != 0 && MathUtil.Between(mouse.x, layoutResult.ScreenRect.xMin - (growFactor * 2), screenRect.xMin + (growFactor * 2))) {
                if (side == 0) {
                    style.SetCursor(cursorResizeH, StyleState.Normal);
                }
                else if ((side & WindowSide.Bottom) != 0) {
                    style.SetCursor(cursorResizeD, StyleState.Normal);
                }
                else {
                    style.SetCursor(cursorResizeDiagonalTLBR, StyleState.Normal);
                }
                side |= WindowSide.Left;
            }

            if (side == 0) {
                style.SetCursor(null, StyleState.Normal);
            }
        }

        [OnDragCreate]
        public DragEvent ResizeWindow(MouseInputEvent evt) {
            if (Resizable == false) {
                return null;
            }

            const float growFactor = 3f;

            Rect screenRect = layoutResult.ScreenRect.Grow(growFactor);
            Vector2 mouse = evt.LeftMouseDownPosition;

            WindowSide side = 0;

            if ((allowedDragSides & WindowSide.Top) != 0 && MathUtil.Between(mouse.y, screenRect.yMin - (growFactor * 2), screenRect.yMin + growFactor)) {
                side |= WindowSide.Top;
            }

            if ((allowedDragSides & WindowSide.Right) != 0 && MathUtil.Between(mouse.x, screenRect.xMax - (growFactor * 2), screenRect.xMax + growFactor)) {
                side |= WindowSide.Right;
            }

            if ((allowedDragSides & WindowSide.Bottom) != 0 && MathUtil.Between(mouse.y, screenRect.yMax - (growFactor * 2), screenRect.yMax + growFactor)) {
                side |= WindowSide.Bottom;
            }

            if ((allowedDragSides & WindowSide.Left) != 0 && MathUtil.Between(mouse.x, screenRect.xMin - (growFactor * 2), screenRect.xMin + growFactor)) {
                side |= WindowSide.Left;
            }

            if (side != 0) {
                evt.StopPropagation();
                return new WindowResizeEvent(this, side);
            }

            return null;
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is SideBarActionEvent sideBarActionEvent) {
                SelectSidebarItem(evt.origin);
            }
        }

        public void SelectSidebarItem(UIElement element) {
            AnimationOptions options = new AnimationOptions();
            options.duration = new UITimeMeasurement(250);
            options.timingFunction = EasingFunction.CubicEaseIn;

            float targetTransformPosY = GetSideBarRunnerTransformY(element);
            if (currentSidebarItem == null) {
                currentSidebarItem = element;
                runner.style.SetTransformPositionY(targetTransformPosY, StyleState.Normal);
            }

            if (currentSidebarItem == element) {
                return;
            }

            AnimationKeyFrame[] frames = {
                new AnimationKeyFrame(0,
                    StyleProperty.TransformPositionY(GetSideBarRunnerTransformY(currentSidebarItem))
                ),
                new AnimationKeyFrame(1,
                    StyleProperty.TransformPositionY(targetTransformPosY)
                )
            };

            currentSidebarItem = element;

            application.Animate(runner, new AnimationData(options, frames));
        }

        private float GetSideBarRunnerTransformY(UIElement sideBarItem) {
            UIElement parent = sideBarItem.Parent as UIElement;
            return parent.layoutResult.localPosition.y + sideBarItem.layoutResult.localPosition.y;
        }

        public bool ContainsPoint(Vector2 point) {
            return layoutResult.ScreenRect.Grow(3).ContainOrOverlap(point);
        }

        private DragEvent CreateDrag_TitleBar(MouseInputEvent evt) {

            DragEvent resizeWindowEvent = ResizeWindow(evt);
            if (resizeWindowEvent != null) {
                evt.StopPropagation();
                return resizeWindowEvent;
            }
            
            if (Draggable == false) {
                return null;
            }

            titlebarButtons = titlebarButtons ?? FindById("title-button-group");
            if (evt.Origin.IsAncestorOf(titlebarButtons)) {
                return null;
            }

            return new WindowDragEvent(this, evt.MousePosition - layoutResult.screenPosition);
        }

        public class WindowDragEvent : DragEvent {

            private readonly Vector2 offset;

            public WindowDragEvent(UIElement origin, Vector2 offset) : base(origin) {
                this.offset = offset;
            }

            public override void Update() {
                if (MousePosition.x > 0 && MousePosition.x < target.application.Width && MousePosition.y > 0 && MousePosition.y < target.application.Width) {
                    origin.style.SetAlignmentOriginX(MousePosition.x - offset.x, StyleState.Normal);
                    origin.style.SetAlignmentOriginY(MousePosition.y - offset.y, StyleState.Normal);
                }
            }
        }

        public class WindowResizeEvent : DragEvent {

            private readonly WindowSide windowSide;
            private readonly Size originalSize;
            private readonly Vector2 originalScreenPosition;
            private readonly Vector2 originalAlignment;

            public WindowResizeEvent(UIElement origin, WindowSide side) : base(origin) {
                this.windowSide = side;
                this.originalScreenPosition = origin.layoutResult.screenPosition;
                this.originalAlignment = new Vector2(origin.style.AlignmentOriginX.value, origin.style.AlignmentOriginY.value);
                this.originalSize = origin.layoutResult.actualSize;
            }

            public override void Update() {
                if ((windowSide & WindowSide.Top) != 0) {
                    float mouseDeltaY = MousePosition.y - originalScreenPosition.y;
                    float y = originalAlignment.y + mouseDeltaY;
                    float height = originalSize.height - mouseDeltaY;
                    origin.style.SetAlignmentOriginY(y, StyleState.Normal);
                    origin.style.SetPreferredHeight(height, StyleState.Normal);
                }

                if ((windowSide & WindowSide.Bottom) != 0) {
                    float newHeight = MousePosition.y - originalScreenPosition.y;
                    origin.style.SetPreferredHeight(newHeight, StyleState.Normal);
                }

                if ((windowSide & WindowSide.Left) != 0) {
                    float mouseDeltaX = MousePosition.x - originalScreenPosition.x;
                    float x = originalAlignment.x + mouseDeltaX;
                    float width = originalSize.width - mouseDeltaX;
                    origin.style.SetAlignmentOriginX(x, StyleState.Normal);
                    origin.style.SetPreferredWidth(width, StyleState.Normal);
                }

                if ((windowSide & WindowSide.Right) != 0) {
                    float newWidth = MousePosition.x - originalScreenPosition.x;
                    origin.style.SetPreferredWidth(newWidth, StyleState.Normal);
                }
            }
        }

        [Flags]
        public enum WindowSide {

            Top = 1 << 0,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3

        }

        public void SidebarReset() {
            currentSidebarItem = sidebar?.GetChild(0);
            if (currentSidebarItem != null) {
                runner.style.SetTransformPositionY(null, StyleState.Normal);
            }
            else {
                runner.SetEnabled(false);
            }
        }
    }

}
