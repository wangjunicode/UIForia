using System;
using System.Globalization;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace Demo {

    [Template("Demo/BuildingDesigner/BuildingDesigner")]
    public class BuildingDesigner : UIElement {

        #region parameters
        /// <summary>
        /// Element Property that controls the show/hide animation  
        /// </summary>
        public bool isActive;
        #endregion

        #region callback properties
        public Action<int> OnBrushTypeChanged => BrushTypeChanged;
        public Action<int> OnBasicStyleChanged => BasicStyleChanged;
        public Action onCancel => ExitBuildingDesigner;
        public Action onConfirm => ConfirmDesign;
        #endregion

        #region properties
        public RepeatableList<ISelectOption<int>> brushTypes;
        public RepeatableList<ISelectOption<int>> basicStyles;
        
        public Category currentBrushType;
        public BasicCategory currentBasicBrushStyle;
        
        public RepeatableList<Color> availableColors;

        public Color green = new Color32(100, 205, 125, 255);
        public Color red = new Color32(255, 30, 30, 255);
        #endregion
        
        private RepeatableList<Brush> basicRoundBrushes = new RepeatableList<Brush>();
        private RepeatableList<Brush> basicBevelBrushes = new RepeatableList<Brush>();
        private RepeatableList<Brush> basicPerpBrushes = new RepeatableList<Brush>();
        private RepeatableList<Brush> blockBrushes = new RepeatableList<Brush>();

        [UsedImplicitly]
        public bool showCancel;
        [UsedImplicitly]
        public bool showConfirm;
        
        private UIElement prevTool;
        private ScrollView brushesContainer;
        
        /// <summary>
        /// Life-cycle method; called once
        /// </summary>
        public override void OnCreate() {
            GenerateDummyData();
        }
        
        [OnPropertyChanged("isActive")]
        public void OnActiveChanged(string propertyName) {
            SetAttribute("placement", isActive ? "show" : "hide");
        }
        
        #region event handlers

        private void ConfirmDesign() {
            //TriggerEvent(new UIPanelEvent(UIPanel.Dock));
        }
        
        private void ExitBuildingDesigner() {
            //TriggerEvent(new UIPanelEvent(UIPanel.Dock));
        }
        
        private void BasicStyleChanged(int style) {
            if (currentBrushType != Category.Basic) {
                return;
            }

            currentBasicBrushStyle = (BasicCategory)style;
        }

        private void BrushTypeChanged(int category) {
            Category prev = currentBrushType;
            currentBrushType = (Category)category;
            if (prev != currentBrushType) {
                brushesContainer.ScrollToHorizontalPercent(0f);
                brushesContainer.ScrollToVerticalPercent(0f);
            }
        }

        public void ShowCancelDialog() {
            showCancel = true;
        }

        public void ShowConfirmDialog() {
            showConfirm = true;
        }

        public void SaveAndExit() {
            SaveBuilding();
            //TriggerEvent(new UIPanelEvent(UIPanel.Dock));
        }

        public RepeatableList<Brush> GetBrushes() {
            if (currentBrushType == Category.Basic) {
                switch (currentBasicBrushStyle) {
                    case BasicCategory.BEVEL:
                        return basicBevelBrushes;
                    case BasicCategory.PERP:
                        return basicPerpBrushes;
                    default:
                        return basicRoundBrushes;
                }
            }

            return blockBrushes;
        }

        public void SelectBrush(MouseInputEvent evt, Brush brush) {
            evt.StopPropagation();
            DeselectToolButton();
        }
        
        private void DeselectToolButton() {
            prevTool?.SetAttribute("selected", null);
            prevTool = null;
        }

        public void SaveBuilding() {
            // save the building in the database
        }

        public void ToolButtonClicked(ToolButton id) {
            UIElement tool = FindById(id.ToString());
            prevTool?.SetAttribute("selected", null);

            switch (id) {
                case ToolButton.MAGIC_WAND:
                case ToolButton.GRID_UP:
                case ToolButton.GRID_DOWN:
                case ToolButton.DELETE_ALL:
                    prevTool = null;
                    return;
            }

            tool.SetAttribute("selected", "true");
            prevTool = tool;
        }
        #endregion
        
        #region dummydata
        private void GenerateDummyData() {
            brushesContainer = FindById<ScrollView>("brushes-container");

            brushTypes = new RepeatableList<ISelectOption<int>> {
                new SelectOption<int>("Basic", (int) Category.Basic),
                new SelectOption<int>("Blocks", (int) Category.Block)
            };

            basicStyles = new RepeatableList<ISelectOption<int>> {
                new SelectOption<int>("Rounded Corners", (int) BasicCategory.ROUND),
                new SelectOption<int>("Bevel Corners", (int) BasicCategory.BEVEL),
                new SelectOption<int>("Perpendicular Corners", (int) BasicCategory.PERP)
            };

            Texture2D[] textures = Resources.LoadAll<Texture2D>("building-previews");

            for (int i = 0; i < textures.Length; i++) {
                Texture2D texture = textures[i];
                if (texture.name.StartsWith("BASIC_BVL")) {
                    basicBevelBrushes.Add(MakeBrush(texture, i));
                }
                else if (texture.name.StartsWith("BASIC_PRP")) {
                    basicPerpBrushes.Add(MakeBrush(texture, i));
                }
                else if (texture.name.StartsWith("BASIC_RND")) {
                    basicRoundBrushes.Add(MakeBrush(texture, i));
                }
                else if (texture.name.StartsWith("BLOCK")) {
                    blockBrushes.Add(MakeBrush(texture, i));
                }
            }

            availableColors = new RepeatableList<Color>() {
                new Color(0.1f, 0.6f, 0.6f),
                new Color(0.2f, 0.7f, 0.5f),
                new Color(0.4f, 0.4f, 0.4f),
                new Color(0.6f, 0.3f, 0.3f),
                new Color(0.7f, 0.1f, 0.2f),
                new Color(0.8f, 0.7f, 0.15f),
                new Color(0.16f, 0.6f, 0.62f),
                new Color(0.23f, 0.7f, 0.35f),
                new Color(0.81f, 0.67f, 0.1f),
            };
        }

        private static Brush MakeBrush(Texture2D texture, int id) {
            return new Brush() {
                    Label = BuildingTexts.Brush(texture.name.Substring(texture.name.LastIndexOf("-") + 1)),
                    ID = id,
                    Texture = texture
            };
        }
        #endregion
    }
}
