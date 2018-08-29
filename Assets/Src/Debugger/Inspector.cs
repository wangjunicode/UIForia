using System.Collections.Generic;
using Rendering;
using Src;
using Src.Input;
using Src.Layout;
using UnityEngine;

namespace Debugger {

    [Template("Src/Debugger/Inspector.xml")]
    public class Inspector : UIElement {

        public float time;
        public Vector2 mousePosition;
        public List<int> values;
        public bool showMe;
        public int selectedValue;
        public LayoutDirection dir;
        public int currentAxisMode;
        public MainAxisAlignment mainAlignment;
        public CrossAxisAlignment crossAxis;
        public bool isHorizontal;
        public int crossAxisIndex;
        public bool showError;
        
        public Inspector() {
            selectedValue = 0;
            this.values = new List<int>();
            mainAlignment = MainAxisAlignment.Start;
            dir = LayoutDirection.Row;
            isHorizontal = true;
        }

        public override void OnUpdate() {
//            time = Time.realtimeSinceStartup;
        }

        
        public void ToggleLayoutDirection() {
            if (dir == LayoutDirection.Column) {
                dir = LayoutDirection.Row;
                isHorizontal = true;
            }
            else {
                isHorizontal = false;
                dir = LayoutDirection.Column;
            }
        }

        public void ShowThings() {
            this.showMe = !showMe;
        }

        public void SwitchValue() {
            if (values.Count == 0) {
                showError = true;
            }
            if (values.Count > 0) {
                selectedValue = (selectedValue + 1) % values.Count;
            }
        }
        
        public void ToggleLayoutMode() {
            currentAxisMode += 1;
            currentAxisMode = currentAxisMode % 4;
            switch (currentAxisMode) {
                    case 0:
                        mainAlignment = MainAxisAlignment.Center;
                        break;
                    case 1:
                        mainAlignment = MainAxisAlignment.End;
                        break;
                    case 2:
                        mainAlignment = MainAxisAlignment.SpaceAround;
                        break;
                    case 3:
                        mainAlignment = MainAxisAlignment.SpaceAround;
                        break;
                    case 4:
                        mainAlignment = MainAxisAlignment.Start;
                        break;
            }
        }

        public void ToggleCrossAxis() {
            crossAxisIndex++;
            if (crossAxisIndex == 4) {
                crossAxisIndex = 0;
            }
            switch (crossAxisIndex) {
                case 0:
                    crossAxis = CrossAxisAlignment.Center;
                    break;
                case 1:
                    crossAxis = CrossAxisAlignment.End;
                    break;
                case 2:
                    crossAxis = CrossAxisAlignment.Stretch;
                    break;
                case 3:
                    crossAxis = CrossAxisAlignment.Start;
                    break;
            }
        }
        
        public void OnMouseEnter(MouseInputEvent evt) {
//            Debug.Log("Entered! " + evt.mousePosition);
        }

        public void OnMouseContext() {
            values.RemoveAt(values.Count - 1);
        }

        public void OnMouseDown(MouseInputEvent evt) {
            values.Add(values.Count);
            selectedValue = (selectedValue + 1) % values.Count;
        }

        public void AddToValues() {
            values.Add(values.Count);
            showError = false;
        }
    }

}