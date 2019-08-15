using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using Vertigo;
using Debug = System.Diagnostics.Debug;

namespace Src.Systems {

    public class RootRenderBox : RenderBox {

        public override Rect RenderBounds => new Rect(0, 0, element.View.Viewport.width, element.View.Viewport.height);

        public override void PaintBackground(RenderContext ctx) { }

    }

    public class RenderOwner {

        internal UIView view;
        private Camera defaultCamera;
        private readonly LightList<UIElement> enabledElementList;
        private readonly LightStack<RenderBox> stack;
        private readonly RenderBoxPool painterPool;
        private LightStack<ClipData> clipStack;
        private StructList<RenderBoxWrapper> wrapperList;
        private StructList<DrawCommand> drawList;
        private StructStack<RenderBoxWrapper> wrapperStack;
        internal LightList<ClipData> renderedClippers;
        private LightList<ClipData> clipDataPool = new LightList<ClipData>();
        private LightList<ClipData> culledClippers = new LightList<ClipData>();
        
        private SimpleRectPacker maskPackerR = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerG = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerB = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerA = new SimpleRectPacker(Screen.width, Screen.height, 0);
        private SimpleRectPacker textPacker = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker texturePacker = new SimpleRectPacker(Screen.width, Screen.height, 4);

        private RenderTexture clipTexture;
        
        private static readonly DepthComparer s_RenderComparer = new DepthComparer();
        private static readonly StructList<Vector2> s_SubjectRect = new StructList<Vector2>(4);

        public RenderOwner(UIView view, Camera camera) {
            this.view = view;
            this.defaultCamera = camera;
            this.enabledElementList = new LightList<UIElement>();
            this.stack = new LightStack<RenderBox>();
            this.painterPool = new RenderBoxPool();
            this.clipStack = new LightStack<ClipData>();
            this.wrapperList = new StructList<RenderBoxWrapper>(32);
            this.wrapperStack = new StructStack<RenderBoxWrapper>(16);
            this.drawList = new StructList<DrawCommand>(0);
            this.renderedClippers = new LightList<ClipData>();
            this.view.RootElement.renderBox = new RootRenderBox();
            this.view.RootElement.renderBox.element = view.RootElement;
        }

        public void Render(RenderContext renderContext) {
            GatherBoxDataParallel(); // todo -- move and push on thread to do parallel w/ layout

            Cull();

            DrawClipShapes(renderContext);

            Draw(renderContext);

            wrapperList.QuickClear();
            drawList.QuickClear();
        }

        public enum DrawCommandType {

            BackgroundTransparent,
            ForeGroundTransparent,
            BackgroundOpaque,
            ForeGroundOpaque,

        }

        public struct DrawCommand {

            public RenderBox renderBox;
            public DrawCommandType commandType;

            public DrawCommand(RenderBox box, DrawCommandType commandType) {
                this.renderBox = box;
                this.commandType = commandType;
            }

        }


        private void DrawClipShapes(RenderContext ctx) {
            if (clipTexture == null) {
                clipTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            }

            maskPackerA.Clear();

            Material sdfClipMaterial = Resources.Load<Material>("Materials/UIForiaSDFMask");

            ctx.SetRenderTexture(clipTexture);

            for (int i = 0; i < renderedClippers.size; i++) {
//                ClipData box = renderedClippers.array[i];
//
//                ClipShape clipShape = box.GetClipShape();
//
//                if (clipShape == null) {
//                    box.clipRect = new Vector4(0, 0, 0, 0);
//                    box.clipUVs = default;
//                    continue;
//                }
//
//                // may be faster to alternate packers since it is easier to find free space when more free space is available (profile this)
//                // foreach packer -> packer.TryFitRect(i, rect)
//                if (maskPackerA.TryPackRect(clipShape.width, clipShape.height, out SimpleRectPacker.PackedRect r)) {
//                    Matrix4x4 mat = Matrix4x4.Translate(new Vector3(r.xMin, -r.yMin, 0));
//
//                    ctx.DrawClipShape(clipShape);
//                    box.clipTexture = clipTexture;
//
//                    // can compress into 2 floats for xy and width / height. but only have max of 6553.6 when using floats, can be int at 65536 which is fine
//                    // just need to watch out for screen size (ultra high pixel count over 4k probably don't need to worry about yet)
//                    // todo -- this is screen oriented, will need to handle the case when this is transformed
//                    box.clipRect = new Vector4(
//                        box.element.layoutResult.screenPosition.x,
//                        box.element.layoutResult.screenPosition.y,
//                        box.element.layoutResult.actualSize.width,
//                        box.element.layoutResult.actualSize.height
//                    );
//                    // can compress to float (2 bytes each for reasonable precision)
//                    box.clipUVs = new Vector4(
//                        r.xMin / (float) clipTexture.width,
//                        r.yMin / (float) clipTexture.height,
//                        r.xMax / (float) clipTexture.width,
//                        r.yMax / (float) clipTexture.height
//                    );
//                }
            }

            ctx.SetRenderTexture(null);
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
                this.siblingIndex = wrapper.siblingIndex;
                this.traversalIndex = -1;
                this.layer = wrapper.zIndex;
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
                    current.renderBox.clippedBoxCount = 0;
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

            if (!printed) {
                printed = true;
                for (int i = 0; i < wrapperList.size; i++) {
                    UnityEngine.Debug.Log(wrapperList.array[i].element + " -- " + (wrapperList.array[i].renderOp));
                }
            }
        }

        private bool printed = false;

