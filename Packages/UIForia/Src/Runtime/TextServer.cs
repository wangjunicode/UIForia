using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Sequential)]
    internal struct AscentNormalizedFontMetrics {

        public float ascent;
        public float descent;
        public float lineGap;
        public float capHeight;
        public float xHeight;
        public float advanceSpaceWidth;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UnscaledGlyphMetric {

        public int codepoint;
        public int glyphIndex;
        public float leftBearing;
        public float topBearing;
        public float advanceX;
        public int flags;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FontLineVertex {

        public float2 pos;
        public float2 par;
        public float2 limits;
        public float scale;
        public float lineWidth;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FontFillVertex {

        public float2 pos;
        public float2 par;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphRequest {

        public uint codepoint;
        public ushort fontId;
        public float pixelSize;
        public float sdfSize;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphResponse : IComparable<GlyphResponse> {

        public uint glyphIndex;
        public uint lineVertexStart;
        public uint lineVertexCount;
        public uint fillVertexStart;
        public uint fillVertexCount;
        public float rectWidth;
        public float rectHeight;

        public int CompareTo(GlyphResponse other) {
            return (int) other.rectHeight - (int) rectHeight;
        }

    }

    [Flags]
    internal enum ShapeLogSettings {

        None = 0,
        LogCreateContext = 1 << 0,
        LogDestroyContext = 1 << 1,
        LogCreateFont = 1 << 2,
        LogShapeText = 1 << 3,
        LogInvalidFont = 1 << 4,
        All = LogCreateContext | LogDestroyContext | LogCreateFont | LogShapeText | LogInvalidFont

    }

    internal unsafe struct ShapeContext : IDisposable {

        [NativeDisableUnsafePtrRestriction] internal IntPtr ptr;

        public ShapeContext(IntPtr ptr) {
            this.ptr = ptr;
        }

        public ShapeContext(ShapeLogSettings logSettings, TextServer.DebugCallback debugCallback = null) {
            ptr = TextServer.CreateShapingContext(logSettings, debugCallback);
        }

        public ushort CreateFontFromFile(string fileName, int faceIndex = 0) {
            return default;
            // return TextServer.CreateFontFromFile(ptr, fileName, faceIndex);
        }

        public ushort CreateFontFromBytes(byte* bytes, int byteCount, int faceIndex = 0) {
            return TextServer.CreateFontFromBytes(ptr, bytes, byteCount, faceIndex);
        }

        public ushort CreateFontFromBytes(byte[] bytes, int faceIndex = 0) {
            fixed (byte* b = bytes) {
                return TextServer.CreateFontFromBytes(ptr, b, bytes.LongLength, faceIndex);
            }
        }

        public int Shape(TextShapeRequestInfo request, TextShapeBuffers buffers) {
            return TextServer.ShapeText(ptr, ref request, ref buffers);
        }

        public void Dispose() {
            if (ptr != IntPtr.Zero) {
                TextServer.DestroyShapingContext(ptr);
                ptr = IntPtr.Zero;
            }
        }

        public static implicit operator IntPtr(ShapeContext ctx) {
            return ctx.ptr;
        }

    }

    internal static unsafe class TextServer {

        public delegate void DebugCallback(string message);

#if UNITY_IOS && !UNITY_EDITOR
        public const string kLibraryName = "__Internal";
#else
        public const string kLibraryName = "UIForiaTextEngine";
#endif

        [DllImport(kLibraryName)]
        public static extern IntPtr CreateRenderContextIL2CPP();

        [DllImport(kLibraryName)]
        public static extern IntPtr CreateRenderContext(DebugCallback callback);
        
        [DllImport(kLibraryName)]
        public static extern ushort RegisterFontFile(IntPtr ctx, string fontPath, AscentNormalizedFontMetrics* metrics);

        [DllImport(kLibraryName)]
        public static extern void DestroyRenderContext(IntPtr ctx);

        [DllImport(kLibraryName)]
        public static extern void CopyGlyphBuffers(IntPtr ctx, FontLineVertex* lineBuffer, FontFillVertex* fillBuffer);

        [DllImport(kLibraryName)]
        public static extern int GetGlyphCount(IntPtr ctx, ushort fontId);

        [DllImport(kLibraryName)]
        public static extern int GetGlyphCodepoints(IntPtr ctx, ushort fontId, uint* codepoints);

        [DllImport(kLibraryName)]
        public static extern void GenerateRenderStructures(IntPtr ctx, GlyphRequest* requests, int count, GlyphResponse* response, int* outLineVertexCount, int* outFillVertexCount);

        [DllImport(kLibraryName)]
        public static extern IntPtr CreateShapingContextIL2CPP();

        [DllImport(kLibraryName)]
        public static extern IntPtr CreateShapingContext(ShapeLogSettings logSettings, DebugCallback callback);

        [DllImport(kLibraryName)]
        public static extern void DestroyShapingContext(IntPtr context);

        [DllImport(kLibraryName)] 
        public static extern int ShapeText(IntPtr ctx, ref TextShapeRequestInfo request, ref TextShapeBuffers buffers);

        [DllImport(kLibraryName)]
        public static extern ushort CreateFontFromBytes(IntPtr ctx, byte* file, long size, int faceIndex);

    }

}