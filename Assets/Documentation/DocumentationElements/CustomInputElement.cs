using System;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace Documentation.DocumentationElements {
    
    public class CustomInputData {
        public float MyVal;

        protected bool Equals(CustomInputData other) {
            return MyVal.Equals(other.MyVal);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((CustomInputData)obj);
        }

        public override int GetHashCode() {
            return MyVal.GetHashCode();
        }
    }
    public class DemoDragEvent : DragEvent {

        private Vector2 offset;
        
        public DemoDragEvent(UIElement element, Vector2 offset) {
            this.offset = offset - element.layoutResult.screenPosition;
        }

        public override void Update() {
            origin.style.SetAlignmentOffsetX(new OffsetMeasurement(MousePosition.x - offset.x), StyleState.Normal);
        }

        public override void Drop(bool success) {
            base.Drop(success);
            origin.style.SetAlignmentOffsetX(new OffsetMeasurement(0), StyleState.Normal);
        }

    }
    
    [Template("DocumentationElements/CustomInputElement.xml")]
    public class CustomInputElement : UIElement {

        public CustomInputData Value;

        public float num;

        public event Action<CustomInputData> onValueChanged;
        
        public event Action<float> onNumChanged;

        [OnPropertyChanged(nameof(Value))]
        public void OnMyValChanged() {
            Debug.Log($"CustomInputElement: Value changed");
            //onValueChanged?.Invoke(Value);
        }
        
        [OnPropertyChanged(nameof(num))]
        public void OnNumValueChanged() {
            Debug.Log($"CustomInputElement: num changed");
            // invoking the custom event handler trigger code in BindingsDemo.cs
            //onNumChanged?.Invoke(num);
            // Doesn't do anything obvious. Should be related to two-way-binding. todo: figure that out
        }

        public DragEvent onDragCreate(MouseInputEvent evt) {
            // moves the element as the mouse moves
            return new DemoDragEvent(evt.Origin, evt.MousePosition);
        }
        
        public void OnDragMove(DragEvent evt) {

            evt.StopPropagation();
            
            Debug.Log("You are dragging the yellow thing.");
            
            // we need to create a new object here otherwise the OnPropertyChanged handler wouldn't fire
            Value = new CustomInputData() {
                    MyVal = evt.MousePosition.x
            };

            num = evt.MousePosition.x;
            
            onValueChanged?.Invoke(Value);
            onNumChanged?.Invoke(num);
        }
 
    }
}
