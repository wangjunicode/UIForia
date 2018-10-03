using Src.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class SDFText : MaskableGraphic {

        public TMP_FontAsset m_fontAsset;
        public Color m_fontColor;
        private Mesh mesh;
        public TextMeshProUGUI tmp;
        
        public override Material materialForRendering => m_fontAsset.material;

        protected override void Start() {
            m_fontAsset = Resources.Load<TMP_FontAsset>("Gotham-Medium SDF");
        }

        private void Update() {
            GenerateWords();
        }

        protected void GenerateWords() {
            string input = "Input Goes Here";
            TextInfo textInfo = TextUtil.ProcessText(new TextSpan(input));
            TextThing thing = new TextThing();
            
            SpanInfo spanInfo = new SpanInfo();
            spanInfo.charCount = textInfo.charInfos.Length;
            spanInfo.fontSize = 24;
            spanInfo.wordCount = textInfo.wordCount;
            spanInfo.font = m_fontAsset;
//            thing.ApplyGlyphAdjustments(spanInfo, textInfo.wordInfos, textInfo.charInfos);
//            thing.RunLayout(spanInfo, textInfo);
//            Mesh mesh = thing.GenerateMesh(textInfo);
//            Mesh tmpMesh = tmp.mesh;
//            canvasRenderer.SetMesh(mesh);
//            canvasRenderer.SetMaterial(m_fontAsset.material, m_fontAsset.material.GetTexture(ShaderUtilities.ID_MainTex));
        }

//            TMP_TextInfo textInfo;
//            TMP_Glyph glyph;
//            TMP_FontUtilities.SearchForGlyph(m_fontAsset, (int) 'm', out glyph);
//
//            Vector3[] vertices = new Vector3[8];
//            Vector2[] uv = new Vector2[8];
//            Vector2[] uvtwo = new Vector2[8];
//
//            float padding = 0f;//0.5f;
//            float style_padding = 0;
//            float m_fontSize = 36f;
//            float m_fontScale;
//            float m_charWidthAdjDelta = 0;
//            float m_lineOffset = 0;
//            float m_baselineOffset = 0;
//            float m_fontScaleMultiplier = 1;
//            ;
//            TMP_TextElement m_cached_TextElement = glyph;
//
////            HandleKerning(m_fontAsset, '\0'.Equals( ))
//
//            GlyphValueRecord glyphAdjustments = new GlyphValueRecord();
//            float m_xAdvance = 0f;
//            float baseScale = m_fontScale = (m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale);
//            float currentElementScale = m_fontScale * m_fontScaleMultiplier * m_cached_TextElement.scale;
//            float fontBaseLineOffset = m_fontAsset.fontInfo.Baseline * m_fontScale * m_fontScaleMultiplier * m_fontAsset.fontInfo.Scale;
//            // m_fontScale = m_currentFontSize * smallCapsMultiplier / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale;
//
//            FaceInfo faceInfo = m_fontAsset.fontInfo;
//
//            Vector2 uv0;
//            uv0.x = (m_cached_TextElement.x - padding - style_padding) / faceInfo.AtlasWidth;
//            uv0.y = 1 - (m_cached_TextElement.y + padding + style_padding + m_cached_TextElement.height) / faceInfo.AtlasHeight;
//
//            Vector2 uv1;
//            uv1.x = uv0.x;
//            uv1.y = 1 - (m_cached_TextElement.y - padding - style_padding) / faceInfo.AtlasHeight;
//
//            Vector2 uv2;
//            uv2.x = (m_cached_TextElement.x + padding + style_padding + m_cached_TextElement.width) / faceInfo.AtlasWidth;
//            uv2.y = uv1.y;
//
//            Vector2 uv3;
//            uv3.x = uv2.x;
//            uv3.y = uv0.y;
//
//            Vector3 top_left;
//            top_left.x = m_xAdvance + ((m_cached_TextElement.xOffset - padding - style_padding + glyphAdjustments.xPlacement) * currentElementScale * (1 - m_charWidthAdjDelta));
//            top_left.y = fontBaseLineOffset + (m_cached_TextElement.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale - m_lineOffset + m_baselineOffset;
//            top_left.z = 0;
//
//            Vector3 bottom_left;
//            bottom_left.x = top_left.x;
//            bottom_left.y = top_left.y - ((m_cached_TextElement.height + padding * 2) * currentElementScale);
//            bottom_left.z = 0;
//
//            Vector3 top_right;
//            top_right.x = bottom_left.x + ((m_cached_TextElement.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta));
//            top_right.y = top_left.y;
//            top_right.z = 0;
//
//            Vector3 bottom_right;
//            bottom_right.x = top_right.x;
//            bottom_right.y = bottom_left.y;
//            bottom_right.z = 0;
//
//            // add anchor offset + justification offset to each vertex positon
//
//            
////            m_textInfo.meshInfo[materialIndex].vertices[0 + index_X4] = characterInfoArray[i].vertex_BL.position;
////            m_textInfo.meshInfo[materialIndex].vertices[1 + index_X4] = characterInfoArray[i].vertex_TL.position;
////            m_textInfo.meshInfo[materialIndex].vertices[2 + index_X4] = characterInfoArray[i].vertex_TR.position;
////            m_textInfo.meshInfo[materialIndex].vertices[3 + index_X4] = characterInfoArray[i].vertex_BR.position;
//            
//            vertices[0] = bottom_left; //new Vector3(-97.6f, -9.8f, 0.0f);
//            vertices[1] = top_left; //new Vector3(-97.6f, 10.1f, 0.0f);
//            vertices[2] = top_right;//new Vector3(-68.1f, 10.1f, 0.0f);
//            vertices[3] = bottom_right;//new Vector3(-68.1f, -9.8f, 0.0f);
//            vertices[4] = Vector3.zero;
//            vertices[5] = Vector3.zero;
//            vertices[6] = Vector3.zero;
//            vertices[7] = Vector3.zero;
//
//            uv[0] = uv0;// new Vector2(0.2f, 0.1f);
//            uv[1] = uv1; //new Vector2(0.2f, 0.2f);
//            uv[2] = uv2; //new Vector2(0.3f, 0.2f);
//            uv[3] = uv3; //new Vector2(0.3f, 0.1f);
//            uv[4] = Vector2.zero;
//            uv[5] = Vector2.zero;
//            uv[6] = Vector2.zero;
//            uv[7] = Vector2.zero;
//
//            uvtwo[0] = new Vector2(0.0f, 0.5f);
//            uvtwo[1] = new Vector2(511.0f, 0.5f);
//            uvtwo[2] = new Vector2(2093567.0f, 0.5f);
//            uvtwo[3] = new Vector2(2093056.0f, 0.5f);
//            uvtwo[4] = Vector2.zero;
//            uvtwo[5] = Vector2.zero;
//            uvtwo[6] = Vector2.zero;
//            uvtwo[7] = Vector2.zero;
//
//            mesh = new Mesh();
//            mesh.MarkDynamic();
//            mesh.vertices = vertices;
//            mesh.uv = uv;
//            mesh.uv2 = uvtwo;
//
//            Color32[] color32 = new Color32[8];
//            color32[0] = Color.white;
//            color32[1] = Color.white;
//            color32[2] = Color.white;
//            color32[3] = Color.white;
//            color32[4] = Color.white;
//            color32[5] = Color.white;
//            color32[6] = Color.white;
//            color32[7] = Color.white;
//
//            mesh.triangles = new[] {0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4};
//            mesh.normals = new[] {
//                Vector3.back,
//                Vector3.back,
//                Vector3.back,
//                Vector3.back,
//                Vector3.back,
//                Vector3.back,
//                Vector3.back,
//                Vector3.back
//            };
//            mesh.tangents = new Vector4[] {
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f),
//                new Vector4(-1f, 0.0f, 0.0f, 1f)
//            };
//            mesh.colors32 = color32;
//            mesh.RecalculateBounds();
//        }

