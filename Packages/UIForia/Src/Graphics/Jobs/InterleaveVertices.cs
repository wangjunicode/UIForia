using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Graphics;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    [BurstCompile]
    internal unsafe struct InterleaveVertices : IJob, IVertigoParallel {

        public DataList<DrawInfo>.Shared drawList;
        public PerThread<GeometryBuffer> perThread_GeometryBuffer;
        public ParallelParams parallel { get; set; }
        [NativeSetThreadIndex] public int threadIndex;

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }
        
        public void Execute() {
            Run(0, drawList.size);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TextVertex {

            public float3 position;
            public float4 texCoord0;
            public float4 texCoord1;
            public Color32 color;

        }
        
        private void Run(int start, int end) {

            PagedByteAllocator byteAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(128), Allocator.Temp, Allocator.TempJob);
            List_VertexChannelDesc layoutList = new List_VertexChannelDesc(16, Allocator.Temp);

            DrawInfo* drawInfoArray = drawList.GetArrayPointer();
            ref GeometryBuffer geometryBuffer = ref perThread_GeometryBuffer.GetForThread(threadIndex);

            for (int i = start; i < end; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];
                ref GeometryInfo info = ref TypedUnsafe.AsRef(drawInfo.geometryInfo);

                int vertexCount = info.vertexCount;
                
                // if ((drawInfo.type & DrawType.Shape) != 0) {
                //     
                //     TextVertex* textPtr = geometryBuffer.data.Allocate<TextVertex>(vertexCount);
                //     float4* texCoord1 = (float4*) info.texCoord1;
                //     for (int v = 0; v < vertexCount; v++) {
                //         ref TextVertex vertex = ref textPtr[v];
                //         vertex.position = info.positions[v];
                //         vertex.texCoord0 = info.texCoord0[v];
                //         vertex.texCoord1 = texCoord1[v];
                //         vertex.color = info.colors[v];
                //     }
                //     info.interleaved = textPtr;
                //
                //     continue;
                // }
                
                int vertexSize = GetChannelInfo(ref layoutList, ref info);

                byte* ptr = geometryBuffer.data.Allocate<byte>(vertexSize * vertexCount);
                byte* outputPtr = ptr;
                info.interleaved = ptr;
                
                VertexChannelDesc* layoutArray = layoutList.array;
                int layoutListSize = layoutList.size;
                
                for (int v = 0; v < vertexCount; v++) {
                    
                    for (int j = 0; j < layoutListSize; j++) {

                        VertexChannelFormat format = layoutArray[j].format;
                        switch (format) {
                            default:
                            case VertexChannelFormat.Off:
                                break;

                            case VertexChannelFormat.Float1: {
                                int* src = ((int*) layoutArray[j].ptr) + v;
                                int* dst = (int*) outputPtr;
                                *dst = *src;
                                outputPtr += 4;
                                break;
                            }

                            case VertexChannelFormat.Float2: {
                                long* src = ((long*) layoutArray[j].ptr) + v;
                                long* dst = (long*) outputPtr;
                                *dst = *src;
                                outputPtr += 8;
                                break;
                            }

                            case VertexChannelFormat.Float3: {
                                int3* src = ((int3*) layoutArray[j].ptr) + v;
                                int3* dst = (int3*) outputPtr;
                                *dst = *src;
                                outputPtr += 12;
                                break;
                            }

                            case VertexChannelFormat.Float4: {
                                long* src = (long*)((int4*) layoutArray[j].ptr) + v;
                                long* dst = (long*) outputPtr;
                                *dst = *src;
                                src++;
                                dst++;
                                *dst = *src;
                                outputPtr += 16;
                                break;
                            }

                        }
                    }
                    
                }

            }

            layoutList.Dispose();
            byteAllocator.Dispose();
        }

        private static int GetChannelInfo(ref List_VertexChannelDesc output, ref GeometryInfo info) {
            output.size = 0;

            int vertexSize = 12;
            // position is always index 0
            VertexLayout layout = info.vertexLayout;

            output.array[output.size++] = new VertexChannelDesc() {
                channel = VertexChannel.Position,
                format = VertexChannelFormat.Float3,
                ptr = info.positions
            };

            if (layout.color != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.color;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.Color,
                    format = format,
                    ptr = info.colors
                };
            }

            if (layout.normal != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.normal;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.Normal,
                    format = format,
                    ptr = info.normal
                };
                vertexSize += (int) format;
            }

            if (layout.texCoord0 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord0;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord0,
                    format = format,
                    ptr = info.texCoord0
                };
                vertexSize += (int) format;
            }

            if (layout.texCoord1 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord1;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord1,
                    format = format,
                    ptr = info.texCoord1
                };
            }

            if (layout.texCoord2 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord2;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord2,
                    format = format,
                    ptr = info.texCoord2,
                };
            }

            if (layout.texCoord3 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord3;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord3,
                    format = format,
                    ptr = info.texCoord3,
                };
            }

            if (layout.texCoord4 != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.texCoord4;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.TextureCoord4,
                    format = format,
                    ptr = info.texCoord4,
                };
            }

            if (layout.tangent != VertexChannelFormat.Off) {
                VertexChannelFormat format = layout.tangent;
                vertexSize += (int) format;
                output.array[output.size++] = new VertexChannelDesc() {
                    channel = VertexChannel.Tangent,
                    format = format,
                    ptr = info.tangent
                };
            }

            // if (layout.texCoord5 != VertexChannelFormat.Off) {
            //     VertexChannelFormat format = layout.texCoord5;
            //     output[output.size++] = new VertexChannelDesc() {
            //         channel = VertexChannel.TextureCoord5,
            //         format = format,
            //         ptr = info.texCoord5,
            //     };
            // }
            //
            // if (layout.texCoord6 != VertexChannelFormat.Off) {
            //     VertexChannelFormat format = layout.texCoord6;
            //     output[output.size++] = new VertexChannelDesc() {
            //         channel = VertexChannel.TextureCoord6,
            //         format = format,
            //         ptr = info.texCoord6,
            //     };
            // }
            //
            // if (layout.texCoord7 != VertexChannelFormat.Off) {
            //     VertexChannelFormat format = layout.texCoord7;
            //     output[output.size++] = new VertexChannelDesc() {
            //         channel = VertexChannel.TextureCoord7,
            //         format = format,
            //         ptr = buffer.data.Allocate<byte>((int) format * vertexCount)
            //     };
            // }
            return vertexSize;
        }

      
    }

}