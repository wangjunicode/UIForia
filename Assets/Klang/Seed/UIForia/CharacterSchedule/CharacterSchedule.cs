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

        public int position;
        public int start;
        public int duration;

        public CharacterScheduleBlock(int position, int start, int duration) {
            this.position = position;
            this.start = start;
            this.duration = duration;
        }

    }

    [Template("Klang/Seed/UIForia/CharacterSchedule/CharacterSchedule.xml")]
    public class CharacterSchedule : UIElement {

        public CharacterData characterData;
        public RepeatableList<CharacterScheduleBlock> scheduleBlocks = new RepeatableList<CharacterScheduleBlock>();
        private UIElement slotContainer;

        public void CreateDestroySchedule(MouseInputEvent evt) {
            bool leftUpThisFrame = evt.IsMouseLeftUpThisFrame;
            bool rightUpThisFrame = evt.IsMouseRightUpThisFrame;

            if (!(leftUpThisFrame || rightUpThisFrame)) {
                return;
            }

            slotContainer = slotContainer ?? FindById("slot-container");
            List<UIElement> slotChildren = slotContainer.GetChildren();

            for (int i = 0; i < slotChildren.Count; i++) {
                LayoutResult result = slotChildren[i].layoutResult;
                if (!result.ScreenRect.Contains(evt.MousePosition)) continue;

                if (leftUpThisFrame) {
                    int start = (int) result.ActualHeight * i;
                    scheduleBlocks.Add(new CharacterScheduleBlock(i, start, 1));
                }
                else {
                    for (int j = 0; j < scheduleBlocks.Count; j++) {
                        if (scheduleBlocks[j].position == i) {
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