using System;
using Documentation.Features;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace Demo {

    public struct BrushSelection {
        public enum Category {
            BASIC, BLOCK, TEMPLATE
        }

        public enum BasicCategory {
            PERP, BEVEL, ROUND
        }

        public enum TemplateCategory {
            SEED, PLAYER
        }

        public Category category;
        public BasicCategory basic;
        public TemplateCategory templates;
        public int id;
    }
    
    public struct Brush {
        public int ID;
        public string Label;
        public Texture2D Texture;
    }

    [Template("Demo/BuildingDesigner/BuildingDesigner")]
    public class BuildingDesigner : UIElement {

        public RepeatableList<ISelectOption<int>> BrushTypes = new RepeatableList<ISelectOption<int>> {
            new SelectDemo.SelectOption<int>("Basic", (int) BrushSelection.Category.BASIC),
            new SelectDemo.SelectOption<int>("Blocks", (int) BrushSelection.Category.BLOCK),
            new SelectDemo.SelectOption<int>("Seed Templates", (int) BrushSelection.Category.TEMPLATE),
            new SelectDemo.SelectOption<int>("Player Templates", (int)BrushSelection.Category.TEMPLATE)
        };
        
        public Action<int> OnBrushTypeChanged => BrushTypeChanged;

        public BrushSelection.Category CurrentBrushType;
        public BrushSelection.BasicCategory CurrentBasicBrushStyle;
        public BrushSelection.TemplateCategory CurrentTemplateCategory;
        public int SelectedColorIndex;

        // all those selections are used in the template. as soon as uiforia can reference nested enums we can remove those again.
        public BrushSelection.Category CategoryBLOCK => BrushSelection.Category.BLOCK;
        public BrushSelection.Category CategoryBASIC => BrushSelection.Category.BASIC;
        public BrushSelection.Category CategoryTEMPLATE => BrushSelection.Category.TEMPLATE;

        public int PlaxinCost = 24;
        public string PlaxinIconPath;
        
        public bool IsPlayerTemplate => CurrentBrushType == BrushSelection.Category.TEMPLATE && CurrentTemplateCategory == BrushSelection.TemplateCategory.PLAYER;
        public bool IsSeedTemplate => CurrentBrushType == BrushSelection.Category.TEMPLATE && CurrentTemplateCategory == BrushSelection.TemplateCategory.SEED;
        public bool IsBasicSelected => CurrentBrushType == BrushSelection.Category.BASIC;

        public bool IsPaletteOpened;
        private bool dialogOpened;
        
        public RepeatableList<Brush> BasicRoundBrushes = new RepeatableList<Brush>(); 
        public RepeatableList<Brush> BasicBevelBrushes = new RepeatableList<Brush>(); 
        public RepeatableList<Brush> BasicPerpBrushes = new RepeatableList<Brush>(); 
        public RepeatableList<Brush> BlockBrushes = new RepeatableList<Brush>(); 
        public RepeatableList<Brush> TemplateSeedBrushes = new RepeatableList<Brush>(); 
        public RepeatableList<Brush> TemplatePlayerBrushes = new RepeatableList<Brush>(); 
        
        public RepeatableList<Color> AvailableColors;

        
        private void BrushTypeChanged(int category) {
            switch (category) {
                case 2:
                    CurrentTemplateCategory = BrushSelection.TemplateCategory.SEED;
                    break;
                case 3:
                    CurrentTemplateCategory = BrushSelection.TemplateCategory.PLAYER;
                    category--;
                    break;
            }

            CurrentBrushType = (BrushSelection.Category)category;
        }
        
    }

}