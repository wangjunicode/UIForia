using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class ClipData {

        public bool isRect;
        public bool isCulled;
        public Vector4 screenSpaceBounds;
        public ClipData parent;
        public int visibleBoxCount;
        public StructList<Vector2> intersected;
        public PolyRect worldBounds;
        public RenderBox renderBox;
        public RenderTexture clipTexture;
        public Vector4 clipUVs;
        public ClipShape clipShape;
        public int zIndex;
        public int lastFrameId; // todo -- if user wants to make a clipper thats fine
        public int textureChannel;
        public SimpleRectPacker.PackedRect textureRegion;
        public LightList<ClipData> dependents;

        public ClipData() {
            intersected = new StructList<Vector2>();
            dependents = new LightList<ClipData>();
        }

        public void Clear() {
            parent = null;
            clipShape = null;
            isCulled = false;
            visibleBoxCount = 0;
            isRect = false;
            renderBox = null;
            intersected.size = 0;
            worldBounds = default;
            dependents.QuickClear();
        }

    }

}