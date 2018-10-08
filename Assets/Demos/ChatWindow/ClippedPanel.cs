using System;
using Rendering;
using Src;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

[Template(TemplateType.String, @"
<UITemplate>
    <Contents>
        <Children/>
    </Contents>
</UITemplate>
")]
public class ClippedPanel : UIElement, IDrawable {

    private static readonly VertexHelper s_VertexHelper = new VertexHelper();

    protected Mesh mesh;
    protected bool isMeshDirty;
    protected bool isMaterialDirty;
    public float clipSize = 10f;

    public ClippedCorner clippedCorner = ClippedCorner.None;

    private Material material;
    
    public override void OnReady() {
        isMeshDirty = true;
        isMaterialDirty = true;
        material = new Material(Graphic.defaultGraphicMaterial);
    }

    public ClippedCorner prev;
    public override void OnUpdate() {
        if (clippedCorner != prev) {
            prev = clippedCorner;
            OnAllocatedSizeChanged();
        }
    }
    
    public void OnAllocatedSizeChanged() {
        isMeshDirty = true;
        onMeshDirty?.Invoke(this);
    }

    public void OnStylePropertyChanged(StyleProperty property) {
        
        if (property.propertyId == StylePropertyId.BackgroundColor) {
            isMaterialDirty = true;
        }
        
    }

    public Mesh GetMesh() {
        /*
         * Generate a mesh with optionally clipped corners
         * Vertex index layout:
         * 0    1            13    14
         *
         * 3    2            12    15
         *
         *
         * 4    5            8     11
         *
         * 6    7            9     10
         */
        if (!isMeshDirty) return mesh;

        if (mesh == null) mesh = new Mesh();

        Color32 color32 = style.computedStyle.BackgroundColor;
        Size size = layoutResult.allocatedSize;

        if (clippedCorner == ClippedCorner.None || clipSize <= 0f) {
            s_VertexHelper.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 0f));
            s_VertexHelper.AddVert(new Vector3(0, size.height), color32, new Vector2(0f, 1f));
            s_VertexHelper.AddVert(new Vector3(size.width, size.height), color32, new Vector2(1f, 1f));
            s_VertexHelper.AddVert(new Vector3(size.width, 0), color32, new Vector2(1f, 0f));

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);
            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

        clipSize = Mathf.Min(clipSize, size.height);

        float width = size.width;
        float height = size.height;

        Vector3 v0 = new Vector3(0, 0);
        Vector3 v1 = new Vector3(clipSize, 0);
        Vector3 v2 = new Vector3(clipSize, clipSize);
        Vector3 v3 = new Vector3(0, clipSize);

        Vector3 v4 = new Vector3(0, height - clipSize);
        Vector3 v5 = new Vector3(clipSize, height - clipSize);
        Vector3 v6 = new Vector3(0, height);
        Vector3 v7 = new Vector3(clipSize, height);

        Vector3 v8 = new Vector3(width - clipSize, height - clipSize);
        Vector3 v9 = new Vector3(width - clipSize, height);
        Vector3 v10 = new Vector3(width, height);
        Vector3 v11 = new Vector3(width, height - clipSize);

        Vector3 v12 = new Vector3(width - clipSize, clipSize);
        Vector3 v13 = new Vector3(width - clipSize, 0);
        Vector3 v14 = new Vector3(width, 0);
        Vector3 v15 = new Vector3(width, clipSize);

        v0.y =  -v0.y;
        v1.y =  -v1.y;
        v2.y =  -v2.y;
        v3.y =  -v3.y;

        v4.y =  -v4.y;
        v5.y =  -v5.y;
        v6.y =  -v6.y;
        v7.y =  -v7.y;

        v8.y =  -v8.y;
        v9.y =  -v9.y;
        v10.y =  -v10.y;
        v11.y =  -v11.y;

        v12.y =  -v12.y;
        v13.y =  -v13.y;
        v14.y =  -v14.y;
        v15.y =  -v15.y;

        // todo -- compute UVs
        s_VertexHelper.AddVert(v0, color32, new Vector2());
        s_VertexHelper.AddVert(v1, color32, new Vector2());
        s_VertexHelper.AddVert(v2, color32, new Vector2());
        s_VertexHelper.AddVert(v3, color32, new Vector2());
        s_VertexHelper.AddVert(v4, color32, new Vector2());
        s_VertexHelper.AddVert(v5, color32, new Vector2());
        s_VertexHelper.AddVert(v6, color32, new Vector2());
        s_VertexHelper.AddVert(v7, color32, new Vector2());
        s_VertexHelper.AddVert(v8, color32, new Vector2());
        s_VertexHelper.AddVert(v9, color32, new Vector2());
        s_VertexHelper.AddVert(v10, color32, new Vector2());
        s_VertexHelper.AddVert(v11, color32, new Vector2());
        s_VertexHelper.AddVert(v12, color32, new Vector2());
        s_VertexHelper.AddVert(v13, color32, new Vector2());
        s_VertexHelper.AddVert(v14, color32, new Vector2());
        s_VertexHelper.AddVert(v15, color32, new Vector2());

        // top left
        s_VertexHelper.AddTriangle(1, 2, 3);
        if ((clippedCorner & ClippedCorner.TopLeft) == 0) {
            s_VertexHelper.AddTriangle(3, 0, 1);
        }

        // left center
        s_VertexHelper.AddTriangle(3, 2, 5);
        s_VertexHelper.AddTriangle(5, 4, 3);
        // bottom left
        s_VertexHelper.AddTriangle(4, 5, 7);
        if ((clippedCorner & ClippedCorner.BottomLeft) == 0) {
            s_VertexHelper.AddTriangle(7, 6, 4);
        }

        // bottom center
        s_VertexHelper.AddTriangle(8, 9, 7);
        s_VertexHelper.AddTriangle(7, 5, 8);

        // bottom right
        if ((clippedCorner & ClippedCorner.BottomRight) == 0) {
            s_VertexHelper.AddTriangle(11, 10, 9);
        }

        s_VertexHelper.AddTriangle(9, 8, 11);

        // center right
        s_VertexHelper.AddTriangle(15, 11, 8);
        s_VertexHelper.AddTriangle(8, 12, 15);
        // top right
        if ((clippedCorner & ClippedCorner.TopRight) == 0) {
            s_VertexHelper.AddTriangle(13, 14, 15);
        }

        s_VertexHelper.AddTriangle(15, 12, 13);

        // top center
        s_VertexHelper.AddTriangle(13, 12, 2);
        s_VertexHelper.AddTriangle(2, 1, 13);
        // center center
        s_VertexHelper.AddTriangle(12, 8, 5);
        s_VertexHelper.AddTriangle(5, 2, 12);

        s_VertexHelper.FillMesh(mesh);
        s_VertexHelper.Clear();
        return mesh;
    }


    public Material GetMaterial() {
        return Graphic.defaultGraphicMaterial;
    }

    public event Action<IDrawable> onMeshDirty;
    public event Action<IDrawable> onMaterialDirty;

    public int Id => id;
    public bool IsMaterialDirty => isMaterialDirty;
    public bool IsGeometryDirty => isMeshDirty;

    public Texture GetMainTexture() {
        return Texture2D.whiteTexture;
    }
    
    [Flags]
    public enum ClippedCorner {

        None = 0,
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BottomLeft = 1 << 2,
        BottomRight = 1 << 3

    }

}