using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UIForiaFXAA : MonoBehaviour {

    private Material mat;
    
    private static readonly int s_RcpFrame = Shader.PropertyToID("_rcpFrame");
    private static readonly int s_RcpFrameOpt = Shader.PropertyToID("_rcpFrameOpt");

    void Start() {
        mat = new Material(Shader.Find("Hidden/FXAA3"));
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        float rcpWidth = 1.0f / Screen.width;
        float rcpHeight = 1.0f / Screen.height;

        mat.SetVector(s_RcpFrame, new Vector4(rcpWidth, rcpHeight, 0, 0));
        mat.SetVector(s_RcpFrameOpt, new Vector4(rcpWidth * 2, rcpHeight * 2, rcpWidth * 0.5f, rcpHeight * 0.5f));

        Graphics.Blit(source, destination, mat);
    }

}