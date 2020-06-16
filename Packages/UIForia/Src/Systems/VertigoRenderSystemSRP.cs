using UIForia.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Src.Systems {

    internal class VertigoRenderPass : ScriptableRenderPass
    {
        private static readonly string _ProfilerTag = "UIForia Main Command Buffer";

        private RenderContext _renderContext;

        public VertigoRenderPass(RenderContext renderContext)
        {
            renderPassEvent = RenderPassEvent.AfterRendering;
            _renderContext = renderContext;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_ProfilerTag);
            _renderContext.Render(renderingData.cameraData.camera, cmd);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public class VertigoRenderSystemSRP : ScriptableRendererFeature {

        public override void Create()
        {
            return;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (var application in UIForia.Application.Applications)
            {
                if (application.Camera == renderingData.cameraData.camera)
                {
                    renderer.EnqueuePass(new VertigoRenderPass(application.renderSystem.GetRenderContext()));
                }
            }
        }
    }
}