        private void Cull() {
            // first do an easy screen cull
            // screen is always aligned
            // if world space rect is not inside the screen, fail immediately
            renderedClippers.Clear();

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
            screenClip.worldBounds.p0 = new Vector2(1, 2);
            screenClip.worldBounds.p1 = new Vector2(Screen.width, 2);
            screenClip.worldBounds.p2 = new Vector2(Screen.width, Screen.height);
            screenClip.worldBounds.p3 = new Vector2(1, Screen.height);
            screenClip.intersected.array[0] = screenClip.worldBounds.p0;
            screenClip.intersected.array[1] = screenClip.worldBounds.p1;
            screenClip.intersected.array[2] = screenClip.worldBounds.p2;
            screenClip.intersected.array[3] = screenClip.worldBounds.p3;
            screenClip.intersected.size = 4;

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

                        switch (renderBox.clipBehavior) {
                            
                            case ClipBehavior.Never:
                                renderBox.clipper = null;
                                renderBox.culled = false;
                                break;

                            case ClipBehavior.Screen:
                                renderBox.clipper = screenClip;
                                
                                break;

                            case ClipBehavior.View:
//                                renderBox.clipper = viewClip;
                                break;
                            
                            case ClipBehavior.Normal:

                                // if current clipper is clipped, mark as clipped
                                ClipData clipData = clipStack.array[clipStack.size - 1];

                                if (!clipData.isCulled) {
                                    Rect renderBounds = renderBox.RenderBounds;
                                    SVGXMatrix transform = wrapper.element.layoutResult.matrix;

                                    Vector2 p0 = transform.Transform(renderBounds.xMin, renderBounds.yMin);
                                    Vector2 p1 = transform.Transform(renderBounds.xMax, renderBounds.yMin);
                                    Vector2 p2 = transform.Transform(renderBounds.xMax, renderBounds.yMax);
                                    Vector2 p3 = transform.Transform(renderBounds.xMin, renderBounds.yMax);

                                    // if contained by resulting screen aligned rect or overlaps it, we will draw 

                                    Rect screenAligned = clipData.screenSpaceBounds;
                                    if (screenAligned.Contains(p0) || screenAligned.Contains(p1) || screenAligned.Contains(p2) || screenAligned.Contains(p3)) {
                                        
                                    }
                                    else {
                                        
                                    }

                                    // todo -- this assumes both rects are in the same space, they are not always! fix this!
                                    renderBox.culled = !screenAligned.Overlaps(new Rect(p0.x, p0.y, p2.x - p0.x, p2.y - p0.y));
                                    
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

                        if (!renderBox.culled) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.BackgroundTransparent));
                        }

                        break;
                    }
                    case RenderOpType.DrawForeground:

                        if (!renderBox.culled) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.ForeGroundTransparent));
                        }

                        break;

                    case RenderOpType.PushClipShape: {
                        // push no matter what, if this clipper gets clipped by parent clipper we'll figure it out later
                        // for now assume no rotation or scaling, we can handle transformations later
                        // wrapper.renderBox.intersectClipRect = RectExtensions.Intersect(wrapper.orientedScreenRect);

                        Rect localBounds = renderBox.RenderBounds;

                        SVGXMatrix transform = wrapper.element.layoutResult.matrix;

                        Vector2 p0 = transform.Transform(localBounds.xMin, localBounds.yMin);
                        Vector2 p1 = transform.Transform(localBounds.xMax, localBounds.yMin);
                        Vector2 p2 = transform.Transform(localBounds.xMax, localBounds.yMax);
                        Vector2 p3 = transform.Transform(localBounds.xMin, localBounds.yMax);

                        ClipData clipData = GetClipData();
                        clipData.parent = clipStack.PeekUnchecked();
                        clipData.worldBounds = new PolyRect(p0, p1, p2, p3);
                        clipData.renderBox = renderBox;
                        clipData.isCulled = true;
                        clipData.visibleBoxCount = 0;
                        clipData.intersected.size = 0;
                        
                        if (!clipData.parent.isCulled) {
                            s_SubjectRect.array[0] = p0;
                            s_SubjectRect.array[1] = p1;
                            s_SubjectRect.array[2] = p2;
                            s_SubjectRect.array[3] = p3;
                            s_SubjectRect.size = 4;
                            SutherlandHodgman.GetIntersectedPolygon(s_SubjectRect, clipData.parent.intersected, ref clipData.intersected);
                            clipData.isCulled = clipData.intersected.size == 0;
                            if (!clipData.isCulled) {
                                clipData.screenSpaceBounds = GetBounds(clipData.intersected);
                            }
                        }

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

        
        private ClipData GetClipData() {
            if (clipDataPool.size > 0) {
                return clipDataPool.RemoveLast();
            }
            return new ClipData();
        }
        
        private static Rect GetBounds(StructList<Vector2> p) {
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

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private void Draw(RenderContext renderContext) {
            DrawCommand[] commands = drawList.array;
            int commandCount = drawList.size;

            for (int i = 0; i < commandCount; i++) {
                ref DrawCommand cmd = ref commands[i];

                switch (cmd.commandType) {
                    case DrawCommandType.BackgroundTransparent:
                        cmd.renderBox.PaintBackground(renderContext);
                        break;

                    case DrawCommandType.ForeGroundTransparent:
                        cmd.renderBox.PaintForeground(renderContext);
                        break;

                    case DrawCommandType.BackgroundOpaque:
                        throw new NotImplementedException();

                    case DrawCommandType.ForeGroundOpaque:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // exposed for testing
        internal StructList<RenderBoxWrapper> WrapperList => wrapperList;

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