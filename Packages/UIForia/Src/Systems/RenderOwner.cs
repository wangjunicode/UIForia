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
        private LightStack<RenderBox> clipStack;
        private StructList<RenderBoxWrapper> wrapperList;
        private StructList<DrawCommand> drawList;
        private StructStack<RenderBoxWrapper> wrapperStack;
        private LightList<RenderBox> renderedClippers;

        private SimpleRectPacker maskPackerR = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerG = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerB = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker maskPackerA = new SimpleRectPacker(Screen.width, Screen.height, 0);
        private SimpleRectPacker textPacker = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private SimpleRectPacker texturePacker = new SimpleRectPacker(Screen.width, Screen.height, 4);

        private static readonly DepthComparer s_RenderComparer = new DepthComparer();

        public RenderOwner(UIView view, Camera camera) {
            this.view = view;
            this.defaultCamera = camera;
            this.enabledElementList = new LightList<UIElement>();
            this.stack = new LightStack<RenderBox>();
            this.painterPool = new RenderBoxPool();
            this.clipStack = new LightStack<RenderBox>();
            this.wrapperList = new StructList<RenderBoxWrapper>(32);
            this.wrapperStack = new StructStack<RenderBoxWrapper>(16);
            this.drawList = new StructList<DrawCommand>(0);
            this.renderedClippers = new LightList<RenderBox>();
            this.view.RootElement.renderBox = new RootRenderBox();
            this.view.RootElement.renderBox.element = view.RootElement;
        }

        public void Render(RenderContext renderContext) {
            
            GatherBoxDataParallel(); // todo -- move and push on thread to do parallel w/ layout

            Cull();

            DrawClipShapes(renderContext);

            Draw(renderContext);

            renderedClippers.QuickClear();
            wrapperList.QuickClear();
            drawList.QuickClear();
//            view.rootElement.renderBox.Render(renderContext);
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

        private RenderTexture clipTexture;

        private void DrawClipShapes(RenderContext ctx) {
            if (clipTexture == null) {
                clipTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            }

            maskPackerA.Clear();

            Material sdfClipMaterial = Resources.Load<Material>("Materials/UIForiaSDFMask");

            ctx.SetRenderTexture(clipTexture);

            for (int i = 0; i < renderedClippers.size; i++) {
                RenderBox box = renderedClippers.array[i];

                ClipShape clipShape = box.GetClipShape();

                if (clipShape == null) {
                    box.clipRect = new Vector4(0, 0, 0, 0);
                    box.clipUVs = default;
                    continue;
                }

                // may be faster to alternate packers since it is easier to find free space when more free space is available (profile this)
                // foreach packer -> packer.TryFitRect(i, rect)
                if (maskPackerA.TryPackRect(clipShape.width, clipShape.height, out SimpleRectPacker.PackedRect r)) {
                    Matrix4x4 mat = Matrix4x4.Translate(new Vector3(r.xMin, -r.yMin, 0));

                    ctx.DrawClipShape(clipShape);
                    box.clipTexture = clipTexture;

                    // can compress into 2 floats for xy and width / height. but only have max of 6553.6 when using floats, can be int at 65536 which is fine
                    // just need to watch out for screen size (ultra high pixel count over 4k probably don't need to worry about yet)
                    // todo -- this is screen oriented, will need to handle the case when this is transformed
                    box.clipRect = new Vector4(
                        box.element.layoutResult.screenPosition.x,
                        box.element.layoutResult.screenPosition.y,
                        box.element.layoutResult.actualSize.width,
                        box.element.layoutResult.actualSize.height
                    );
                    // can compress to float (2 bytes each for reasonable precision)
                    box.clipUVs = new Vector4(
                        r.xMin / (float) clipTexture.width,
                        r.yMin / (float) clipTexture.height,
                        r.xMax / (float) clipTexture.width,
                        r.yMax / (float) clipTexture.height
                    );
                }
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
                    current.renderBox.clipped = false;
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

            RenderBoxWrapper[] wrappers = wrapperList.array;

            
            bool activeClipperIsCulled = false;

            for (int i = 0; i < wrapperList.size; i++) {
                RenderBoxWrapper wrapper = wrappers[i];

                RenderBox renderBox = wrapper.renderBox;

                switch (wrapper.renderOp) {
                    case RenderOpType.Unset:
                        break;

                    case RenderOpType.DrawBackground:

                        renderBox.clipped = false;

                        switch (renderBox.clipBehavior) {
                            case ClipBehavior.Never:
                                renderBox.clipper = null;
                                drawList.Add(new DrawCommand(renderBox, DrawCommandType.BackgroundTransparent));
                                break;

                            case ClipBehavior.Screen:
                                renderBox.clipper = null;
                                break;

                            case ClipBehavior.Normal:

                                // if current clipper is clipped, mark as clipped

                                // otherwise intersect with currently visible polygon
                                // any rounded shape masking will happen in the shader I think
                                if (activeClipperIsCulled) {
                                    renderBox.clipper = clipStack.array[clipStack.size - 1];
                                    renderBox.clipper.clippedBoxCount++;
                                    renderBox.clipped = true;
                                    continue;
                                }

                                Rect renderBounds = renderBox.RenderBounds;
                                SVGXMatrix transform = wrapper.element.layoutResult.matrix;

                                Vector2 p0 = transform.Transform(renderBounds.xMin, renderBounds.yMin);
                                Vector2 p1 = transform.Transform(renderBounds.xMax, renderBounds.yMin);
                                Vector2 p2 = transform.Transform(renderBounds.xMax, renderBounds.yMax);
                                Vector2 p3 = transform.Transform(renderBounds.xMin, renderBounds.yMax); 
                                
                                // clipShape.ContainsOrOverlaps(worldSpaceBounds);

                                for (int j = 0; j < clipStack.size; j++) {
                                    // there is probably a faster way to do this linearly, need bounds in screen space to compare against each other
                                    // if parent is clipped & not overflowing parent  & identity transform -> clipped = true
                                    Rect bounds = renderBox.RenderBounds;

                                    // 2 phases for culling 
                                    //     1. general rect bounds intersection, always run
                                    //     2. user defined cull check, run if painter implements ShouldCull

                                    // go through 

                                    if (clipStack.array[j].ShouldCull(bounds)) {
                                        renderBox.clipped = true;
                                        break;
                                    }
                                }

                                if (!renderBox.clipped && clipStack.size > 0) {
                                    renderBox.clipper = clipStack.array[clipStack.size - 1];
                                    renderBox.clipper.clippedBoxCount++;
                                    Assert.IsFalse(renderBox.clipper == renderBox);
                                }

                                break;

                            case ClipBehavior.View:
                                break;
                        }

                        if (!renderBox.clipped) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.BackgroundTransparent));
                        }

                        break;

                    case RenderOpType.DrawForeground:

                        if (!renderBox.clipped) {
                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.ForeGroundTransparent));
                        }

                        break;

                    case RenderOpType.PushClipShape:
                        // push no matter what, if this clipper gets clipped by parent clipper we'll figure it out later
                        // for now assume no rotation or scaling, we can handle transformations later
                        // wrapper.renderBox.intersectClipRect = RectExtensions.Intersect(wrapper.orientedScreenRect);
                        ClipData clipData = new ClipData();
                        clipData.clipParent = clipStack.PeekUnchecked();
                        // clipData.clipShape = renderBox.GetClipShape();
                        
                        Polygon clipPolygon = clipData.clipParent
                        wrapper.renderBox.clipPolygon = clipPolygon.Intersect(wrapper.element.layoutResult.matrix, wrapper.renderBox.RenderBounds);
                        
                        if (wrapper.renderBox.clipPolygon == null) {
                            wrapper.renderBox.clipped = true;
                        }
                        
                        clipStack.Push(wrapper.renderBox);
                        
                        break;

                    case RenderOpType.PopClipShape:

                        RenderBox box = clipStack.Pop();
                        // want to remove no-op clip shapes (ie broad phase already culled the whole membership)
                        renderedClippers.Add(box);
                        if (box.clippedBoxCount > 0) {
                            // todo -- always 0
                        }

                        break;

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