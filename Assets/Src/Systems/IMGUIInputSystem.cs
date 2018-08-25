using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Systems {

    public class IMGUIInputSystem : ISystem, IInputSystem {

        private readonly IStyleSystem styleSystem;
        private readonly ILayoutSystem layoutSystem;

        private readonly List<UIStyleSet> hoveredElements;

//        private struct StyleRect {
//
//           public readonly UIStyleSet 
//
//        }
        
        public IMGUIInputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.hoveredElements = new List<UIStyleSet>();
        }

        public void OnReset() { }

        public void OnUpdate() {
            switch (Event.current.type) {
                case EventType.MouseMove:
                    Vector2 mouse = Event.current.mousePosition;
                    int rectCount = layoutSystem.RectCount;
                    LayoutResult[] layoutResults = layoutSystem.LayoutResults;
                    for (int i = 0; i < rectCount; i++) {
                        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                        if (layoutResults[i].rect.Contains(mouse)) {

                            int elementId = layoutResults[i].elementId;
                            UIStyleSet style = styleSystem.GetStyleForElement(elementId);
                            if (style != null && style.HasHoverStyle && !hoveredElements.Contains(style)) {
                                style.EnterState(StyleState.Hover);
                                hoveredElements.Add(style);
                            }
                        }

                    }
                    for (int i = 0; i < hoveredElements.Count; i++) {
                        
                    }
                    break;
            }
        }

        public void OnDestroy() { }

        public void OnInitialize() { }

        public void OnElementCreated(UIElementCreationData elementData) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

    }

}