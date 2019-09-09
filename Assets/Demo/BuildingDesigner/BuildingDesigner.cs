using System;
using Documentation.Features;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Demo.BuildingDesigner {

    public enum BrushSelectionCategory {

        Basic,
        Block,
        Template,
        Player

    }

    public class MenuItemData {
        public string ImageUrl;
        public string Label;
    }

    [Template("Demo/BuildingDesigner/BuildingDesigner")]
    public class BuildingDesigner : UIElement {

        public RepeatableList<MenuItemData> MenuItems = new RepeatableList<MenuItemData>() {
                new MenuItemData() { ImageUrl = "Prototyping/icon__menu_chat", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon_plant_64", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon__menu_build", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon_chop_64", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon__menu_schedule", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon__menu_stockpile", Label = "Construction" },
                new MenuItemData() { ImageUrl = "Prototyping/icon__menu_build", Label = "Construction" }
        };

        public RepeatableList<ISelectOption<int>> brushTypes = new RepeatableList<ISelectOption<int>> {
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