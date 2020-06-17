namespace UIForia.Rendering {

    internal enum RenderOperationType {

        DrawBatch,
        PushRenderTexture,
        ClearRenderTextureRegion,
        BlitRenderTexture,
        SetScissorRect,
        SetCameraViewMatrix,
        SetCameraProjectionMatrix,

        PopRenderTexture

    }

}