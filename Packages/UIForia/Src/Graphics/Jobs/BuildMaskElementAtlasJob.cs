using UIForia.ListTypes;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public unsafe struct MaskInfo {

        public int drawInfoId; // i dont know how to resolve this! will need to offset this per-rendercontext when merging draw info lists
        // if sorting, also need to track that somehow, might just sort the ids and not the drawinfos

        public GeometryInfo* geometry;
        public AxisAlignedBounds2D bounds;
        public float4 blockUVs;
        public int textureId;
        public MaskType maskType;

    }

    internal struct MaskTarget {

        public int width;
        public int height;
        public List_Int32 shapeContents; // todo -- implement for caching?
        public int textureId;
        public RangeInt drawRange;

    }

    // [BurstCompile]
    internal unsafe struct BuildMaskElementAtlasJob : IJob {

        public DataList<MaskInfo>.Shared maskElementList;
        public DataList<MaskTarget>.Shared maskTargetList;
        public PagedByteAllocator pagedByteAllocator;
        public DataList<Batch>.Shared batchList;

        public void Execute() {

            float width = 256;
            float height = 256;

            float textureWidth = 2048;
            float textureHeight = 2048;

            int blockIdx = 0;

            float x = 0;
            float y = 0;
            DataList<VertexChannelDesc> vertexChannelList = new DataList<VertexChannelDesc>(16, Allocator.Temp);

            List_Int32 softGeometryList = new List_Int32(16, Allocator.Temp);

            maskTargetList.Add(new MaskTarget() {
                width = 2048,
                height = 2048,
            });

            // todo --  sort by largest (clipped) bounds

            for (int i = 0; i < maskElementList.size; i++) {

                ref MaskInfo maskInfo = ref maskElementList[i];

                // allocate space
                // set clip bounds id (make sure we dont exceed rect)
                // draw into the space
                maskInfo.textureId = 0;
                maskInfo.blockUVs = new float4(x, y, width / textureWidth, height / textureHeight);

                // need the oriented bounds & batching & set the data id somehow

                // i need to batch here too but only after space allocation

                blockIdx++;

            }

            // I already have mesh geometry from BakeShapes at this point

            for (int i = 0; i < maskElementList.size; i++) {
                ref MaskInfo maskInfo = ref maskElementList[i];

                switch (maskInfo.maskType) {

                    case MaskType.Bounds:
                        break;

                    case MaskType.SoftGeometry:
                        softGeometryList.Add(i);
                        break;

                    default:
                    case MaskType.HardGeometry:
                        break;

                    case MaskType.Material:
                        break;

                    case MaskType.Texture:
                        break;

                }

            }

            if (softGeometryList.size > 0) {

                int vertexCount = 0;
                int triangleCount = 0;

                for (int i = 0; i < softGeometryList.size; i++) {
                    ref MaskInfo maskInfo = ref maskElementList[softGeometryList[i]];
                    vertexCount += maskInfo.geometry->vertexCount;
                    triangleCount += maskInfo.geometry->triangleCount;
                }
               
                int* triangles = pagedByteAllocator.Allocate<int>(triangleCount);
                Color32* colors = pagedByteAllocator.Allocate<Color32>(vertexCount);
                float3* positions = pagedByteAllocator.Allocate<float3>(vertexCount);

                vertexChannelList.Add(new VertexChannelDesc() {
                    channel = VertexChannel.Position,
                    format = VertexChannelFormat.Float3,
                    ptr = positions
                });
                
                vertexChannelList.Add(new VertexChannelDesc() {
                    channel = VertexChannel.Color,
                    format = VertexChannelFormat.Float1,
                    ptr = colors
                });
                
                for (int i = 0; i < softGeometryList.size; i++) {

                    ref MaskInfo maskInfo = ref maskElementList[softGeometryList[i]];

                    TypedUnsafe.MemCpy(positions, maskInfo.geometry->positions, maskInfo.geometry->vertexCount);
                    TypedUnsafe.MemCpy(colors, maskInfo.geometry->colors, maskInfo.geometry->vertexCount);
                    TypedUnsafe.MemCpy(triangles, maskInfo.geometry->triangles, maskInfo.geometry->triangleCount);

                    positions += maskInfo.geometry->vertexCount;
                    colors += maskInfo.geometry->vertexCount;
                    triangles += maskInfo.geometry->triangleCount;

                }

                Batch batch = new Batch() {
                    geometry = pagedByteAllocator.Allocate(vertexChannelList.GetArrayPointer(), vertexChannelList.size),
                    triangles = triangles,
                    triangleCount = triangleCount,
                    batchType = BatchType.Mask,
                    materialId = MaterialId.UIForiaSoftGeometryMask,
                    overflowBounds = null,
                    meshId = 0,
                    propertyOverrides = null,
                    vertexCount = vertexCount,
                    vertexChannelCount = vertexChannelList.size,
                    vertexLayout = new VertexLayout() {
                        color = VertexChannelFormat.Float1
                    }
                };
                    
                // 2 issues
                // 1. how do i handle nested masks when they come from different places?
                // probably with a sort?
                // how do i make masks reuse geometry

                // are mask calls just batches?
                // yes i think so
                // duplicate geometry? prefer not to
                // masks are pushed, not set
                // they can get popped
                // then sorting should handle it
                
                GeometryInfo geometryInfo = new GeometryInfo() {
                    colors = colors,
                    positions = positions,
                    triangles = triangles,
                    vertexCount = vertexCount,
                    triangleCount = triangleCount,
                    vertexLayout = new VertexLayout() {
                        color = VertexChannelFormat.Float1,
                    }
                };

                // maskDrawCallList.Add(new MaskDrawCall() {
                //     boundsCount = 0,
                //     boundsPointer = null,
                //     geometryInfo = geometryInfo,
                //     materialId = MaterialId.UIForiaSoftGeometryMask,
                //     scissorRect = default
                // });
                
                ref MaskTarget target = ref maskTargetList[0];
                target.drawRange.length++;
                
            }

            softGeometryList.Dispose();
            vertexChannelList.Dispose();
        }

    }

}