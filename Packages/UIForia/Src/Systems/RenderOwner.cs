using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace Src.Systems {

    public class RenderOwner {

        internal UIView view;
        internal readonly RenderBoxPool painterPool;
        internal readonly LightStack<ClipData> clipStack;
        internal readonly StructList<RenderBoxWrapper> wrapperList;
        internal readonly StructList<DrawCommand> drawList;
        internal readonly StructStack<RenderBoxWrapper> wrapperStack;
        internal readonly LightList<ClipData> renderedClippers;
        internal readonly LightList<ClipData> clipDataPool;
        internal readonly LightList<ClipData> culledClippers;

        private static readonly DepthComparer s_RenderComparer = new DepthComparer();
        private static readonly StructList<Vector2> s_SubjectRect = new StructList<Vector2>(4);

        public RenderOwner(UIView view, Camera camera) {
            this.view = view;
            this.painterPool = new RenderBoxPool();
            this.clipStack = new LightStack<ClipData>();
            this.wrapperList = new StructList<RenderBoxWrapper>(32);
            this.wrapperStack = new StructStack<RenderBoxWrapper>(16);
            this.drawList = new StructList<DrawCommand>(0); // resized on first use
            this.renderedClippers = new LightList<ClipData>();
            this.view.RootElement.renderBox = new RootRenderBox();
            this.view.RootElement.renderBox.element = view.RootElement;
            this.clipDataPool = new LightList<ClipData>();
            this.culledClippers = new LightList<ClipData>();
        }

        // exposed for testing
        internal StructList<RenderBoxWrapper> WrapperList => wrapperList;

        public void Render(RenderContext renderContext) {
            GatherBoxDataParallel(); // todo -- move and push on thread to do parallel w/ layout

            Cull();

            DrawClipShapes(renderContext);
        
            Draw(renderContext);

            wrapperList.QuickClear();
            drawList.QuickClear();
        }


        private void DrawClipShapes(RenderContext ctx) {
            for (int i = 0; i < renderedClippers.size; i++) {
                ClipData clipData = renderedClippers.array[i];
                clipData.clipPath = clipData.renderBox?.GetClipShape();
                ctx.DrawClipData(clipData);
            }
        }

        private void UpdateRenderBox(UIElement element) {
            // get painter
            // see if it the same as current render box
            // if not destroy, create
            // finally, update styles
            element.renderBox.OnDestroy();
            // todo -- pool
            CreateRenderBox(element);
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


        // this is intended to be run while layout is running (ie in parallel)
        public void GatherBoxDataParallel() {
            UIElement root = view.rootElement;

            wrapperList.QuickClear();

            int idx = 0;

            wrapperStack.Push(new RenderBoxWrapper(root));

            while (wrapperStack.size > 0) {
                RenderBoxWrapper current = wrapperStack.array[--wrapperStack.size];

                current.traversalIndex = idx++;
                wrapperList.Add(current);

                if (current.renderOp == RenderOpType.DrawBackground) {
                    current.renderBox = current.element.renderBox;
                    // ReSharper disable once PossibleNullReferenceException
                    current.renderBox.culled = false;
                    LightList<UIElement> children = current.element.children;
 
                    //  if((current.element.renderBox.typeFlags & RenderBoxFlag.PreRenderIcon) != 0) {
                    // will need to check if culled or not

                    // }

                    //if((current.element.renderBox.typeFlags & RenderBoxFlag.PreRenderText) != 0) {
                    // will need to check if culled or not
                    //}


                    if (current.renderBox.overflowX != Overflow.Visible || current.renderBox.overflowY != Overflow.Visible) {
                        wrapperStack.Push(new RenderBoxWrapper(current, RenderOpType.PopClipShape));
                    }
                    
                    // if is post effect
                    // push render target

                    if (current.element.renderBox.hasForeground) {
                        wrapperStack.Push(new RenderBoxWrapper(current, RenderOpType.DrawForeground));
                    }

                    if (children != null) {
                        int childCount = children.size;
                        wrapperList.EnsureAdditionalCapacity(childCount);

                        for (int i = childCount - 1; i >= 0; i--) {
                            UIElement child = children.array[i];
                            if (!child.isEnabled) {
                                continue;
                            }

                            if (child.renderBox == null) {
                                CreateRenderBox(child);
                            }

                            if ((child.flags & UIElementFlags.EnabledThisFrame) != 0) {
                                UpdateRenderBox(child);
                            }

                            wrapperStack.Push(new RenderBoxWrapper(child));
                        }
                    }

                    if (current.renderBox.overflowX != Overflow.Visible || current.renderBox.overflowY != Overflow.Visible) {
                        // clips get pushed even if culled? need to handle overflowing if parent if offscreen but child is not
                        wrapperStack.Push(new RenderBoxWrapper(current, RenderOpType.PushClipShape));
                    }

                    // if is post effect
                    // pop render target
                }
            }

            wrapperList.Sort(s_RenderComparer);

//            if (!printed) {
//                printed = true;
//                for (int i = 0; i < wrapperList.size; i++) {
//                    Debug.Log(wrapperList.array[i].element + " -- " + (wrapperList.array[i].renderOp));
//                }
//            }
        }

        private bool printed = false; // todo remove

        private void Cull() {
            // first do an easy screen cull
            // screen is always aligned
            // if world space rect is not inside the screen, fail immediately

            for (int i = 0; i < renderedClippers.size; i++) {
                renderedClippers.array[i].Clear();
            }

            for (int i = 0; i < culledClippers.size; i++) {
                culledClippers.array[i].Clear();
            }

            clipDataPool.AddRange(renderedClippers);
            clipDataPool.AddRange(culledClippers);

            renderedClippers.size = 0;
            culledClippers.size = 0;

            RenderBoxWrapper[] wrappers = wrapperList.array;

            ClipData screenClip = GetClipData();
            screenClip.worldBounds.p0 = new Vector2(0, 0);
            screenClip.worldBounds.p1 = new Vector2(Screen.width, 0);
            screenClip.worldBounds.p2 = new Vector2(Screen.width, Screen.height);
            screenClip.worldBounds.p3 = new Vector2(0, Screen.height);
            screenClip.intersected.array[0] = screenClip.worldBounds.p0;
            screenClip.intersected.array[1] = screenClip.worldBounds.p1;
            screenClip.intersected.array[2] = screenClip.worldBounds.p2;
            screenClip.intersected.array[3] = screenClip.worldBounds.p3;
            screenClip.intersected.size = 4;
            screenClip.aabb = new Vector4(screenClip.worldBounds.p0.x, screenClip.worldBounds.p0.y, screenClip.worldBounds.p2.x, screenClip.worldBounds.p2.y);
            screenClip.isCulled = false;
            renderedClippers.Add(screenClip);

            clipStack.Push(screenClip);
            
            for (int i = 0; i < wrapperList.size; i++) {
                RenderBoxWrapper wrapper = wrappers[i];

                RenderBox renderBox = wrapper.renderBox;

                switch (wrapper.renderOp) {
                    case RenderOpType.Unset:
                        break;

                    case RenderOpType.DrawBackground: {
                        renderBox.culled = false;

                        switch (renderBox.element.style.ClipBehavior) {
                            case ClipBehavior.Never:
                                renderBox.clipper = null;
                                break;

                            case ClipBehavior.Screen:
                                renderBox.clipper = screenClip;
                                break;

                            case ClipBehavior.View:
                                 renderBox.clipper = renderBox.element.View.rootElement.renderBox.clipper;

                                break;

                            case ClipBehavior.Normal:

                                ClipData clipData = clipStack.array[clipStack.size - 1];

                                // if current clipper is culled, mark as culled
                                if (!clipData.isCulled) {
                                    Rect renderBounds = renderBox.RenderBounds;

                                    SVGXMatrix transform = wrapper.element.layoutResult.matrix;

                                    // todo -- only transform if not identity

                                    // todo -- considering inlining Transform
                                    Vector2 p0 = transform.Transform(renderBounds.xMin, renderBounds.yMin);
                                    Vector2 p1 = transform.Transform(renderBounds.xMax, renderBounds.yMin);
                                    Vector2 p2 = transform.Transform(renderBounds.xMax, renderBounds.yMax);
                                    Vector2 p3 = transform.Transform(renderBounds.xMin, renderBounds.yMax);

                                    // if contained by resulting screen aligned rect or overlaps it, we will draw 
                                    Vector4 objectScreenBounds;
                                    if (transform.IsTranslationOnly) {
                                        objectScreenBounds = new Vector4(p0.x, p0.y, p2.x, p2.y);
                                    }
                                    else {
                                        objectScreenBounds = GetBounds(p0, p1, p2, p3);
                                    }

                                    // cheap solution is to compare world space bounds for overlap better would be compare oriented bounds
                                    renderBox.culled = !clipData.aabb.OverlapAsRect(objectScreenBounds);

                                    if (!renderBox.culled) {
                                        renderBox.clipper = clipStack.array[clipStack.size - 1];
                                        renderBox.clipper.visibleBoxCount++;
                                    }
                                }
                                else {
                                    renderBox.culled = true;
                                }

                                break;
                        }

                        if (!renderBox.culled && renderBox.visibility != Visibility.Hidden) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.BackgroundTransparent));
                        }

                        break;
                    }

                    case RenderOpType.DrawForeground:

                        if (!renderBox.culled) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.ForegroundTransparent));
                        }

                        break;

                    case RenderOpType.PushClipShape: {
                        Rect localBounds = renderBox.RenderBounds;

                        SVGXMatrix transform = wrapper.element.layoutResult.matrix;

                        Vector2 p0 = transform.Transform(localBounds.xMin, localBounds.yMin);
                        Vector2 p1 = transform.Transform(localBounds.xMax, localBounds.yMin);
                        Vector2 p2 = transform.Transform(localBounds.xMax, localBounds.yMax);
                        Vector2 p3 = transform.Transform(localBounds.xMin, localBounds.yMax);

                        ClipData clipData = GetClipData();
                        clipData.parent = clipStack.array[clipStack.size - 1];
                        clipData.worldBounds = new PolyRect(p0, p1, p2, p3);
                        clipData.renderBox = renderBox;
                        clipData.isCulled = true;
                        clipData.visibleBoxCount = 0;
                        clipData.intersected.size = 0;
                        clipData.isTransformed = !transform.IsTranslationOnly;

                        if (!clipData.parent.isCulled) {
                            if (!clipData.isTransformed && !clipData.parent.isTransformed) {
                                clipData.aabb = clipData.parent.aabb.IntersectAsRect(new Vector4(p0.x, p0.y, p2.x, p2.y));
                                clipData.isCulled = clipData.aabb.z == 0 || clipData.aabb.w == 0;
                                clipData.intersected.array[0] = new Vector2(clipData.aabb.x, clipData.aabb.y);
                                clipData.intersected.array[1] = new Vector2(clipData.aabb.z, clipData.aabb.y);
                                clipData.intersected.array[2] = new Vector2(clipData.aabb.z, clipData.aabb.w);
                                clipData.intersected.array[3] = new Vector2(clipData.aabb.x, clipData.aabb.w);
                                clipData.intersected.size = 4;
                            }
                            else {
                                s_SubjectRect.array[0] = p0;
                                s_SubjectRect.array[1] = p1;
                                s_SubjectRect.array[2] = p2;
                                s_SubjectRect.array[3] = p3;
                                s_SubjectRect.size = 4;
                                SutherlandHodgman.GetIntersectedPolygon(s_SubjectRect, clipData.parent.intersected, ref clipData.intersected);
                                clipData.isCulled = clipData.intersected.size == 0;
                                if (!clipData.isCulled) {
                                    clipData.aabb = GetBounds(clipData.intersected);
                                }
                            }
                        }

                        // push no matter what, if this clipper gets clipped by parent clipper we'll figure it out later
                        clipStack.Push(clipData);

                        break;
                    }

                    case RenderOpType.PopClipShape: {
                        ClipData clip = clipStack.Pop();
                        // want to remove no-op clip shapes (ie broad phase already culled the whole membership)
                        if (clip.isCulled || clip.visibleBoxCount == 0) {
                            culledClippers.Add(clip);
                        }
                        else {
                            renderedClippers.Add(clip);
                        }

                        break;
                    }

                    // might be a pre-pass to find these
                    // then do cull check afterwards
                    case RenderOpType.PushPostEffect:
                        // drawListStack.Push();
                        // drawListStack.Peek().needsScreenGrab = needsScreenGrab;
                        // drawListStack.Peek().needsScissorRect = needsScissorRect;
                        break;

                    case RenderOpType.PopPostEffect:
                        // drawListStack.Pop();

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // this returns true if the rect is rotated also! careful!
        private static bool IsRect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            int distP0P1 = (int) (p0 - p1).sqrMagnitude;
            int distP1P2 = (int) (p1 - p2).sqrMagnitude;
            int distP2P3 = (int) (p2 - p3).sqrMagnitude;
            int distP3P0 = (int) (p3 - p0).sqrMagnitude;
            int distP2P0 = (int) (p2 - p0).sqrMagnitude;
            int distP1P3 = (int) (p1 - p3).sqrMagnitude;

            return (distP0P1 ^ distP1P2 ^ distP2P3 ^ distP3P0 ^ distP2P0 ^ distP1P3) == 0;
        }

        private ClipData GetClipData() {
            if (clipDataPool.size > 0) {
                return clipDataPool.RemoveLast();
            }

            return new ClipData();
        }

        private static Vector4 GetBounds(in Vector2 p0, in Vector2 p1, in Vector2 p2, in Vector2 p3) {
            float minX = p0.x;
            float minY = p0.y;
            float maxX = p0.x;
            float maxY = p0.y;

            if (p1.x < minX) minX = p1.x;
            if (p1.x > maxX) maxX = p1.x;
            if (p1.y < minY) minY = p1.y;
            if (p1.y > maxY) maxY = p1.y;

            if (p2.x < minX) minX = p2.x;
            if (p2.x > maxX) maxX = p2.x;
            if (p2.y < minY) minY = p2.y;
            if (p2.y > maxY) maxY = p2.y;

            if (p3.x < minX) minX = p3.x;
            if (p3.x > maxX) maxX = p3.x;
            if (p3.y < minY) minY = p3.y;
            if (p3.y > maxY) maxY = p3.y;

            return new Vector4(minX, minY, maxX, maxY);
        }

        private static Vector4 GetBounds(StructList<Vector2> p) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int b = 0; b < p.size; b++) {
                Vector2 point = p.array[b];
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }

            return new Vector4(minX, minY, maxX, maxY);
        }

        private void Draw(RenderContext renderContext) {
            DrawCommand[] commands = drawList.array;
            int commandCount = drawList.size;

            // bad api usage, fix this while supporting on the fly clipper creation
            renderContext.clipContext.ConstructClipData();

            for (int i = 0; i < commandCount; i++) {
                ref DrawCommand cmd = ref commands[i];

                switch (cmd.commandType) {
                    case DrawCommandType.BackgroundTransparent:
                        cmd.renderBox.PaintBackground(renderContext);
                        break;

                    case DrawCommandType.ForegroundTransparent:
                        cmd.renderBox.PaintForeground(renderContext);
                        break;

                    case DrawCommandType.BackgroundOpaque:
                        throw new NotImplementedException();

                    case DrawCommandType.ForegroundOpaque:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public enum DrawCommandType {

            BackgroundTransparent,
            ForegroundTransparent,
            BackgroundOpaque,
            ForegroundOpaque,

        }

        public struct DrawCommand {

            public RenderBox renderBox;
            public DrawCommandType commandType;

            public DrawCommand(RenderBox box, DrawCommandType commandType) {
                this.renderBox = box;
                this.commandType = commandType;
            }

        }

        internal struct RenderBoxWrapper {

            public RenderOpType renderOp;
            public UIElement element;
            public RenderBox renderBox;
            public int layer;
            public int zIndex;
            public int siblingIndex;
            public int traversalIndex;

            public RenderBoxWrapper(UIElement element) {
                this.renderOp = RenderOpType.DrawBackground;
                this.element = element;
                this.renderBox = element.renderBox;
                this.zIndex = renderBox.zIndex;
                this.siblingIndex = element.siblingIndex;
                this.traversalIndex = -1;
                this.layer = renderBox.layer;
            }

            public RenderBoxWrapper(RenderBoxWrapper wrapper, RenderOpType renderOperation) {
                this.renderOp = renderOperation;
                this.element = wrapper.element;
                this.renderBox = element.renderBox;
                this.zIndex = wrapper.zIndex;
                this.layer = wrapper.layer;
                this.siblingIndex = wrapper.siblingIndex;
                this.traversalIndex = -1;
            }

        }

        public enum RenderOpType {

            Unset = 0,
            DrawBackground = 1,
            DrawForeground = 2,
            PushClipShape = 3,
            PopClipShape = 4,
            PushPostEffect = 5,
            PopPostEffect = 6,

        }

        private class DepthComparer : IComparer<RenderBoxWrapper> {

            // todo -- find a way to not incur copy cost with structs here
            public int Compare(RenderBoxWrapper a, RenderBoxWrapper b) {
                if (a.layer != b.layer) {
                    return a.layer - b.layer;
                }

                // view might be a layer
//                if (a.viewDepthIdx != b.viewDepthIdx) {
//                    return a.viewDepthIdx > b.viewDepthIdx ? -1 : 1;
//                }

                if (a.zIndex != b.zIndex) {
                    return a.zIndex - b.zIndex;
                }

                return a.traversalIndex - b.traversalIndex;
            }

        }

    }

}