namespace UIForia.Graphics {

    public enum RenderCommandType {

        ElementBatch,
        ShapeEffectBatch,
        Mesh,
        MeshBatch,
        SDFTextBatch,
        SDFTextEffectBatch,

        CreateRenderTarget,
        PushRenderTexture,
        ClearRenderTarget,
        MaskAtlasBatch,

        UpdateClipRectBuffer,

        SetClipRectBuffer,

        Callback,

        SetTextDataBuffer,

        SetShapeDatabuffer,

        SetGradientDataBuffer,

        ShadowBatch

    }

}