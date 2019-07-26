using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Vertigo;
using Debug = System.Diagnostics.Debug;

namespace Src.Systems {

    public class RootRenderBox : RenderBox {

        // 0 , 0?
        public override Rect RenderBounds => element.View.Viewport;

        public override void PaintBackground(RenderContext ctx) { }

    }

    public class RenderOwner {

        internal UIView view;
        private Camera defaultCamera;
        private readonly LightList<UIElement> enabledElementList;
        private readonly LightStack<RenderBox> stack;
        private readonly RenderBoxPool painterPool;
        private StructStack<Rect> clipStack;

        public RenderOwner(UIView view, Camera camera) {
            this.view = view;
            this.defaultCamera = camera;
            this.enabledElementList = new LightList<UIElement>();
            this.stack = new LightStack<RenderBox>();
            this.painterPool = new RenderBoxPool();
            this.clipStack = new StructStack<Rect>();
            this.view.RootElement.renderBox = new RootRenderBox();
            this.view.RootElement.renderBox.element = view.RootElement;
        }

        public void Render(RenderContext renderContext) {
           // var geometry = new UIForiaGeometry();
          //  geometry.FillRectUniformBorder_Miter(-100, -100);
         //      renderContext.DrawBatchedGeometry(geometry);
            view.rootElement.renderBox.Render(renderContext);
        }

        private void CreateOrUpdateRenderBox(UIElement element) {
            string painterId = element.style.Painter;

            if (element.renderBox == null) {
                CreateRenderBox(element);
            }
            else if (element.renderBox.uniqueId != painterId) {
                element.renderBox.OnDestroy();
                // todo -- pool
                CreateRenderBox(element);
            }
        }

        private void CreateRenderBox(UIElement element) {
            string painterId = element.style.Painter;
            RenderBox painter = null;

            if (painterId == null) {
                if (element is UITextElement) {
                    painter = new TextRenderBox();
                }
                else {
                    painter = new StandardRenderBox();
                }
            }
            else {
                painter = painterPool.GetCustomPainter(painterId);
                if (painter == null) {
                    if (element is UITextElement) {
                        painter = new TextRenderBox();
                    }
                    else {
                        painter = new StandardRenderBox();
                    }
                }
            }

            painter.element = element;
            painter.visibility = element.style.Visibility;
            painter.uniqueId = painterId;
            painter.OnInitialize();
            element.renderBox = painter;
        }

        public void GatherBoxData() {
            UIElement root = view.rootElement;

            stack.Push(root.renderBox);

            while (stack.size > 0) {
                RenderBox parent = stack.array[--stack.size];

                LightList<UIElement> children = parent.element.children;

                if (children.size == 0) {
                    parent.firstChild = null;
                    return;
                }

                RenderBox lastChild = null;

                // fuck it, handle z-index sort later

                for (int i = 0; i < children.size; i++) {
                    UIElement child = children.array[i];

                    if (!child.isEnabled) {
                        continue;
                    }

                    if (child.renderBox == null || (child.flags & UIElementFlags.EnabledThisFrame) != 0) {
                        CreateOrUpdateRenderBox(child);
                    }

                    Debug.Assert(child.renderBox != null, "child.renderBox != null");

                    if (child.renderBox.visibility == Visibility.Hidden) {
                        continue;
                    }

                    if (parent.firstChild == null) {
                        parent.firstChild = child.renderBox;
                    }

                    if (lastChild != null) {
                        lastChild.nextSibling = child.renderBox;
                    }

                    child.renderBox.nextSibling = null;
                    lastChild = child.renderBox;

                    stack.Push(child.renderBox);
                }
            }
        }


        // really do want a flat list
        // if a child fails the broadphase cull check, do not add it but do add its children

        // todo -- jobify this
        public void BuildClipGroups() {
            clipStack.Push(new Rect(0, 0, Screen.width, Screen.height));
            BuildClipGroups(view.RootElement.renderBox);
        }

        private void BuildClipGroups(RenderBox parent) {
            if (parent.firstChild == null) {
                return;
            }

            Rect clipRect = clipStack.PeekUnchecked();

            if (parent.overflowX != Overflow.Visible || parent.overflowY != Overflow.Visible) {
                // todo -- this needs to handle transformation
                clipRect = clipRect.Intersect(parent.element.layoutResult.ScreenRect);
                clipStack.Push(clipRect);
            }

            RenderBox ptr = parent.firstChild;
            while (ptr != null) {
                if (ptr.clipBehavior != ClipBehavior.Never) {
                    ptr.clipped = false; // clipStack.PeekUnchecked().Overlaps(ptr.RenderBounds); //  || clipStack.PeekUnchecked().Contains(ptr.RenderBounds);
                }
                else {
                    ptr.clipped = false;
                }

                ptr.clipRect = clipRect;
                BuildClipGroups(ptr);
                ptr = parent.nextSibling;
            }

            if (parent.overflowX != Overflow.Visible || parent.overflowY != Overflow.Visible) {
                clipStack.Pop();
            }
        }

        public void OnElementEnabled(UIElement element) {
            if (enabledElementList.Contains(element)) {
                enabledElementList.Add(element);
            }
        }

        public void OnElementDisabled(UIElement element) {
            enabledElementList.Remove(element);
        }

        public void OnElementDestroyed(UIElement element) {
            enabledElementList.Remove(element);
        }

    }

}