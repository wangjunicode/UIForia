using System;
using Documentation.Features;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace Demo {
    public enum ToolButton {
        NONE,
        SELECT,
        MAGIC_WAND,
        BRUSH,
        BUCKET,
        EYE_DROPPER,
        GRID_UP,
        GRID_DOWN,
        DELETE_ALL,
        ERASER,
        COLOR
    }

    public struct BrushSelection {
        public enum Category {
            BASIC,
            BLOCK,
            TEMPLATE
        }

        public enum BasicCategory {
            PERP,
            BEVEL,
            ROUND
        }

        public enum TemplateCategory {
            SEED,
            PLAYER
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
                new SelectOption<int>("Basic", (int)BrushSelection.Category.BASIC), 
                new SelectOption<int>("Blocks", (int)BrushSelection.Category.BLOCK), 
                new SelectOption<int>("Seed Templates", (int)BrushSelection.Category.TEMPLATE), 
                new SelectOption<int>("Player Templates", (int)BrushSelection.Category.TEMPLATE)
        };
        public RepeatableList<ISelectOption<int>> BasicStyles = new RepeatableList<ISelectOption<int>> {
                new SelectOption<int>("Rounded Corners", (int)BrushSelection.BasicCategory.ROUND),
                new SelectOption<int>("Bevel Corners", (int)BrushSelection.BasicCategory.BEVEL),
                new SelectOption<int>("Perpendicular Corners", (int)BrushSelection.BasicCategory.PERP)
        };

        public Action<int> OnBrushTypeChanged => BrushTypeChanged;
        public Action<int> OnBasicStyleChanged => BasicStyleChanged;

        public BrushSelection.Category CurrentBrushType;
        public BrushSelection.BasicCategory CurrentBasicBrushStyle;
        public BrushSelection.TemplateCategory CurrentTemplateCategory;

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

        public RepeatableList<Brush> BasicRoundBrushes = new RepeatableList<Brush>();
        public RepeatableList<Brush> BasicBevelBrushes = new RepeatableList<Brush>();
        public RepeatableList<Brush> BasicPerpBrushes = new RepeatableList<Brush>();
        public RepeatableList<Brush> BlockBrushes = new RepeatableList<Brush>();
        public RepeatableList<Brush> TemplateSeedBrushes = new RepeatableList<Brush>();
        public RepeatableList<Brush> TemplatePlayerBrushes = new RepeatableList<Brush>();

        public RepeatableList<Color> AvailableColors;
        public Color SelectedColor;

        private bool dialogOpened;
        private UIElement prevTool;
        private Color lightGrey = new Color(0.39f, 0.39f, 0.39f);

        public override void OnCreate() {

            Texture2D[] textures = Resources.LoadAll<Texture2D>("building-previews");
            
            for (int i = 0; i < textures.Length; i++) {
                Texture2D texture = textures[i];
                if (texture.name.StartsWith("BASIC_BVL")) {
                    BasicBevelBrushes.Add(MakeBrush(texture, i, "BASIC_BVL"));
                } else if (texture.name.StartsWith("BASIC_PRP")) {
                    BasicPerpBrushes.Add(MakeBrush(texture, i, "BASIC_PRP"));
                } else if (texture.name.StartsWith("BASIC_RND")) {
                    BasicRoundBrushes.Add(MakeBrush(texture, i, "BASIC_RND"));
                } else if (texture.name.StartsWith("BLOCK")) {
                    BlockBrushes.Add(MakeBrush(texture, i, "BLOCK"));
                }

            }
            Application.styleImporter.ImportStyleSheetFromFile("Demo/BuildingDesigner/BuildingDesignerMain.style").TryGetAnimationData("open-dialog", out openDialogAnimation);
            Application.styleImporter.ImportStyleSheetFromFile("Demo/BuildingDesigner/BuildingDesignerMain.style").TryGetAnimationData("close-dialog", out closeDialogAnimation);

            AvailableColors = new RepeatableList<Color>() {
                    new Color(0.1f, 0.6f, 0.6f),
                    new Color(0.2f, 0.7f, 0.5f),
                    new Color(0.4f, 0.4f, 0.4f),
                    new Color(0.6f, 0.3f, 0.3f),
                    new Color(0.7f, 0.1f, 0.2f),
                    new Color(0.8f, 0.7f, 0.1f),
                    new Color(0.1f, 0.6f, 0.6f),
                    new Color(0.2f, 0.7f, 0.5f),
                    new Color(0.4f, 0.4f, 0.4f),
                    new Color(0.6f, 0.3f, 0.3f),
                    new Color(0.7f, 0.1f, 0.2f),
                    new Color(0.8f, 0.7f, 0.1f),
            };
        }

        private static Brush MakeBrush(Texture2D texture, int id, string prefix) {
            return new Brush() {
                    Label = texture.name.Replace(prefix, string.Empty).Replace("_", " ").Replace("-", ""),
                    ID = id,
                    Texture = texture
            };
        }

        private AnimationData openDialogAnimation;
        private AnimationData closeDialogAnimation;
        public void ShowDialog(string id) {
            if (dialogOpened) {
                return;
            }
            
            dialogOpened = true;
            
            UIElement confirmDialog = FindById(id);
            
            Application.Animate(confirmDialog, openDialogAnimation);
        }
        public void CloseDialog(string id) {
            if (!dialogOpened) {
                return;
            }
            
            dialogOpened = false;
            
            UIElement confirmDialog = FindById(id);
            
            Application.Animate(confirmDialog, closeDialogAnimation);
        }

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

        private void BasicStyleChanged(int style) {
            if (CurrentBrushType != BrushSelection.Category.BASIC) {
                return;
            }

            CurrentBasicBrushStyle = (BrushSelection.BasicCategory)style;
        }

        public void SaveAndExit() {
            if (dialogOpened) {
                return;
            }

            // uiBuildingDesignController.SelectAllCells();
            SaveBuilding();
            // uiBuildingDesignController.AbortDesign();
        }

        public void OpenColorPalette() {
            IsPaletteOpened = !IsPaletteOpened;
        }

        public void SelectColor(Color color) {
            IsPaletteOpened = false;
            SelectedColor = color;
        }

        public RepeatableList<Brush> GetBasicBrushes(BrushSelection.BasicCategory style) {
            switch (style) {
                case BrushSelection.BasicCategory.BEVEL:
                    return BasicBevelBrushes;
                case BrushSelection.BasicCategory.PERP:
                    return BasicPerpBrushes;
                default:
                    return BasicRoundBrushes;
            }
        }
        
        
        public void SelectBrush(MouseInputEvent evt, Brush brush) {
            evt.StopPropagation();
            DeselectToolButton();
            // uiBuildingDesignController.SelectBrush(
            //         new BrushSelection {id = brush.ID, category = CurrentBrushType, basic = CurrentBasicBrushStyle, templates = CurrentTemplateCategory});
        }
        
        private void DeselectToolButton() {
            prevTool?.style.SetBackgroundColor(lightGrey, StyleState.Normal);
            prevTool = null;
        }
        
        public void DeleteBrush(MouseInputEvent evt, Brush brush) {
            evt.StopPropagation();
            
            // uiBuildingDesignController.DeleteBrush(brush.ID);
            
            for (var i = 0; i < TemplatePlayerBrushes.Count; i++) {
                if (TemplatePlayerBrushes[i].ID == brush.ID) {
                    TemplatePlayerBrushes.RemoveAt(i);
                    break;
                }
            }
        }
        
        public void SaveBuilding() {
            var name = "Template " + (TemplatePlayerBrushes.Count + 1);
            // uiBuildingDesignController.SaveBrush(name);
            // uiBuildingDesignController.ChangeDesignMode(DesignMode.BLOCKS);
        }
        
        public void ToolButtonClicked(ToolButton id) {
            UIElement tool = FindById(id.ToString());

            prevTool?.style.SetBackgroundColor(lightGrey, StyleState.Normal);

            bool changeIconColor = true;

            switch (id) {
                case ToolButton.SELECT:
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.SELECTION);
                    break;
                case ToolButton.MAGIC_WAND:
                    changeIconColor = false;
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.SELECTION);
                    // uiBuildingDesignController.SelectAllCells();
                    break;
                case ToolButton.BRUSH:
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.PAINT);
                    break;
                case ToolButton.BUCKET:
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.PAINT);
                    // uiBuildingDesignController.ChangePaintMode(DesignPaintMode.FILL);
                    break;
                case ToolButton.EYE_DROPPER:
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.PAINT);
                    // uiBuildingDesignController.ChangePaintMode(DesignPaintMode.PICK);
                    break;
                case ToolButton.ERASER:
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.BLOCKS);
                    // uiBuildingDesignController.ChangeBlockMode(DesignBlockMode.SUB);
                    break;
                case ToolButton.GRID_UP:
                    changeIconColor = false;
                    // uiBuildingDesignController.ChangeVisibilityHeight(1);
                    break;
                case ToolButton.GRID_DOWN:
                    changeIconColor = false;
                    // uiBuildingDesignController.ChangeVisibilityHeight(-1);
                    break;
                case ToolButton.DELETE_ALL:
                    changeIconColor = false;
                    // uiBuildingDesignController.ClearDesign();
                    // uiBuildingDesignController.ChangeDesignMode(DesignMode.BLOCKS);
                    break;
                case ToolButton.COLOR:
                    break;
            }

            if (!changeIconColor) {
                prevTool = null;
                return;
            }

            tool.style.SetBackgroundColor(Color.black, StyleState.Normal);
            prevTool = tool;
        }

    }

}
