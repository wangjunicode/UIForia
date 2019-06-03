using System;
using Documentation.Features;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEditor.U2D;
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
        
        public DemoDragEvent(UIElement element, Vector2 offset) : base(element) {
            this.offset = offset - element.layoutResult.screenPosition;
        }

        public override void Update() {
            origin.style.SetAnchorTarget(AnchorTarget.Screen, StyleState.Normal);
            origin.style.SetAnchorLeft(new UIFixedLength(0), StyleState.Normal);
            origin.style.SetAnchorTop(new UIFixedLength(0), StyleState.Normal);
            origin.style.SetTransformBehavior(TransformBehavior.AnchorMinOffset, StyleState.Normal);
            origin.style.SetTransformPositionX(new TransformOffset(MousePosition.x - offset.x), StyleState.Normal);
            origin.style.SetTransformPositionY(new TransformOffset(MousePosition.y - offset.y), StyleState.Normal);
        }

        public override void Drop(bool success) {
            base.Drop(success);
            origin.style.SetAnchorTarget(AnchorTarget.Parent, StyleState.Normal);
            origin.style.SetTransformBehavior(TransformBehavior.LayoutOffset, StyleState.Normal);
            origin.style.SetTransformPositionX(new TransformOffset(0), StyleState.Normal);
            origin.style.SetTransformPositionY(new TransformOffset(0), StyleState.Normal);
        }

    }
    
    [Template("Documentation/DocumentationElements/CustomInputElement.xml")]
    public class CustomInputElement : UIElement {

        public CustomInputData Value;

        public float num;

        public event Action<float> customOnNumChange;
        
        /*
         * todo: <Text>MyVal: {Value.MyVal}</Text> results in InvalidCastException: Specified cast is not valid. 
         */
        public string GetMyVal() {
            return Value.MyVal.ToString();
        }

        [WriteBinding(nameof(Value))]
        public event Action<CustomInputData> onValueChanged;
        
        [WriteBinding(nameof(num))]
        public event Action<float> onNumChanged;

        [OnPropertyChanged(nameof(Value))]
        public void OnMyValChanged(string propertyName) {
            Debug.Log($"CustomInputElement: {propertyName} changed");
            onValueChanged?.Invoke(Value);
        }
        
        [OnPropertyChanged(nameof(num))]
        public void OnNumValueChanged(string propertyName) {
            Debug.Log($"CustomInputElement: {propertyName} changed");
            // invoking the custom event handler trigger code in BindingsDemo.cs
            onNumChanged?.Invoke(num);
            // Doesn't do anything obvious. Should be related to two-way-binding. todo: figure that out
            customOnNumChange?.Invoke(num);
        }

        public DragEvent onDragCreate(MouseInputEvent evt) {
            // moves the element as the mouse moves
            return new DemoDragEvent(this, evt.MousePosition);
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
            customOnNumChange?.Invoke(num);
        }
 
    }
}
