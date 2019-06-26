//using System;
//using System.Threading;
//using SVGX;
//using UIForia.Elements;
//using UIForia.Layout;
//using UIForia.Rendering;
//using UIForia.Systems;
//using UIForia.Util;
//using UnityEngine;
//using UnityEngine.Rendering;
//using Vertigo;
//
//namespace UIForia.Systems {
//
//    public interface IElementPainter {
//
//        void OnElementStyleChanged(UIElement element, StructList<StyleProperty> changeSet);
//
//        void GetPaintMetaData(UIElement element, out PaintMetaData metaData);
//
//        bool ShouldRepaint(UIElement element, bool hasVisibleAncestors);
//
//        void Paint(GraphicsContext2 ctx, UIElement element);
//
//    }
//
//    [CustomPainter("DefaultBox")]
//    public class DefaultBoxPainter : IElementPainter {
//
//        private readonly IntMap<RenderInfo> renderMap = new IntMap<RenderInfo>();
//        private readonly IStyleSystem styleSystem;
//
//        public void OnElementStyleChanged(UIElement element, StructList<StyleProperty> changeSet) {
//            if (!renderMap.TryGetValue(element.id, out RenderInfo renderInfo)) {
//                return;
//            }
//
//            int count = changeSet.size;
//            StyleProperty[] properties = changeSet.array;
//            for (int i = 0; i < count; i++) {
//                ref StyleProperty property = ref properties[i];
//
//                switch (property.propertyId) {
//                    case StylePropertyId.BorderColorTop:
//                    case StylePropertyId.BorderColorRight:
//                    case StylePropertyId.BorderColorBottom:
//                    case StylePropertyId.BorderColorLeft:
//                        break;
//
//                    case StylePropertyId.BorderTop:
//                    case StylePropertyId.BorderRight:
//                    case StylePropertyId.BorderBottom:
//                    case StylePropertyId.BorderLeft:
//                        break;
//                }
//
//                renderInfo.requiresRepaint = true;
//                // todo -- make an IntStructMap<T> where T : Struct
//                renderMap[element.id] = renderInfo;
//            }
//        }
//
//        private void UpdateElementStyles(UIElement element) { }
//
//        public void OnActivatePainter(UIElement element) {
//            // if painter or self painter, skip most of this function
//
//            RenderInfo renderInfo = new RenderInfo();
//            // todo pre-compute border data, linearize colors, etc
//            UIStyleSet style = element.style;
//            renderInfo.backgroundColor = style.BackgroundColor;
//            renderInfo.backgroundImage = style.BackgroundImage;
//            renderInfo.backgroundRotation = style.BackgroundImageRotation.value; // todo -- resolve this to a float
//            renderInfo.opacity = style.Opacity;
//            // todo resolve to float
//            renderInfo.backgroundScale = new Vector2(style.BackgroundImageScaleX.value, style.BackgroundImageScaleY.value);
//            renderInfo.visibility = style.Visibility;
//            renderInfo.uvRect = new Rect(0, 0, 1, 1);
//            renderInfo.uvOffset = new Vector2(0, 0);
//            renderInfo.uvTiling = new Vector2(1, 1);
//            renderInfo.backgroundTint = style.BackgroundTint;
//            renderInfo.borderRadius = element.layoutResult.borderRadius;
//
//            renderInfo.clipRect = Vector4.zero; // todo -- wrong
//
//            renderInfo.borderSize = element.layoutResult.border;
//            renderInfo.borderColorTop = style.BorderColorTop;
//            renderInfo.borderColorRight = style.BorderColorRight;
//            renderInfo.borderColorBottom = style.BorderColorBottom;
//            renderInfo.borderColorLeft = style.BorderColorLeft;
//            renderInfo.scale = element.layoutResult.scale;
//            renderInfo.position = element.layoutResult.screenPosition;
//            renderInfo.rotation = 0; // todo -- fix this
//            // renderInfo.renderMethod = ComputeRenderType(element, renderInfo);
//
//            float borderColorTop = VertigoUtil.ColorToFloat(style.BorderColorTop);
//            float borderColorRight = VertigoUtil.ColorToFloat(style.BorderColorRight);
//            float borderColorBottom = VertigoUtil.ColorToFloat(style.BorderColorBottom);
//            float borderColorLeft = VertigoUtil.ColorToFloat(style.BorderColorLeft);
//
//            renderInfo.packedBorderColors = new Vector4(borderColorTop, borderColorRight, borderColorBottom, borderColorLeft);
//
//            Color32 backgroundColor = style.BackgroundColor;
//            Color32 backgroundTint = style.BackgroundTint;
//            Texture backgroundImage = style.BackgroundImage;
//
//            float packedBackgroundColor = VertigoUtil.ColorToFloat(style.BackgroundColor);
//            float packedBackgroundTint = VertigoUtil.ColorToFloat(style.BackgroundTint);
//            PaintMode colorMode = PaintMode.None;
//
//            if (!ReferenceEquals(backgroundImage, null)) {
//                colorMode |= PaintMode.Texture;
//            }
//
//            if (backgroundTint.a > 0) {
//                colorMode |= PaintMode.TextureTint;
//            }
//
//            if (backgroundColor.a > 0) {
//                colorMode |= PaintMode.Color;
//            }
//
//            renderInfo.packedColorData = new Vector4(packedBackgroundColor, packedBackgroundTint, (int) colorMode, 0);
//            renderMap[element.id] = renderInfo;
//        }
//
//        public void OnDeactivate(UIElement element) {
//            renderMap.Remove(element.id);
//        }
//
//        public void GetPaintMetaData(UIElement element, out PaintMetaData metaData) {
//            metaData = new PaintMetaData() { };
//        }
//
//        public bool ShouldRepaint(UIElement element, bool hasVisibleAncestors) {
//            // RenderInfo info = (RenderInfo) element.renderData;
//            return renderMap[element.id].requiresRepaint;
//        }
//
//        public void Paint(GraphicsContext2 ctx, UIElement element) {
//            RenderInfo info = renderMap[element.id];
//            UIForiaDrawer drawer = ctx.ActivateDrawer<UIForiaDrawer>();
//            
//            drawer.MixedBorderRect(100, 100, 100, 100, info);
//            
////            ctx.PaintChildren(element);
//        }
//
//    }
//
//    public class VertigoRenderSystem : IRenderSystem {
//
//        private Camera camera;
//        private ILayoutSystem layoutSystem;
//        private CommandBuffer commandBuffer;
//        private LightList<UIView> views;
//        private IStyleSystem styleSystem;
//        private IntMap<RenderInfo> renderInfos;
//        private LightList<UIElement> elementsToRender;
//        private readonly GraphicsContext2 gfx;
//
//
//        public VertigoRenderSystem(Camera camera, ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
//            this.camera = camera;
//            this.layoutSystem = layoutSystem;
//            this.views = new LightList<UIView>();
//            this.commandBuffer = new CommandBuffer(); // todo -- per view
//            this.styleSystem = styleSystem;
//            this.styleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
//            this.renderInfos = new IntMap<RenderInfo>();
//            this.elementsToRender = new LightList<UIElement>(0);
//            this.camera?.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
//            this.gfx = new GraphicsContext2(commandBuffer);
//        }
//
//        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> propertyList) {
//            int count = propertyList.size;
//            StyleProperty[] properties = propertyList.array;
//            for (int i = 0; i < count; i++) {
//                ref StyleProperty property = ref properties[i];
//
//                switch (property.propertyId) {
//                    case StylePropertyId.BorderColorTop:
//                    case StylePropertyId.BorderColorRight:
//                    case StylePropertyId.BorderColorBottom:
//                    case StylePropertyId.BorderColorLeft:
//                        break;
//
//                    case StylePropertyId.BorderTop:
//                    case StylePropertyId.BorderRight:
//                    case StylePropertyId.BorderBottom:
//                    case StylePropertyId.BorderLeft:
//                        break;
//                }
//            }
//
//            UpdateElementStyles(element);
//        }
//
//        public event Action<ImmediateRenderContext> DrawDebugOverlay;
//
//        public void OnReset() { }
//
//        public void OnUpdate() {
//            camera.orthographicSize = Screen.height * 0.5f;
//            float width = Screen.width * 0.5f;
//            float height = Screen.height * 0.5f;
//            Matrix4x4 matrix = camera.worldToCameraMatrix;
//            matrix *= Matrix4x4.Translate(new Vector3(-width, height));
//            gfx.BeginFrame(matrix, camera.projectionMatrix);
//
//            for (int i = 0; i < views.Count; i++) {
//                RenderView(views[i]);
//            }
//
//            gfx.EndFrame();
//        }
//
//        public class RenderNode {
//
//            public UIElement element;
//            public RenderNode parent;
//            public RenderNode firstChild;
//            public RenderNode lastChild;
//            public RenderNode nextSibling;
//            public RenderNode prevSibling;
//            
//            public void AddChild(RenderNode node) {
//                if (firstChild == null) {
//                    firstChild = node;
//                    lastChild = node;
//                }
//                else {
//                    if (lastChild == firstChild) {
//                        firstChild.nextSibling = node;
//                        lastChild = node;
//                    }
//                    else {
//                        lastChild.prevSibling.nextSibling = node;
//                        lastChild = node;
//                    }
//                }
//            }
//
//        }
//
//        public class RenderNodeTree {
//
//            public UIElement root;
//            public RenderTexture textureCache;
//            public PaintCache paintCache;
//            public int depth;
//            public int itemCount;
//
//        }
//
//        public class PaintCache { }
//
//        private void RenderTree(UIElement root) {
//        
//            LightList<UIElement> subtreeRoots = new LightList<UIElement>();
//            LightStack<UIElement> elementStack = new LightStack<UIElement>();
//            elementStack.Push(root);
//            subtreeRoots.Add(root);
//
//            int currentTreeSize = 0;
//            int maxDepth = 0;
//            int treeDepthLevels = 0;
//            int baseDepth = root.depth;
//            
//            RenderNode currentRenderNode = new RenderNode();
//            // caching the surface (render texture?)
//            // caching sequences of elements (repaint boundary)
//            
//            // repaint boundaries
//            //     manual boundaries
//            //     clipping
//            //     canvases
//            //     surfaces
//            
//            // for non complex things im probably better off redrawing
//            // for complex things that we don't need to repaint might as well repaint
//            
//            // element.GetRenderNodes();
//            // geometry cache vs draw list cache
//            // elements can cache geometry
//            // render trees cache draw lists
//            // cache can be cleared while draw list is not
//            // how do we gc caches? 
//            
//            while (elementStack.Count > 0) {
//            
//                UIElement current = elementStack.PopUnchecked();
//                
//                PaintMetaData metaData = current.GetPaintMetaData();
//
////                if (metaData.wasRepaintBoundary) { }
//
//                if (metaData.isRepaintBoundary) {
//                    subtreeRoots.Add(current);
//                }
//                else {
//                    
//                    currentTreeSize++;
//                    
//                    if (current.depth > maxDepth) {
//                        maxDepth = current.depth;
//                    }
//
//                    if (current.requiresPainting) { // non zero size in both axes && has paint || is paint modifier
//                        RenderNode node = new RenderNode();
//                        currentRenderNode.AddChild(node);
//                    }
//                 
//                    // if element.lastRenderTreeId != currentRenderTreeId
//                        // get cached data as needed from old cache (if !layout changed && complex & !requires repaint)
//                        // current cache.write(oldcache, location) 
//                        
//                    for (int i = current.children.Count - 1; i >= 0; i--) {
//                        elementStack.Push(current.children[i]);
//                        // if is repaint boundary -> next
//                        // if requires painting
//                            
//                    }
//                }
//            }
//
//            treeDepthLevels = maxDepth - baseDepth;
//            
//            // from here on we can parallelize
//            for (int i = 0; i < subtreeRoots.Count; i++) {
//                // diff with last frame or create dummy if last frame doesn't have one
//                RenderNode lastFrameTree = FindTreeRootFromLastFrame(subtreeRoots[i]);
//                
//            }
//            
//        }
//
//        private RenderNode FindTreeRootFromLastFrame(UIElement subtreeRoot) {
//            throw new NotImplementedException();
//        }
//
//        private void RenderView(UIView view) {
//            UIElement[] visibleElements = view.visibleElements.Array;
//            int count = view.visibleElements.Count;
//
//            if (count == 0) return;
//
//            UIForiaDrawer drawer = gfx.ActivateDrawer<UIForiaDrawer>();
//
//            for (int i = count - 1; i >= 0; i--) {
//                UIElement element = visibleElements[i];
//                LayoutResult layoutResult = element.layoutResult;
//                UpdateElementStyles(element); // todo -- don't do this here
//
//                // todo -- cull pass for non painters
//
//                renderInfos.TryGetValue(element.id, out RenderInfo renderInfo);
//
//                // todo -- replace this with local position & matrix passed to shader
//
//                float x = layoutResult.screenPosition.x;
//                float y = layoutResult.screenPosition.y;
//                float w = layoutResult.actualSize.width;
//                float h = layoutResult.actualSize.height;
//
//                switch (renderInfo.renderMethod) {
//                    case RenderMethod.None:
//                        break;
//
//                    case RenderMethod.Fill:
//                    case RenderMethod.UniformBorderFill:
//                        drawer.FillRectUniformBorder(x, y, w, h, renderInfo);
//                        break;
//
//                    case RenderMethod.MixedBorderFill:
//                    case RenderMethod.MixedBorderNoFill:
//                    case RenderMethod.UniformBorderNoFill:
//                        drawer.MixedBorderRect(x, y, w, h, renderInfo);
//                        break;
//
//                    case RenderMethod.Painter:
//                        renderInfo.painter.Paint(element, gfx, Matrix4x4.identity);
//                        drawer = gfx.ActivateDrawer(drawer);
//                        break;
//
//                    case RenderMethod.SelfPainter:
//                        renderInfo.selfPainter.Paint(gfx, Matrix4x4.identity);
//                        drawer = gfx.ActivateDrawer(drawer);
//                        break;
//
//                    case RenderMethod.Text:
//                        UITextElement textElement = (UITextElement) element;
//                        drawer.Text(x, y, textElement.TextInfo, renderInfo); // maybe also width & height so we can do relative uv mapping (ie masking or cutout)
//                        break;
//
//                    default:
//                        throw new ArgumentOutOfRangeException(renderInfo.renderMethod.ToString());
//                }
//            }
//        }
//
//        public void OnDestroy() { }
//
//        public void OnViewAdded(UIView view) {
//            views.Add(view);
//            // todo -- each view can take its own camera
//            // each view has its own camera origin
//            // each view can attach to a different camera event
//        }
//
//        public void OnViewRemoved(UIView view) {
//            views.Remove(view);
//        }
//
//        private static RenderMethod ComputeRenderType(UIElement element, in RenderInfo info) {
//            LayoutResult layoutResult = element.layoutResult;
//
//            if (info.visibility == Visibility.Hidden || info.opacity <= 0) {
//                return RenderMethod.None;
//            }
//
//            if (info.painter != null) {
//                return RenderMethod.Painter;
//            }
//            else if (info.selfPainter != null) {
//                return RenderMethod.SelfPainter;
//            }
//            else if (element is UITextElement) {
//                return RenderMethod.Text;
//            }
//
//            Texture bg = info.backgroundImage;
//            Color32 bgColor = info.backgroundColor;
//
//            bool hasBorder = !layoutResult.border.IsZero;
//            bool hasMixedBorder = hasBorder && !layoutResult.border.IsUniform;
//
//            bool hasBorderColor = hasBorder && (info.borderColorTop.a > 0 || info.borderColorRight.a > 0 || info.borderColorBottom.a > 0 || info.borderColorLeft.a > 0);
//
//            if ((bg == null && bgColor.a == 0 && (!hasBorderColor))) {
//                return RenderMethod.None;
//            }
//
//            RenderMethod retn = 0;
//
//            if (bg != null || bgColor.a != 0) {
//                retn |= RenderMethod.Fill;
//            }
//
//            if (hasBorder) {
//                if (hasMixedBorder) {
//                    retn |= RenderMethod.MixedBorder;
//                }
//                else {
//                    retn |= RenderMethod.UniformBorder;
//                }
//            }
//
//
//            return retn;
//        }
//
//
//        private void UpdateElementStyles(UIElement element) {
//            // if painter or self painter, skip most of this function
//
//            RenderInfo renderInfo = new RenderInfo();
//            // todo pre-compute border data, linearize colors, etc
//            UIStyleSet style = element.style;
//            renderInfo.backgroundColor = style.BackgroundColor;
//            renderInfo.backgroundImage = style.BackgroundImage;
//            renderInfo.backgroundRotation = style.BackgroundImageRotation.value; // todo -- resolve this to a float
//            renderInfo.opacity = style.Opacity;
//            // todo resolve to float
//            renderInfo.backgroundScale = new Vector2(style.BackgroundImageScaleX.value, style.BackgroundImageScaleY.value);
//            renderInfo.visibility = style.Visibility;
//            renderInfo.uvRect = new Rect(0, 0, 1, 1);
//            renderInfo.uvOffset = new Vector2(0, 0);
//            renderInfo.uvTiling = new Vector2(1, 1);
//            renderInfo.backgroundTint = style.BackgroundTint;
//            renderInfo.borderRadius = element.layoutResult.borderRadius;
//
//            renderInfo.clipRect = Vector4.zero; // todo -- wrong
//
//            renderInfo.borderSize = element.layoutResult.border;
//            renderInfo.borderColorTop = style.BorderColorTop;
//            renderInfo.borderColorRight = style.BorderColorRight;
//            renderInfo.borderColorBottom = style.BorderColorBottom;
//            renderInfo.borderColorLeft = style.BorderColorLeft;
//            renderInfo.scale = element.layoutResult.scale;
//            renderInfo.position = element.layoutResult.screenPosition;
//            renderInfo.rotation = 0; // todo -- fix this
//            renderInfo.renderMethod = ComputeRenderType(element, renderInfo);
//
//            float borderColorTop = VertigoUtil.ColorToFloat(style.BorderColorTop);
//            float borderColorRight = VertigoUtil.ColorToFloat(style.BorderColorRight);
//            float borderColorBottom = VertigoUtil.ColorToFloat(style.BorderColorBottom);
//            float borderColorLeft = VertigoUtil.ColorToFloat(style.BorderColorLeft);
//
//            renderInfo.packedBorderColors = new Vector4(borderColorTop, borderColorRight, borderColorBottom, borderColorLeft);
//
//            Color32 backgroundColor = style.BackgroundColor;
//            Color32 backgroundTint = style.BackgroundTint;
//            Texture backgroundImage = style.BackgroundImage;
//
//            float packedBackgroundColor = VertigoUtil.ColorToFloat(style.BackgroundColor);
//            float packedBackgroundTint = VertigoUtil.ColorToFloat(style.BackgroundTint);
//            PaintMode colorMode = PaintMode.None;
//
//            if (!ReferenceEquals(backgroundImage, null)) {
//                colorMode |= PaintMode.Texture;
//            }
//
//            if (backgroundTint.a > 0) {
//                colorMode |= PaintMode.TextureTint;
//            }
//
//            if (backgroundColor.a > 0) {
//                colorMode |= PaintMode.Color;
//            }
//
//            renderInfo.packedColorData = new Vector4(packedBackgroundColor, packedBackgroundTint, (int) colorMode, 0);
//            renderInfos[element.id] = renderInfo;
//        }
//
//        public void OnElementEnabled(UIElement element) {
//            LightStack<UIElement> stack = LightStack<UIElement>.Get();
//            stack.Push(element);
//            while (stack.Count > 0) {
//                UIElement current = stack.PopUnchecked();
//
//                if (current.isDisabled) {
//                    continue;
//                }
//
//                // todo -- only if render-relevant style
//                UpdateElementStyles(current);
//
//                int childCount = current.children.Count;
//                UIElement[] children = current.children.Array;
//                for (int i = 0; i < childCount; i++) {
//                    stack.Push(children[i]);
//                }
//            }
//
//            LightStack<UIElement>.Release(ref stack);
//        }
//
//        public void OnElementDisabled(UIElement element) {
//            renderInfos.Remove(element.id);
//        }
//
//        public void OnElementDestroyed(UIElement element) {
//            renderInfos.Remove(element.id);
//        }
//
//        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }
//
//        public void OnElementCreated(UIElement element) { }
//
//        public void SetCamera(Camera camera) {
//            this.camera?.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
//            this.camera = camera; // todo -- should be handled by the view
//            this.camera?.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
//        }
//
//    }
//
//}