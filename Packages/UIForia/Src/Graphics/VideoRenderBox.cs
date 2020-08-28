using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Graphics {


    public class VideoRenderBox : Rendering.StandardRenderBox2 {

        private RenderTexture renderTexture;

        public override void OnSizeChanged(Size size) {
            base.OnSizeChanged(size);

            if (!ReferenceEquals(renderTexture, null)) {
                renderTexture.Release();
                Object.Destroy(renderTexture);
            }

            UIVideoElement videoElement = element as UIVideoElement;
            renderTexture = new RenderTexture((int) size.width, (int) size.height, 24);
            videoElement.videoPlayer.targetTexture = renderTexture;

        }

        public override void PaintBackground3(RenderContext3 ctx) {
            if (requireRendering) {
                ctx.SetBackgroundTexture(renderTexture);
                ctx.DrawElement(0, 0, size.width, size.height, drawDesc);
            }

            if (overflowHandling != 0) {

                // todo -- add an overflow offset style
                float clipX = 0;
                float clipY = 0;
                float clipWidth = float.MaxValue;
                float clipHeight = float.MaxValue;

                if ((overflowHandling & OverflowHandling.Horizontal) != 0) {
                    clipWidth = size.width;
                }

                if ((overflowHandling & OverflowHandling.Vertical) != 0) {
                    clipHeight = size.height;
                }

                ctx.PushClipRect(clipX, clipY, clipWidth, clipHeight);
            }

        }

        public override void OnDestroy() {
            renderTexture.Release();
            Object.Destroy(renderTexture);
        }

    }

}