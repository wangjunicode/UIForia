using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {
    
    public struct ElementRenderInfo {

        // public Texture backgroundTexture;
            
        public UIColor borderColorTop;
        public UIColor borderColorRight;
        public UIColor borderColorBottom;
        public UIColor borderColorLeft;
            
        public UIColor backgroundColor;
        public UIColor backgroundTint;
            
        public float cornerRadiusTopLeft;
        public float cornerRadiusTopRight;
        public float cornerRadiusBottomRight;
        public float cornerRadiusBottomLeft;
            
        public float cornerBevelTopLeft;
        public float cornerBevelTopRight;
        public float cornerBevelBottomRight;
        public float cornerBevelBottomLeft;

        public ushort backgroundRectMinX;
        public ushort backgroundRectMinY;
        public ushort backgroundRectMaxX;
        public ushort backgroundRectMaxY;

        public ushort backgroundImageOffsetX;
        public ushort backgroundImageOffsetY;
        public float backgroundImageScaleX;
        public float backgroundImageScaleY;
        public float backgroundImageTileX;
        public float backgroundImageTileY;
        public float backgroundImageRotation;
        public BackgroundFit backgroundImageFit;

        public MeshFillDirection meshFillDirection;
        public MeshFillOrigin meshFillOrigin;
        public float meshFillAmount;
        public float meshFillRotation;
        public float meshFillRadius;
        public float meshFillOffsetX;
        public float meshFillOffsetY;

    }
    
    public enum RenderCommandType {

        DrawElement,
        DrawText,
        DrawPainter,
        PushMask,
        PopMask,
        PushEffect,
        PopEffect

    }

    public abstract class UIPainter<T> : UIPainter where T : struct {

        public abstract void SetParameters(int callIdx, UIElement element, T parameters);

    }

    public class UIPainter {

        public void PaintElement(UIRenderContext2D context, UIElement cmdElement) { }

        public virtual JobHandle SchedulePaintForeground() {
            throw new NotImplementedException();
        }

        public virtual JobHandle SchedulePaintBackground() {
            throw new NotImplementedException();
        }

        public virtual JobHandle ScheduleUpdate() {
            throw new NotImplementedException();
        }

        public virtual void OnSetupFrame() {
            throw new NotImplementedException();
        }

        public virtual void OnTeardownFrame() {
            throw new NotImplementedException();
        }

        public virtual void OnUpdate() {
            throw new NotImplementedException();
        }

        public virtual void PaintForeground(ref UIRenderContext2D ctx) {
            throw new NotImplementedException();
        }

        public virtual void PaintBackground(ref UIRenderContext2D ctx) {
            throw new NotImplementedException();
        }

    }

    public class UIMask { }

    public class UIEffect {

        public void Setup(UIRenderContext2D context) {
            throw new NotImplementedException();
        }

        public void Draw(UIRenderContext2D context) {
            throw new NotImplementedException();
        }

    }

    public class BatchPainter { }

    public struct RenderCommand {

        public RenderCommandType type;
        public UIElement element;
        public UIPainter uiPainter;
        public UIMask mask;
        public UIEffect effect;
        public int index;
        public int lastChildIndex;

    }

    public class RenderBatch {

        public LightList<Mesh> meshes;
        public LightList<Material> materials;
        public LightList<float4x4> matrices;

    }

    public class UITextureMask : UIMask {

        public void Push(UIRenderContext2D context) {
            // Rect rect = context.GetLayoutBounds(elementRange);
            RenderTexture texture = RenderTexture.GetTemporary(100, 100, 24);
        }

        public void Pop(UIRenderContext2D context, RenderTexture texture, Rect rect) {
            Mesh mesh = new Mesh();
            Material material = default;
            context.commandBuffer.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0);
        }

    }

    public class ElementPainter : UIPainter {

        private Mesh mesh;
        private Material material;

        public void OnInitialize() {
            mesh = new Mesh();
            material = Resources.Load<Material>("ElementMaterial");
        }

        public void OnBecameVisible() { }

        public void OnBecameInvisible() { }

        public void Update(UIRenderContext2D context) {

            // i want to avoid material setup where posssible
            // can highly optimize the built in material stuff
            // dont want to run a bunch of user code if I dont have to do though

            // mostly around text meshes for built in stuff 
            // dont really need infrastructure for built in shader 
            // can burst it all easily 

            material.EnableKeyword("BEVEL");
            material.EnableKeyword("ROUND");
            material.EnableKeyword("REVEAL");
            material.EnableKeyword("UV_TRANSFORM");
            material.EnableKeyword("FRAGMENT_MASK");

            material.SetFloat("_BevelAmount", 5f);
            material.SetFloat("_BevelAmount", 5f);
            material.SetFloat("_BevelAmount", 5f);
            material.SetFloat("_BevelAmount", 5f);
            material.SetFloat("_BevelAmount", 5f);
            material.SetFloat("_OutlineSize", 5f);
            material.SetFloat("_OutlineColor", 5f);

            material.SetVector("_Size", new Vector4(100, 100, 0, 0));

            UIElement element = default;
            OffsetRect borders = element.GetLayoutBorders();
            Size size = element.GetLayoutSize();
            float4x4 matrix = element.GetLayoutWorldMatrix();
            context.commandBuffer.DrawMesh(mesh, matrix, material, 0, 0);

            // context.drawList.Add(new DrawStruct() {
            //     commandBufferToExecute,
            //     someCallback = () => { }
            // });
            //
            // context.drawList.Add(new DrawStruct() {
            //     material, matrix, mesh, whatever
            // });

            // get temp textures
            // draw children to texture
            // use texture to do stuff

        }

    }

    internal struct Renderable {

        public object renderable;
        public UIElement element;
        public RenderableType type;

    }

    internal enum RenderableType {

        DefaultText,
        DefaultElement,
        Painter,
        Effect,
        Mask

    }

    public unsafe class RenderLoop {

        internal AppInfo* appInfo;
        internal UIApplication application;
        internal StructList<RenderCommand> renderCommands;
        internal PerFrameLayoutOutput* perFrameLayoutOutput;

        internal StructList<Renderable> renderables;

       

        public void Update() {

            CheckedArray<ElementId> activeElements = new CheckedArray<ElementId>(appInfo->indexToElementId, appInfo->layoutTree.elementCount);

            StructList<ElementId> culledList = new StructList<ElementId>();
            StructList<ElementId> defaultPained = new StructList<ElementId>();
            StructList<ElementId> customPainted = new StructList<ElementId>();

            // order matters a lot here
            // we already know draw order
            // so could just sort
            // but single thread makes sense for user stuff
            // burst makes sense for internal stuff 
            // need to join or otherwise re-sort 
            // can probably have a zip job 

            UIRenderContext2D ctx = default;

            // should know at compile time if we need to paint foreground and background or just one of them

            // for built in elements i need to figure out which set of mesh fill/bg-mutations to use (if any)
            // probably want a job(s) to classify those 
            // need to know which elements require them 
            
            // borders -> always rectangular, might be texturable
            // outline -> follows sdf contours, might be texturable 
            
            // update pass, then do drawing 
            // update called regardless of culling, but cull info is sent to update 
            // painters always called, can emit no instructions if they want 
            
            // shared painter vs instance painter concept
            // shared works in batches 
            // shared can schedule its works in jobs 
            
            // culling == don't emit or don't invoke?, maybe both
            
            // painter.Update(element.cullingInfo.isCulled)
            
            // if(painter.GetUpdateJob() != default) { await it }
            // else painter.Update(elementsUsingPainter) { }
            // need painter to assign material ids 
            // need painter to determine what to draw 
            //  
            
            for (int i = 0; i < renderables.size; i++) {

                ref Renderable renderable = ref renderables.array[i];

                switch (renderable.type) {

                    case RenderableType.DefaultElement: {

                        int elementIndex = renderable.element.elementId.index;

                        ctx.SetMaterial(new ModuleAndNameKey("uiforia", "default"));
                        ctx.PushMatrix(float4x4.identity);

                        ElementRenderInfo renderInfo = default;

                        // must render = bgColor.a > 0 | bgTint.a > 0 | bgImage != null | non-zero size;
                        // material features
                            
                            // radius/bevel is default when shape == rect, maybe thats just part of the shape description 
                            // has-mesh-fill
                            // has-bg-mutations
                            
                        // materialId = GetShapeType(element);
                         
                        ctx.PopMatrix();
                        break;
                    }

                    case RenderableType.DefaultText:
                        ctx.SetMaterial(new ModuleAndNameKey("uiforia", "textdefault"));
                        break;

                    case RenderableType.Painter:
                        UIPainter painter = (UIPainter) renderable.renderable;
                        painter.PaintElement(ctx, renderable.element);
                        ctx.SetMaterial(new ModuleAndNameKey("uiforia", "default"));
                        break;

                    case RenderableType.Effect:
                        ctx.SetMaterial(new ModuleAndNameKey("uiforia", "default"));
                        break;

                    case RenderableType.Mask:
                        ctx.SetMaterial(new ModuleAndNameKey("uiforia", "default"));
                        break;

                }

            }

            for (int i = 0; i < activeElements.size; i++) {
                PainterId painterId = appInfo->styleTables.Painter[activeElements[i].index];

                if (perFrameLayoutOutput->clipInfos[i].isCulled) {
                    culledList.Add(activeElements[i]);
                }
                // todo -- don't use this, use an ElementId sized table for lookups 
                else if (painterId.id == default) {
                    defaultPained.Add(activeElements[i]);
                }
                else {
                    customPainted.Add(activeElements[i]);
                }

            }

            // update built in material setups

            // update custom material setups 

            // invoke became invisible if custom painted, culled, and was visible
            // invoke become visible if custom painted, not culled, and wasn't visibile

        }

        public struct DrawInfo {

            public DrawInfoType type;

        }

        public enum DrawInfoType {

            RenderElement,
            RenderText,
            ExecuteCommandBuffer,
            SetRenderTarget,
            PushRenderTarget,
            PopRenderTarget,
            RenderMesh,

            RenderTextElement,

            PushMask,

            PopMask

        }

        public void Render() {

            UIRenderContext2D context = new UIRenderContext2D();

            // compute mesh bounds (post transform? might be needed but kind of sucks)
            // how do i handle culling? 
            // painters emit 1 mesh 1 material or multiple? 
            // abstract over command buffer? might let me thread more 
            // need matrices
            // need culling
            // need target surface
            // need to handle clip behavior & visibility & z-index
            // ideally we have a batcher to handle out of order rendering
            // can i lean on instancing for that?

            // whats my primitive?
            // - commnand buffer
            // less work to do, likely faster
            // user can directly inject 
            // - command buffer abstraction
            // can allow multi pass techniques
            // probably a bit slower
            // doesn't have full cmd buffer api 

            // i need a draw format that can collect a bunch of shit to do at once across surfaces
            // this breaks when
            // shader doesn't support instancing
            // shader doesn't support uiforia clipping 
            // shader isn't batch compatible 

            // group commands by surface target
            // need to invoke updates on effects? 
            // painter could push/pop draw cmd too

            StructList<DrawInfo> drawList = new StructList<DrawInfo>();
            ElementMap defaultPaintedMap = default;

            for (int i = 0; i < renderCommands.size; i++) {

                RenderCommand cmd = renderCommands[i];

                // if cmd.isCulled -> continue

                switch (cmd.type) {

                    case RenderCommandType.DrawElement: {

                        if (defaultPaintedMap.Get(cmd.element.elementId)) {
                            drawList.Add(new DrawInfo() {
                                type = cmd.element is UITextElement ? DrawInfoType.RenderTextElement : DrawInfoType.RenderElement
                            });
                        }
                        else {
                            // get painter
                            UIPainter painter = default;
                            painter.PaintElement(context, cmd.element);
                        }

                        break;
                    }

                    case RenderCommandType.DrawText:
                        break;

                    case RenderCommandType.DrawPainter:
                        break;

                    case RenderCommandType.PushMask: {

                        drawList.Add(new DrawInfo() {
                            type = DrawInfoType.PushMask,
                            // maskInfo = cmd.mask
                        });
                        break;
                    }

                    case RenderCommandType.PopMask: {
                        drawList.Add(new DrawInfo() {
                            type = DrawInfoType.PopMask,
                            // maskInfo = cmd.mask
                        });
                        break;
                    }

                    case RenderCommandType.PushEffect: {
                        cmd.effect.Setup(context);
                        break;
                    }

                    case RenderCommandType.PopEffect:
                        cmd.effect.Draw(context);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // at this point we know everything we want to draw
            // from here on out its 100% burst until we actually submit the render calls 
            // we need to do transformation, culling, batching, and submission
            // not sure how to handle clip behaviors & z-index changes 

            // first thing to do is get bounds for everything
            // need this for effect / mask boundaries

            for (int i = 0; i < drawList.size; i++) {

                DrawInfo info = drawList[i];

                switch (info.type) {

                    case DrawInfoType.RenderTextElement:
                    case DrawInfoType.RenderElement: {
                        OrientedBounds bounds = appInfo->perFrameLayoutOutput.bounds[i];
                        break;
                    }

                    case DrawInfoType.ExecuteCommandBuffer: {
                        // unknowable 
                        break;
                    }

                    case DrawInfoType.SetRenderTarget:
                        break;

                    case DrawInfoType.PushRenderTarget: {
                        // as long as the dest is not the same texture
                        // two batches can be drawn at the same time 
                        // if the src has the same format
                        // and there is enough size to accomodate both 
                        // can I recycle regions? not sure because of stencil, may need to clear it manually  
                        break;
                    }

                    case DrawInfoType.PopRenderTarget:
                        break;

                    case DrawInfoType.RenderMesh:
                        break;

                    case DrawInfoType.RenderText:
                        break;

                    case DrawInfoType.PushMask:
                        break;

                    case DrawInfoType.PopMask:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        public class DrawSurface {

            public DrawSurface output;
            public RenderTextureFormat format;
            public SizeInt size;
            public Rect outputBounds;
            public RenderTargetIdentifier surfaceId;

            public void DrawElement(ElementId elementId) { }

            public void DrawMesh(Mesh mesh, Material material, Matrix4x4 matrix) { }

        }

    }

}