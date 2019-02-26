using System;
using TMPro.EditorUtilities;
using UIForia;
using UIForia.Rendering;

namespace UI {

    public enum ToolTipDirection {

        Top,
        Bottom,
        Right,
        Left

    }
    
    [Template("Klang/Seed/UIForia/KlangToolTip/KlangToolTip.xml")]
    public class KlangToolTip : UIElement {

        public string text;
        public ToolTipDirection direction;
        public bool showToolTip;

        private UIElement toolTipRoot;

        public KlangToolTip() {
            direction = ToolTipDirection.Right;
        }
        
        public override void OnCreate() {
            toolTipRoot = FindById("tool-tip-root");
        }

        [OnMouseEnter]
        private void OnMouseEnter() {
            showToolTip = true;
        }

        [OnMouseExit]
        private void OnMouseExit() {
            showToolTip = false;
        }
        
        public override void OnUpdate() {
            if (!showToolTip) return;
            switch (direction) {
                case ToolTipDirection.Top:
                    throw new NotImplementedException();
                case ToolTipDirection.Bottom:
                    throw new NotImplementedException();
                case ToolTipDirection.Right: {
                   // toolTipRoot.style.SetTransformBehaviorX(TransformBehavior.AnchorMaxOffset, StyleState.Normal);
                    toolTipRoot.style.SetTransformPositionX(new TransformOffset(-1, TransformUnit.ActualWidth), StyleState.Normal );
                    break;
                }
                case ToolTipDirection.Left: {
                    float width = toolTipRoot.layoutResult.actualSize.width;
                    toolTipRoot.style.SetTransformBehaviorX(TransformBehavior.AnchorMinOffset, StyleState.Normal);
                    toolTipRoot.style.SetTransformPositionX(new TransformOffset(1, TransformUnit.ActualWidth), StyleState.Normal );
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
    }

}