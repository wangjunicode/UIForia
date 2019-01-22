using System.Collections.Generic;
using Klang.Seed.DataTypes;
using UIForia;
using UIForia.Extensions;
using UIForia.Input;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UI {

    public class CharacterScheduleBlock {

        public int start;
        public int end;

        public CharacterScheduleBlock(int start, int end) {
            this.start = start;
            this.end = end;
        }

    }

    public class ScheduleDragEvent : DragEvent {

        private CharacterScheduleBlock block;

        private int maxIndexTop;

        private int maxIndexBottom;

        public ScheduleDragEvent(UIElement origin, CharacterScheduleBlock block, int maxIndexTop, int maxIndexBottom) : base(origin) {
            this.block = block;
            this.maxIndexTop = maxIndexTop;
            this.maxIndexBottom = maxIndexBottom;
        }

        public override void Update() {
            const int blockSize = 24;
            UIElement parent = (UIElement) origin.Parent;
            Rect parentScreenRect = parent.layoutResult.ScreenRect;
            Vector2 dragStart = DragStartPosition;
            int y = (int) (MousePosition.y - parentScreenRect.y);
            int idx = Mathf.Clamp(y / blockSize, maxIndexTop, maxIndexBottom);

            if (MousePosition.y < dragStart.y) {
                block.start = idx;
            }
            else {
                block.end = idx;
            }
        }
    }

    [Template("Klang/Seed/UIForia/CharacterSchedule/CharacterSchedule.xml")]
    public class CharacterSchedule : UIElement {

        public CharacterData characterData;
        public RepeatableList<CharacterScheduleBlock> scheduleBlocks = new RepeatableList<CharacterScheduleBlock>();
        private UIElement slotContainer;
        private List<UIElement> uiElements;

        public DragEvent StartScheduleDrag(MouseInputEvent evt, UIElement element, CharacterScheduleBlock block) {
            // max to drag up
            // step size
            // max to drag down
            Vector2 mouseInElement = evt.MouseDownPosition - element.layoutResult.screenPosition;

            int maxIndexBottom = 23;
            int maxIndexTop = 0;
               
            for (int i = 0; i < scheduleBlocks.Count; i++) {
                CharacterScheduleBlock cs = scheduleBlocks[i];

                if (cs == block) continue;
                
                if (block.start > cs.end && cs.end > maxIndexTop) {
                    maxIndexTop = cs.end + 1;
                } else if (block.end < cs.start && cs.start < maxIndexBottom) {
                    maxIndexBottom = cs.start - 1;
                }
            }
  
            return new ScheduleDragEvent(element, block, maxIndexTop, maxIndexBottom);
        }

        public override void OnCreate() {
            base.OnCreate();
            slotContainer = FindById("slot-container");
            uiElements = slotContainer.GetChildren();
        }

        public TransformOffset CalculateTransformY(CharacterScheduleBlock block) {
            return uiElements[block.start].layoutResult.localPosition.y;
        }

        public UIMeasurement CalculateHeight(CharacterScheduleBlock block) {
            return uiElements[block.end].layoutResult.LocalRect.yMax - uiElements[block.start].layoutResult.localPosition.y;
        }

        public void CreateDestroySchedule(MouseInputEvent evt) {
            bool leftUpThisFrame = evt.IsMouseLeftUpThisFrame;
            bool rightUpThisFrame = evt.IsMouseRightUpThisFrame;

            if (!(leftUpThisFrame || rightUpThisFrame)) {
                return;
            }

            for (int i = 0; i < uiElements.Count; i++) {
                LayoutResult result = uiElements[i].layoutResult;
                if (!result.ScreenRect.Contains(evt.MousePosition)) continue;

                if (leftUpThisFrame) {

                    for (int j = 0; j < scheduleBlocks.Count; j++) {
                        CharacterScheduleBlock characterScheduleBlock = scheduleBlocks[j];
                        if (characterScheduleBlock.start <= i && characterScheduleBlock.end >= i) {
                            return;
                        } 
                    }
                    
                    scheduleBlocks.Add(new CharacterScheduleBlock(i, i));
                }
                else {
                    for (int j = 0; j < scheduleBlocks.Count; j++) {
                        if (scheduleBlocks[j].start == i) {
                            scheduleBlocks.RemoveAt(j);
                            return;
                        }
                    }
                }

                return;
            }
        }

    }

}
