using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThisOtherThing.UI;
using UIForia.Graphics.ShapeKit;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

namespace UIForia.Graphics {

    public enum CommandType {

        DrawMesh,
        SetRenderTarget,
        Clear,
        SetGlobalTexture,
        Blit,
        CopyTexture,
        DrawBatch

    }

    public struct CommandBufferCommand {

        public CommandType type;
        public int meshId;
        public int batchIndex;
        public int matrixId;
        public MaterialKey materialKey;

    }

    // the problem with parallel 
    // 1. user code
    // 2. state changes

    // state change I can probably reconcile with a sort

    // i need to know state boundaries for batching, but probably not much else since we reset element state between painter calls
    // so if i have a command type i can just sort by where it belongs 
    // don't need to store all the data, just an index and a local offset
    // this means multiple contexts that need to be merged
    // so any id references need to take a context id + offset
    // merge all contexts, adjust offset ids? or just store and resolve with addition

    // for pre-rendering text
    // 2 options
    // 1. dont. may actually be effective with no-overlap by-word which is likely to be frequent. lets do this until it doesn't work
    // 2. try to atlas and sample it later

    // materials
    // if user sets material explicitly by reference, respect it, still try to batch it unless no-batch is set on material
    // if user set material via materialId, respect it, try to batch it,
    // if user then sets material properties for that material -> give it a new material id, do not try to batch unless it was a texture that was set

    // draw command -> sorted later so we can ignore ordering of render calls until we need it

    // for built in elements we can start the mesh creation phase as soon as layout finishes vertically since I don't need their position until actually rendering
    // will just need to ensure the given element isn't culled later on

    // ordering
    // first sort the contexts by their natural occurence (traversalIndex, zOrder, layer, etc)
    // now find the render target changes (if any)
    //    filter them to get opaque and transparent independently
    //    sort opaque ftb and transparent btf

    // batching
    // same material w/ different texture -> use dynamic atlas, maybe only possible with UIForia materials

    [Flags]
    public enum DrawType {

        Mesh = 1 << 0,
        Rect = 1 << 1,
        RoundedRect = 1 << 2,
        Arc = 1 << 3,
        Polygon = 1 << 4,
        Line = 1 << 5,
        Ellipse = 1 << 6,
        SetRenderTarget = 1 << 7,
        SDFText = 1 << 8,

        Shape = Rect | RoundedRect | Arc | Polygon | Line | Ellipse,

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ShapeInfo {

        [FieldOffset(0)] public ShapeRange shapeRange;
        [FieldOffset(8)] public int dataBufferId;
        [FieldOffset(12)] public RectData rectData;

    }

    public struct MaterialKey {

        public MaterialId baseMaterial;
        public int propertyStart;
        public int propertyCount;
        public int textureStart;
        public int textureCount;
        public int materialIndex;

