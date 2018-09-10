using Src;
using Src.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Graphic x-id='graphic'/>
            </Contents>
        </UITemplate>
    ")]
    public class CaretGraphic : UIElement {

        private UIGraphicElement graphic;
        private static readonly UIVertex[] s_Vertices = new UIVertex[4];
        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
        
        public void UpdateMesh(Rect caretRect, Color color) {
            Mesh mesh = graphic.GetMesh();
            
            s_Vertices[0].position = new Vector3(caretRect.x, caretRect.height, 0.0f);
            s_Vertices[1].position = new Vector3(caretRect.x, caretRect.y, 0.0f);
            s_Vertices[2].position = new Vector3(caretRect.x + caretRect.width, caretRect.y, 0.0f);
            s_Vertices[3].position = new Vector3(caretRect.x + caretRect.width, caretRect.height, 0.0f);
            
            s_Vertices[0].color = color;
            s_Vertices[1].color = color;
            s_Vertices[2].color = color;
            s_Vertices[3].color = color;
            
            s_VertexHelper.AddUIVertexQuad(s_Vertices);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            
            graphic.UpdateMesh();
        }
        
        public override void OnCreate() {
            graphic = (UIGraphicElement) FindById("graphic");
            graphic.SetActive(false);
        }

    }

}