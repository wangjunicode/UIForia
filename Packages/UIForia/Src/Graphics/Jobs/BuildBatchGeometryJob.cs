using System;
using UIForia.Graphics.ShapeKit;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Graphics {

    public struct GeometryBuffer : IPerThreadCompatible {

        public PagedByteAllocator data;

        public void Dispose() {
            data.Dispose();
        }

        public void InitializeForThread(Allocator allocator) {
            data = new PagedByteAllocator(TypedUnsafe.Kilobytes(32), allocator, Allocator.TempJob);
        }

        public bool IsInitialized {
            get => data.basePageByteSize != 0;
        }

    }

    [BurstCompile]
    internal unsafe struct BuildBatchGeometryJob : IJob, IVertigoParallelDeferred {

        public DataList<Batch>.Shared batchList;
        public DataList<DrawInfo>.Shared drawList;
        public DataList<int>.Shared batchMemberList;

        public PerThread<GeometryBuffer> perThread_GeometryBuffer;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams.Deferred defer { get; set; }

        public void Execute(int start, int end) {
            Run(start, end);
        }

        public void Execute() {
            Run(0, batchList.size);
        }

        private void Run(int start, int end) {

            DataList<VertexChannelDesc> vertexChannelList = new DataList<VertexChannelDesc>(16, Allocator.Temp);

            ref GeometryBuffer geometryBuffer = ref perThread_GeometryBuffer.GetForThread(threadIndex);

            for (int batchIdx = start; batchIdx < end; batchIdx++) {

                ref Batch batch = ref batchList[batchIdx];

                // todo -- if transforming vertices, check if the matrix has rotation / scale before blinding doing big matrix operations

                int batchStart = batch.memberIdRange.start;
                int batchEnd = batch.memberIdRange.end;

                int vertexCount = 0;
                int triangleCount = 0;

                // todo -- no extra vertices at this point

                for (int i = batchStart; i < batchEnd; i++) {

                    ref DrawInfo drawInfo = ref drawList[batchMemberList[i]];

                    vertexCount += drawInfo.shapeRange.vertexRange.length;
                    triangleCount += drawInfo.shapeRange.triangleRange.length;

                }

                GetVertexChannels(ref vertexChannelList, batch.vertexLayout, ref geometryBuffer, vertexCount);
                int* trianglePtr = geometryBuffer.data.Allocate<int>(triangleCount);

                VertexChannelDesc* vertexChannelDesc = geometryBuffer.data.Allocate(vertexChannelList.GetArrayPointer(), vertexChannelList.size);

                batch.vertexChannelCount = vertexChannelList.size;
                batch.geometry = vertexChannelDesc;
                batch.triangles = trianglePtr;
                batch.triangleCount = triangleCount;
                batch.vertexCount = vertexCount;

                // todo -- i need a matrix
                // todo -- maybe i should tell drawInfo its vertex/triangle offset in batch

                for (int vcIdx = 0; vcIdx < vertexChannelList.size; vcIdx++) {

                    ref VertexChannelDesc channelDesc = ref vertexChannelDesc[vcIdx];
                    int channelSize = (int) channelDesc.format;
                    byte* geometry = channelDesc.ptr;

                    for (int i = batchStart; i < batchEnd; i++) {
                        ref DrawInfo drawInfo = ref drawList[batchMemberList[i]];
                        int drawVertexCount = drawInfo.shapeRange.vertexRange.length;
                        byte* dataPtr = drawInfo.GetChannel(channelDesc.channel);
                        UnsafeUtility.MemCpy(geometry, dataPtr, channelSize * drawVertexCount);
                        geometry += (channelSize * drawVertexCount);
                    }
                }

                int vOffset = 0;
                for (int i = batchStart; i < batchEnd; i++) {
                    ref DrawInfo drawInfo = ref drawList[batchMemberList[i]];
                    int* triangles = drawInfo.GetTriangles();
                    int drawTriangleCount = drawInfo.shapeRange.triangleRange.length;
                    for (int t = 0; t < drawTriangleCount; t++) {
                        trianglePtr[t] = triangles[t] + vOffset;
                    }

                    vOffset += drawInfo.shapeRange.vertexRange.length;
                    // UnsafeUtility.MemCpy(trianglePtr, triangles, sizeof(int) * drawTriangleCount);
                    trianglePtr += drawTriangleCount;
                }

                float3* positions = (float3*) vertexChannelDesc[0].ptr;
                for (int i = batchStart; i < batchEnd; i++) {
                    ref DrawInfo drawInfo = ref drawList[batchMemberList[i]];
                    int count = drawInfo.shapeRange.vertexRange.length;

                    float4x4 matrix = *drawInfo.matrix;

                    for (int j = 0; j < count; j++) {
                        positions[j] = math.transform(matrix, positions[j]);
                        // positions[j] += pos;// new float3(i * 100, i * 100, 0); 
                        //     //math.mul(*drawInfo.matrix, new float4(positions[j].xyz,1)).xyz);math.mul(*drawInfo.matrix, new float4(positions[j].xyz,1)).xyz; // math.mul(positions[j], drawInfo.matrix);
                    }

                    positions += count;

                }

            }

            vertexChannelList.Dispose();
        }

        private static void GetVertexChannels(ref DataList<VertexChannelDesc> output, in VertexLayout layout, ref GeometryBuffer buffer, int vertexCount) {
            output.size = 0;

            // position is always index 0

            output[output.size++] = new VertexChannelDesc() {
                channel = VertexChannel.Position,
                format = VertexChannelFormat.Float3,
                ptr = (byte*) buffer.data.Allocate<float3>(vertexCount)
            };

            if (layout.color != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.color;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.Color,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.normal != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.normal;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.Normal,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord0 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord0;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord0,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord1 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord1;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord1,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord2 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord2;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord2,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord3 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord3;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord3,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord4 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord4;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord4,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord5 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord5;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord5,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord6 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord6;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord6,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }

            if (layout.texCoord7 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord7;
                output[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord7,
                    format = format,
                    ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
                };
            }
        }

    }

    public unsafe struct VertexChannelDesc {

        public VertexChannel channel;
        public VertexChannelFormat format;
        public byte* ptr;

        public int GetUVChannelIndex() {
            switch (channel) {
                default:
                case VertexChannel.Normal:
                case VertexChannel.Color:
                case VertexChannel.Tangent:
                    return -1;

                case VertexChannel.TextureCoord0:
                    return 0;

                case VertexChannel.TextureCoord1:
                    return 1;

                case VertexChannel.TextureCoord2:
                    return 2;

                case VertexChannel.TextureCoord3:
                    return 3;

                case VertexChannel.TextureCoord4:
                    return 4;

                case VertexChannel.TextureCoord5:
                    return 5;

                case VertexChannel.TextureCoord6:
                    return 6;

                case VertexChannel.TextureCoord7:
                    return 7;
            }
        }

    }

}