        public bool isBatchable {
            get => propertyCount == 0;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialPropertyValue {

        [FieldOffset(0)] public int textureId;
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public Color colorValue;
        [FieldOffset(0)] public Vector4 vectorValue;

    }

    public struct MaterialPropertyOverride {

        public MaterialPropertyType propertyType;
        public MaterialPropertyValue value;

    }

    public unsafe class RenderContext2 : RenderContextInfo {

        internal int frameId;
        internal ElementId currentElementId;
        internal int currentRenderCallIdx;
        internal ushort localDrawIdx;
        internal MaterialId activeMaterialId;
        private DrawMode drawMode;
        internal MaterialDatabase materialDatabase;
        private bool hadDrawCallAfterLastMaterialUpdate;
        
        internal PagedByteAllocator stackBuffer;

        internal MaterialKey activeMaterialKey;

        private RangeInt materialPropertyRange;
        private RangeInt materialTextureRange;
        private float4x4* defaultMatrix;

        internal RenderContext2(MaterialDatabase materialDatabase) {
            this.materialDatabase = materialDatabase;
            this.stackBuffer = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.TempJob);
        }

        internal new void Clear() {
            base.Clear();
            stackBuffer.Clear();
        }

        public void SetAntiAliasingWidth(float width) { }

        public void SetShadow(float size, float softness) { }

        public void SetTransform(float4x4 matrix) { }

        public void SetTransform(Matrix4x4 matrix4X4) { }

        public void StrokeRect(float x, float y, float width, float height) { }

        public void SetDrawMode(DrawMode drawMode) { }

        // each render texture we push needs its own draw info list and we cannot batch between them

        public void SetMaterial(MaterialId materialId) {
            activeMaterialId = materialId;
            materialPropertyRange = new RangeInt(propertyOverrides.size, 0);
            materialTextureRange = new RangeInt(textureIds.size, 0);
        }

        // any non texture property override excludes it from batching with any id other than itself

        // when combining render contexts, ill need to update offsets for texture and property overrides for material keys

        // materialKey {
        //    bool isUIForiaBatchable = propertyOverrides.size == 0
        //    MaterialId baseMaterial
        //    RangeInt textureOverrides;
        //    RangeInt propertyOverrides;
        // }

        public void SetMaterial(Material material) {
            // for (int i = 0; i < materialList.size; i++) {
            //     if (materialList.array[i].material == material) {
            //         activeMaterialKey = materialKeys[materialList[i].materialKeyId];
            //     }
            // }
        }

        public struct MaterialUsage {

            public MaterialId materialId;
            public RangeInt propertyRange;
            public RangeInt textureRange;

        }

        public struct ShapeId {

            public int index;

        }

        public struct ShapeInterface {

            private RenderContext2 ctx;
            private int frameId;

            private bool Validate() {
                return ctx.frameId == frameId;
            }

            public void GetTexCoord0(ShapeId shapeId, List<Vector4> texCoords) {
                if (!Validate()) {
                    // throw
                }
                
                // ctx.drawList[shapeId.index].shapeGeometrySource
                
            }

        }

        public ShapeInterface GetShapeInterface() {
            return new ShapeInterface();
        }

        public void SetMaterialFloat(string key, float value) {
            MaterialInfo mat = materialDatabase.materialMap[activeMaterialId.id];

            if (!materialDatabase.HasFloatProperty(mat, key)) {
                return;
            }

            if (hadDrawCallAfterLastMaterialUpdate) {
                // make a new entry        
            }

            materialPropertyRange.length++;

            propertyOverrides.Add(new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Float,
                value = new MaterialPropertyValue() {
                    floatValue = value
                }
            });

        }

        public void SetMaterialTexture(string key, SpriteAtlas texture, string spriteId) {
            // GetSprite CLONES the sprite!!!!!
            // texture.GetSprite(spriteId).textureRect;
        }

        public void SetMaterialTexture(string key, Texture texture) {
            MaterialInfo mat = materialDatabase.materialMap[activeMaterialId.id];

            // if material is UIForia material that accepts texture atlas

            if (!materialDatabase.HasTextureProperty(mat, key)) {
                return;
            }

        }

        private Color32 color;

        public void SetColor(Color32 color) {
            this.color = color;
        }
        
        public void FillRect(float2 position, float2 size) {
            // aa data
            // dpi -> part of aa?
            // edge gradient
            // draw mode
            // matrix id

            drawList.Add(new DrawInfo() {
                type = DrawType.Rect,
                localDrawIdx = localDrawIdx,
                vertexLayout = VertexLayout.UIForiaDefault,
                matrix = defaultMatrix,
                shapeData = (byte*) stackBuffer.Allocate(new RectData() {
                    type = ShapeMode.Fill | ShapeMode.AA,
                    x = position.x,
                    y = position.y,
                    width = size.x,
                    height = size.y,
                    color = color,
                    edgeGradient = new EdgeGradientData() { }
                })
                // materialKeyIndex = activeMaterialId,
            });

            // todo -- AA ring should be handled separately so it ends up in transparent regardless of body queue
            localDrawIdx++;
        }

        internal void Setup(ElementId elementId, int renderCallIdx, float4x4* matrix) {
            localDrawIdx = 0;
            activeMaterialId = new MaterialId();
            currentRenderCallIdx = renderCallIdx;
            currentElementId = elementId;
            defaultMatrix = matrix;
            color = Color.magenta;
        }

    }

    [Flags]
    public enum ShapeMode {

        Fill = 1 << 0,
        Shadow = 1 << 1,
        Stroke = 1 << 2,
        AA = 1 << 3

    }

}