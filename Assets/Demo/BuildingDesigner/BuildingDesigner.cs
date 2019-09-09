using System;
using Documentation.Features;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Demo {

    public enum BrushSelectionCategory {

        Basic,
        Block,
        Template,
        Player

    }

    [Template("Demo/BuildingDesigner/BuildingDesigner")]
    public class BuildingDesigner : UIElement {

        public RepeatableList<ISelectOption<int>> BrushTypes = new RepeatableList<ISelectOption<int>> {
            new SelectDemo.SelectOption<int>("Basic", (int) BrushSelectionCategory.Basic),
            new SelectDemo.SelectOption<int>("Blocks", (int) BrushSelectionCategory.Block),
            new SelectDemo.SelectOption<int>("Seed Templates", (int) BrushSelectionCategory.Template),
            new SelectDemo.SelectOption<int>("Player Templates", (int)BrushSelectionCategory.Player)
        };
        
        public Action<int> OnBrushTypeChanged => BrushTypeChanged;

        
        private void BrushTypeChanged(int category) {
//            switch (category) {
//                case 2:
//                    CurrentTemplateCategory = BrushSelection.TemplateCategory.SEED;
//                    break;
//                case 3:
//                    CurrentTemplateCategory = BrushSelection.TemplateCategory.PLAYER;
//                    category--;
//                    break;
//            }
//
//            CurrentBrushType = (BrushSelection.Category)category;
        }
        
    }

}