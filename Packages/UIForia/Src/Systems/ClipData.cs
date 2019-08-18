using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class ClipData {

        internal bool isTransformed;
        internal bool isCulled;
        internal bool isDrawn;
        
        internal int zIndex;
        internal int textureChannel;
        internal int visibleBoxCount;
        
        internal Vector4 aabb;
        internal ClipData parent;
        internal StructList<Vector2> intersected;
        internal PolyRect worldBounds;
        internal RenderBox renderBox;
        internal RenderTexture clipTexture;
        internal Vector4 clipUVs;
        internal Path2D clipPath;

        internal SimpleRectPacker.PackedRect textureRegion;
        internal LightList<ClipData> dependents;
        internal Vector4 packedBoundsAndChannel;
        internal int regionDrawCount;

        internal ClipData() {
            intersected = new StructList<Vector2>();
            dependents = new LightList<ClipData>();
        }

        public void Clear() {
            parent = null;
            isDrawn = false;
            clipPath = null;
            isCulled = false;
            visibleBoxCount = 0;
            isTransformed = false;
            renderBox = null;
            intersected.size = 0;
            worldBounds = default;
            dependents.QuickClear();
        }

    }

}