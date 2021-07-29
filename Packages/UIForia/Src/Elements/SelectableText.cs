using UIForia.Attributes;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Elements {

    public class SelectTextDragEvent : DragEvent {
        
    }
    
    [TagName("Default", "SelectableText")]
    public class SelectableText : UITextElement {

        internal SelectionInfo cursor;

        public TextMode textMode;

        public override void OnCreate() {
            cursor.cursor = -1;
            cursor.selection = -1;
        }

        public unsafe void RecalcCursorPosition() {
            ref TextDataEntry entry = ref textTable->GetEntry(textId);
            TextShapeCache shapeCache = application.applicationLoop.appInfo->shapeCache;
            TextUtil.RecalcCursorPosition(ref entry, shapeCache, ref cursor);
        }

        [OnMouseClick]
        public unsafe void OnMouseClick(MouseInputEvent evt) {
            if (textMode != TextMode.Edit && textMode != TextMode.Select) {
                return;
            }

            float2 worldPosition = GetLayoutWorldMatrix().c3.xy;
            worldPosition.y *= -1;
            float2 mousePosition = evt.MousePosition - worldPosition;

            ref TextDataEntry entry = ref textTable->GetEntry(textId);
            ref TextShapeCache shapeCache = ref application.applicationLoop.appInfo->shapeCache;
            LightList<LineMapping> glyphLineMappings = TextUtil.CreateGlyphLineMappings(entry, ref shapeCache);
            if (evt.Shift) {
                TextUtil.SetSelectionToPosition(glyphLineMappings, entry, shapeCache, mousePosition, ref cursor);
            }
            else {
                TextUtil.SetCursorToPosition(glyphLineMappings, entry, shapeCache, mousePosition, ref cursor);
            }
            textTable->SetCursor(textId, cursor);
        }

        [OnDragCreate()]
        public DragEvent OnDragCreate(MouseInputEvent evt) {
            return new SelectTextDragEvent();
        }

        [OnDragMove()]
        public unsafe void OnDragMove(DragEvent evt) {
            Debug.Log("moving");
        }

        public float2 GetCursorPosition() {
            return cursor.cursorPosition;
        }

        public float GetCursorHeight() {
            return cursor.cursorHeight;
        }

    }

}