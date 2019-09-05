using System;
using UIForia;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Vertigo;

[CustomPainter("Dissolve")]
public class DissolveEffect : StandardRenderBox {
    
    public Material material;
    private UIForiaGeometry effectGeometry;

    public DissolveEffect() {
        material = new Material(Shader.Find("UIForia/Dissolve"));
        effectGeometry = new UIForiaGeometry();
    }

    [EffectParameter] public Texture2D noise;
    [EffectParameter] public float factor;

    public void MapParameters() { }

    public override void OnInitialize() {
        material.SetTexture("_NoiseTex", Resources.Load<Texture>("UIDissolveNoise_Demo1"));
    }

    public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
        if (propertyList[0].propertyId == StylePropertyId.Painter) {
            // this.data = ((EffectData)propertyList[0].AsPainter).data;
        }
    }

    public Bounds GetLocalBounds() {
        Bounds bounds = default;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        Vector3[] points = geometry.positionList.array;
        int pointCount = geometry.positionList.size;

        for (int i = 0; i < pointCount; i++) {
            ref Vector3 point = ref points[i];
            if (point.x < minX) minX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.x > maxX) maxX = point.x;
            if (point.y > maxY) maxY = point.y;
        }

        Matrix4x4 mat = default;

        bounds.min = new Vector3(minX, minY, 0);
        bounds.max = new Vector3(maxX, maxX, 0);

        Vector3 center = bounds.center;


        return bounds;
    }

    public static Bounds TransformBounds(ref Matrix4x4 transform, ref Bounds localBounds) {
        Vector3 center = transform.MultiplyPoint3x4(localBounds.center);

        // transform the local extents' axes
        Vector3 extents = localBounds.extents;
        Vector3 axisX = transform.MultiplyVector(new Vector3(extents.x, 0, 0));
        Vector3 axisY = transform.MultiplyVector(new Vector3(0, extents.y, 0));
        Vector3 axisZ = transform.MultiplyVector(new Vector3(0, 0, extents.z));

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds {center = center, extents = extents};
    }

    public override void PaintBackground(RenderContext ctx) {
        effectGeometry.Clear();
        effectGeometry.Quad(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

        // size should probably be min of cliprect and descendent screenspace extents
        // todo -- geometry should not be just a quad & transformed by xform matrix. basically a world space render bound

        // this needs to be some bounds, usually the containing bounds for a mesh that can contain all children of the element
        ctx.PushRenderArea(new SizeInt(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height), Color.clear);

        base.PaintBackground(ctx);
    }

    public override void PaintForeground(RenderContext ctx) {
        RenderContext.RenderArea area = ctx.PopRenderArea();
        Texture mainTexture = ctx.GetTextureFromArea(area);

        material.SetTexture("_MainTex", mainTexture);
        material.SetTexture("_NoiseTex", Resources.Load<Texture>("UIDissolveNoise_Demo1"));
        material.SetVector("_RemapRect", new Vector4());
        
        // todo -- need a generic way to re-map texture coordinates
        // maybe in the shader?
        
        ctx.DrawGeometry(geometry, material);
    }

    // todo -- need to store original UVs in order to remap, or use vertex position to do it
    
    public static void MapUVToArea(RenderContext.RenderArea area, RangeInt vertexRange, Vector4[] array, bool zw = false) {
        float texWidth = area.renderTexture.width;
        float texHeight = area.renderTexture.height;

        float startX = area.renderArea.xMin / texWidth;
        float startY = area.renderArea.yMin / texHeight;
        float width = (area.renderArea.xMax - area.renderArea.xMin) / texWidth;
        float height = ((area.renderArea.yMax - area.renderArea.yMin) / texHeight);

        int start = vertexRange.start;
        int end = vertexRange.end;

        if (zw) {
            for (int i = start; i < end; i++) {
                float z = array[i].z;
                float w = 1 - array[i].w;

                array[i].z = (startX + ((z * width)));
                array[i].w = 1 - (startY + (w * height));
            }
        }
        else {
            for (int i = start; i < end; i++) {
                float x = array[i].x;
                float y = 1 - array[i].y;

                array[i].x = (startX + ((x * width)));
                array[i].y = 1 - (startY + (y * height));
            }
        }
    }

    public static void RemapUVToArea(RenderContext.RenderArea area, RangeInt vertexRange, Vector4[] source, Vector4[] array, bool zw = false) {
        float texWidth = area.renderTexture.width;
        float texHeight = area.renderTexture.height;

        float startX = area.renderArea.xMin / texWidth;
        float startY = area.renderArea.yMin / texHeight;
        float width = (area.renderArea.xMax - area.renderArea.xMin) / texWidth;
        float height = ((area.renderArea.yMax - area.renderArea.yMin) / texHeight);

        int start = vertexRange.start;
        int end = vertexRange.end;

        if (zw) {
            for (int i = start; i < end; i++) {
                float z = array[i].z;
                float w = 1 - array[i].w;

                array[i].z = (startX + ((z * width)));
                array[i].w = 1 - (startY + (w * height));
            }
        }
        else {
            for (int i = start; i < end; i++) {
                float x = array[i].x;
                float y = 1 - array[i].y;

                array[i].x = (startX + ((x * width)));
                array[i].y = 1 - (startY + (y * height));
            }
        }
    }

    public static void RemapUVToArea(RenderContext.RenderArea area, RangeInt vertexRange, Vector2[] array) {
        float texWidth = area.renderTexture.width;
        float texHeight = area.renderTexture.height;

        float startX = area.renderArea.xMin / texWidth;
        float startY = area.renderArea.yMin / texHeight;
        float width = (area.renderArea.xMax - area.renderArea.xMin) / texWidth;
        float height = ((area.renderArea.yMax - area.renderArea.yMin) / texHeight);

        int start = vertexRange.start;
        int end = vertexRange.end;

        for (int i = start; i < end; i++) {
            float x = array[i].x;
            float y = 1 - array[i].y;

            array[i].x = (startX + ((x * width)));
            array[i].y = 1 - (startY + (y * height));
        }
    }

}