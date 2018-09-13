using System.Collections.Generic;
using Rendering;

namespace Src.Systems {

    public interface IElementRegistry {

        UIElement GetElement(int elementId);

    }

    public class ElementRegistrySystem : IElementRegistry, ISystem {

        private readonly Dictionary<int, UIElement> map;

        public ElementRegistrySystem() {
            map = new Dictionary<int, UIElement>();
        }

        public UIElement GetElement(int elementId) {
            UIElement retn;
            map.TryGetValue(elementId, out retn);
            return retn;
        }

        public void OnReset() {
            map.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() {
            map.Clear();
        }

        public void OnReady() {
            
        }

        public void OnInitialize() { }

        public void OnElementCreated(MetaData elementData) {
            elementData.element.name = elementData.name;
            map[elementData.element.id] = elementData.element;
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            map.Remove(element.id);
        }

        public void OnElementShown(UIElement element) {
        }

        public void OnElementHidden(UIElement element) {
        }

    }

}