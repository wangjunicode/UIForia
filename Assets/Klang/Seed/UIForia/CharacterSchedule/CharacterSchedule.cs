using System.Collections.Generic;
using Klang.Seed.DataTypes;
using UIForia;
using UIForia.Input;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UI {

    public class CharacterScheduleBlock {

        public int start;
        public int duration;

        public CharacterScheduleBlock(int start, int duration) {
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
            slotContainer = slotContainer ?? FindById("slot-container");
            UIElement[] slotChildren = slotContainer.children;

            for (int i = 0; i < slotChildren.Length; i++) {
                LayoutResult result = slotChildren[i].layoutResult;
                if (result.ScreenRect.Contains(evt.MousePosition)) {
                    int start = (int) (result.ActualHeight) * i;
                    scheduleBlocks.Add(new CharacterScheduleBlock(start, 1));
                }
            }
        }

    }

}