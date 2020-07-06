using UIForia.Systems;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace UIForia.Graphics {

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

            Batch* batchArray = batchList.GetArrayPointer();
            int* batchMemberArray = batchMemberList.GetArrayPointer();
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();
            for (int batchIdx = start; batchIdx < end; batchIdx++) {

                ref Batch batch = ref batchArray[batchIdx];
              //  Profiler.BeginSample("Count data");

                // todo -- if transforming vertices, check if the matrix has rotation / scale before blinding doing big matrix operations

                int batchStart = batch.memberIdRange.start;
                int batchEnd = batch.memberIdRange.end;

                int vertexCount = 0;
                int triangleCount = 0;

                for (int i = batchStart; i < batchEnd; i++) {

                    ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];

                    vertexCount += drawInfo.geometryInfo->vertexCount;
                    triangleCount += drawInfo.geometryInfo->triangleCount;

                }

             //   Profiler.EndSample();

                int* trianglePtr = geometryBuffer.data.Allocate<int>(triangleCount);
                GetVertexChannels(ref vertexChannelList, batch.vertexLayout, ref geometryBuffer, vertexCount);

                VertexChannelDesc* vertexChannelDesc = geometryBuffer.data.Allocate(vertexChannelList.GetArrayPointer(), vertexChannelList.size);

                batch.vertexChannelCount = vertexChannelList.size;
                batch.geometry = vertexChannelDesc;
                batch.triangles = trianglePtr;
                batch.triangleCount = triangleCount;
                batch.vertexCount = vertexCount;

                // todo -- maybe i should tell drawInfo its vertex/triangle offset in batch
               // Profiler.BeginSample("Write geometry data");

               int cpyCount = 0;
                for (int vcIdx = 0; vcIdx < vertexChannelList.size; vcIdx++) {

                    ref VertexChannelDesc channelDesc = ref vertexChannelDesc[vcIdx];
                    int channelSize = (int) channelDesc.format;
                    byte* geometry = (byte*) channelDesc.ptr;

                    for (int i = batchStart; i < batchEnd; i++) {
                        cpyCount++;
                        ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];
                        int drawVertexCount = drawInfo.geometryInfo->vertexCount;
                        byte* dataPtr = (byte*) drawInfo.GetChannel(channelDesc.channel);
                        UnsafeUtility.MemCpy(geometry, dataPtr, channelSize * drawVertexCount);
                        geometry += (channelSize * drawVertexCount);
                    }

                }

                // Profiler.EndSample();
                
                int vOffset = 0;
               // Profiler.BeginSample("Make triangles");
                for (int i = batchStart; i < batchEnd; i++) {
                    ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];
                    int* triangles = drawInfo.GetTriangles();
                    int drawTriangleCount = drawInfo.geometryInfo->triangleCount;
                    for (int t = 0; t < drawTriangleCount; t++) {
                        trianglePtr[t] = triangles[t] + vOffset;
                    }
                
                    vOffset += drawInfo.geometryInfo->vertexCount;
                    trianglePtr += drawTriangleCount;
                }
                
                // Profiler.EndSample();
                // Profiler.BeginSample("World Transform");
                if (batchEnd - batchStart > 1) {
                    float3* positions = (float3*) vertexChannelDesc[0].ptr;
                    for (int i = batchStart; i < batchEnd; i++) {
                        ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];
                        int count = drawInfo.geometryInfo->vertexCount;
                
                        float4x4 matrix = *drawInfo.matrix;
                
                        // todo -- this obviously wont apply transformations
                        // todo -- dont pay for 3d transform when we only need a translation offset, check if matrix is translation only
                        for (int j = 0; j < count; j++) {
                            positions[j].x += matrix.c3.x; // = math.transform(matrix, positions[j]);
                            positions[j].y += matrix.c3.y; // = math.transform(matrix, positions[j]);
                        }
                
                        positions += count;
                
                    }
                }
                
                // Profiler.EndSample();

                // // if(batch.batchType == BatchType.Shape && batch.HasUIForiaMaterial()) {
                // if (batch.materialId.index == MaterialId.UIForiaSDFText.index) {
                //
                //     for (int i = batchStart; i < batchEnd; i++) {
                //         ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];
                //         int count = drawInfo.geometryInfo->vertexCount;
                //         TextInfo* textInfo = (TextInfo*) drawInfo.shapeData;
                //         // drawInfo.geometryInfo->GetOrCreateChannel( VertexChannel.TextureCoord1, VertexChannelFormat.Float4);
                //         // character scale
                //         // face/outline uv
                //         // underlay / glow info
                //         // etc?
                //         // text can result in multiple draw calls 
                //         // id like textinfo to be unmanaged where possible, can copy into burst internally but should be user accessable without disposing
                //
                //     }
                //
                // }
                // else if (batch.materialId.index == MaterialId.UIForiaShape.index) { }
                //
                // for (int i = batchStart; i < batchEnd; i++) {
                //     ref DrawInfo drawInfo = ref drawInfoArray[batchMemberArray[i]];
                //
                //     // drawInfo.overflowBounds
                //     // do i get index of? that sucks
                // }
                //
                // // }

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

}