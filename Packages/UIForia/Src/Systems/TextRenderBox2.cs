using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Text;

namespace UIForia.Rendering {

    public unsafe class TextRenderBox2 : RenderBox {
        
        public override void OnInitialize() {
            base.OnInitialize();
            isBuiltIn = true;
            isTextBox = true;
        }

        public override void PaintBackground3(RenderContext3 ctx) {

            // need to know if text changed
            // or was laid out differently
            // or style changed
            // or span style changed
            // or or or
            // maybe regenerating isnt so awful now 
            // can still do high level culling of lines, maybe even characters if line is overlapping its clip bounds. can be parallel and is fast
            // profile, maybe its not a problem
            UITextElement textElement = (UITextElement) element;

            ctx.SetTextMaterials(textElement.textInfo->materialBuffer);
            for (int i = 0; i < textElement.textInfo->renderRangeList.size; i++) {
                ref TextRenderRange render = ref textElement.textInfo->renderRangeList[i];

                // todo -- should definitely do a broadphase cull here 
                // bool overlappingOrContains = xMax >= clipper.aabb.xMin && xMin <= clipper.aabb.xMax && yMax >= clipper.aabb.yMin && yMin <= clipper.aabb.yMax;

                switch (render.type) {

                    case TextRenderType.Characters:
                        ctx.DrawTextCharacters(render);
                        break;

                    case TextRenderType.Highlight:
                        ctx.DrawTextHighlight(render, textElement.style.SelectionBackgroundColor);
                        break;

                    case TextRenderType.Underline:
                        break;

                    case TextRenderType.Sprite:
                        break;

                    case TextRenderType.Image:
                        break;

                    case TextRenderType.Element:
                        break;
                }

            }

        }

    }

}