//        public TextMeshProUGUI tmp;

//        public void Update() {
////            Texture ownText = m_fontAsset.material.GetTexture(ShaderUtilities.ID_MainTex);
////            Texture othText = tmp.fontMaterial.GetTexture(ShaderUtilities.ID_MainTex);
//            canvasRenderer.SetMaterial(m_fontAsset.material, ownText);
//            canvasRenderer.SetMesh(mesh);
//
//            //mesh.vertices = tmp.mesh.vertices;
//            //mesh.triangles = tmp.mesh.triangles;
//            //mesh.uv = tmp.mesh.uv;
//            //mesh.uv2 = tmp.mesh.uv2;
//            //mesh.RecalculateBounds();
////            Mesh tmpMesh = tmp.mesh;
////            Texture tex = m_fontAsset.material.GetTexture(ShaderUtilities.ID_MainTex);
////            Debug.Log(tmp.fontMaterial.GetVector(ShaderUtilities.ID_MaskCoord));
////            canvasRenderer.SetMaterial(tmp.fontMaterial, tmp.fontMaterial.GetTexture(ShaderUtilities.ID_MainTex));
////            canvasRenderer.SetMesh(tmp.mesh);
////            Vector3[] tmpMeshVertices = tmpMesh.vertices;
////            Vector3[] ourMeshVertices = mesh.vertices;
////            Debug.Log(tmpMesh);
//        }

    }

}