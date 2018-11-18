using System;
using UIForia;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.UI;

public sealed class ClippedPanel : UIContainerElement, IMeshProvider {

    private static readonly VertexHelper s_VertexHelper = new VertexHelper();

    private Mesh mesh;
    private bool isMeshDirty;
    public float clipSize = 10f;

    public ClippedCorner clippedCorner = ClippedCorner.None;

    public override void OnReady() {
        isMeshDirty = true;
    }

    [OnPropertyChanged(nameof(clippedCorner))]
    public void OnClipChanged() {
        isMeshDirty = true;
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

        if (!isMeshDirty && !layoutResult.ActualSizeChanged) {
            return mesh;
        }

        if (mesh == null) {
            mesh = new Mesh();
        }
        
        isMeshDirty = false;

        Color32 color32 = style.BackgroundColor;
        Size size = layoutResult.actualSize;
        if (clippedCorner == ClippedCorner.None || clipSize <= 0f) {
            mesh = MeshUtil.ResizeStandardUIMesh(mesh, size);
            return mesh;
        }

        clipSize = Mathf.Min(clipSize, size.height);

        float width = size.width + 1;
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

        float clipOverWidth = clipSize / width;
        float clipOverHeight = clipSize / height;
        s_VertexHelper.AddVert(v0, color32, new Vector2(0, 0));
        s_VertexHelper.AddVert(v1, color32, new Vector2(clipOverWidth, 0));
        s_VertexHelper.AddVert(v2, color32, new Vector2(clipOverWidth, clipOverHeight));
        s_VertexHelper.AddVert(v3, color32, new Vector2(0, clipOverHeight));
        s_VertexHelper.AddVert(v4, color32, new Vector2(0, 1 - clipOverHeight));
        s_VertexHelper.AddVert(v5, color32, new Vector2(clipOverWidth, 1 - clipOverHeight));
        s_VertexHelper.AddVert(v6, color32, new Vector2(0, 1));
        s_VertexHelper.AddVert(v7, color32, new Vector2(clipOverWidth, 1));
        s_VertexHelper.AddVert(v8, color32, new Vector2(1 - clipOverWidth, 1 - clipOverHeight));
        s_VertexHelper.AddVert(v9, color32, new Vector2(1 - clipOverWidth, 1));
        s_VertexHelper.AddVert(v10, color32, new Vector2(1, 1));
        s_VertexHelper.AddVert(v11, color32, new Vector2(1, 1 - clipOverHeight));
        s_VertexHelper.AddVert(v12, color32, new Vector2(1 - clipOverWidth, clipOverHeight));
        s_VertexHelper.AddVert(v13, color32, new Vector2(1 - clipOverWidth, 0));
        s_VertexHelper.AddVert(v14, color32, new Vector2(1, 0));
        s_VertexHelper.AddVert(v15, color32, new Vector2(1 , clipOverHeight));

        // top left
        s_VertexHelper.AddTriangle(3, 2, 1);
        if ((clippedCorner & ClippedCorner.TopLeft) == 0) {
            s_VertexHelper.AddTriangle(1, 0, 3);
        }

        // left center
        s_VertexHelper.AddTriangle(5, 2, 3);
        s_VertexHelper.AddTriangle(3, 4, 5);
        // bottom left
        s_VertexHelper.AddTriangle(7, 5, 4);
        if ((clippedCorner & ClippedCorner.BottomLeft) == 0) {
            s_VertexHelper.AddTriangle(4, 6, 7);
        }

        // bottom center
        s_VertexHelper.AddTriangle(7, 9, 8);
        s_VertexHelper.AddTriangle(8, 5, 7);

        // bottom right
        if ((clippedCorner & ClippedCorner.BottomRight) == 0) {
            s_VertexHelper.AddTriangle(9, 10, 11);
        }
        s_VertexHelper.AddTriangle(11, 8, 9);

        // center right
        s_VertexHelper.AddTriangle(8, 11, 15);
        s_VertexHelper.AddTriangle(15, 12, 8);
        
        // top right
        if ((clippedCorner & ClippedCorner.TopRight) == 0) {
            s_VertexHelper.AddTriangle(15, 14, 13);
        }
        s_VertexHelper.AddTriangle(13, 12, 15);

        // top center
        s_VertexHelper.AddTriangle(2, 12, 13);
        s_VertexHelper.AddTriangle(13, 1, 2);
        
        // center center
        s_VertexHelper.AddTriangle(5, 8, 12);
        s_VertexHelper.AddTriangle(12, 2, 5);

        s_VertexHelper.FillMesh(mesh);
        s_VertexHelper.Clear();
        return mesh;
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