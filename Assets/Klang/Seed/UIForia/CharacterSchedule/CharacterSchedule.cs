using System.Collections.Generic;
using Klang.Seed.DataTypes;
using UIForia;
using UIForia.Extensions;
using UIForia.Input;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
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
        public List<CharacterScheduleBlock> scheduleBlocks = new List<CharacterScheduleBlock>();
        private UIElement slotContainer;

        public void CreateDestroySchedule(MouseInputEvent evt) {

            bool leftUpThisFrame = evt.IsMouseLeftUpThisFrame;
            bool rightUpThisFrame = evt.IsMouseRightUpThisFrame;

            if (!(leftUpThisFrame || rightUpThisFrame)) {
                return;
            }
            
            slotContainer = slotContainer ?? FindById("slot-container");
            UIElement[] slotChildren = slotContainer.children;

            for (int i = 0; i < slotChildren.Length; i++) {
                LayoutResult result = slotChildren[i].layoutResult;
                if (!result.ScreenRect.Contains(evt.MousePosition)) continue;
                
                if (leftUpThisFrame) {
                    int start = (int) result.ActualHeight * i;
                    scheduleBlocks.Add(new CharacterScheduleBlock(i, start, 1));
                }
                else {
                    int foundIndex = scheduleBlocks.FindIndex(i, (element, index) => element.position == index);
                    if (foundIndex > -1) {
                        scheduleBlocks.RemoveAt(foundIndex);
                    }
                }

                return;
            }
        }

    }

}