// do i know final batch boundaries at this point? maybe not

// what work needs to be done?

// we'll need to modify vertices for uiforia batching, at least add texCoord2 or slot our data index in somewhere

// FillRect();
// ApplyVertexModifier(new VertexModifier().addChannel(TexCoord1, TexCoord0).SetComponent(TexCoord1, X, value);

// technically able to do this in parallel by splitting on batch
// assign geometry pointer back to batch or to external array sized for multithreaded writing (cacheline size)

// ill still want uiforia uniforms setup, unless i decide to send a texture in as extra data instead 
// vertex sample isn't so bad but might be limiting

// def don't want to send extra vertex channels
// uniform buffer is fine but how do I set it up?

// how do i handle clip data in general?
// need a rect, texture, and uvs
// easy way is to break batch when clipper changes
// when rendering a uiforia batch
// go through and collect all the draw infos
// get their clip rects, get their clip uvs
// select material based on batch size
// set the uniform buffer 

// when not using a uiforia compatible shader
// send clip data via uniforms

// so im special casing my shader(s)
// which is probably fine if it has the required feature sets
// can make some kind of api to get 

// border colors? -> maybe do-able cpu side
// clip rect -> pixel align is probably ok
// clip uvs -> maybe 4 halfs are good enough when texture size is <= 2048

// dpi scale -> global uniform, nbd
// opacity -> gotta go somewhere
// size of bounds -> only need for sdf
// shape type -> only need for sdf

// assuming we have clip arrays, non sdf uses  
// texcoord1 = { opacity, pack(clipUVIndex, clipRectIndex), shapeType, packedColors }

// position vec3
// texcoord0 uv.xy, opacity, shape type?
// color rgba -> full if color32
// texcoord1 pack(clipUVTopLeft) pack(clipUVbottomRight), pack(clipRectTopLeft, packclipRectBottomRight)

// alternative is to reduce batching effectiveness and break batch on clipper also
// or limit to 8 different clippers at a time? probably a decent trade off
// so each draw call sends a uniform buffer of 8 * 2 * 4 floats 
// so buffer isn't 1 per object, objects in batch need to match their data to whats in the buffer
// if data doesn't fit, cannot join the batch, must be split.

// per object data isn't stored in uniform buffer anymore, just per clip data 

// uniform buffer 
// float4 clipRect
// float4 clipUVs
// float4 maskUVs? maybe per object

// differnt shader encodes different data
// sdf requires just 4 vertices per object, easy to encode extra per-vertex data there

// text can use more global uniforms