using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    internal struct RenderOperation {

        public int batchIndex;
        public RenderOperationType operationType;
        public RenderTexture renderTexture;
        public SimpleRectPacker.PackedRect rect;
        public Color color;

        public RenderOperation(int batchIndex) {
            this.batchIndex = batchIndex;
            this.operationType = RenderOperationType.DrawBatch;

            this.rect = default;
            this.renderTexture = null;
            this.color = default;
        }

    }

}