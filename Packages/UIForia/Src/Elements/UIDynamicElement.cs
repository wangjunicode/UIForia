using System;
using UIForia.Attributes;

namespace UIForia.Elements {

    public interface IDynamicData {

        Type ElementType { get; }

    }
    
    public interface IDynamicElement {

        void SetData(IDynamicData data);

    }

    [TemplateTagName("Dynamic")]
    public class UIDynamicElement : UIContainerElement {

        public IDynamicData data;
        private IDynamicData previousData;

        public UIDynamicElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        [OnPropertyChanged(nameof(data))]
        private void OnDataChanged(string propertyName) {
            if (previousData != null && data != null) {
                if (previousData.ElementType == data.ElementType) {
                    previousData = data;
                    return;
                }
            }
            
            View.Application.DestroyChildren(this);
            
            if (data != null && View.Application.CreateChildElement(this, data.ElementType) is IDynamicElement child) {
                child.SetData(data);
            }

            previousData = data;
        }

    }

}