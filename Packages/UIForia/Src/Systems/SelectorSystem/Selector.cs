using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Selectors {

    public struct SelectorEffect {

        public int selectorId;
        public ElementReference elementReference;

    }

    public struct SelectorUsage {

        public int id;
        public Selector selector;
        public LightList<StyleSet2> resultSet;

    }

    public struct SelectionTarget {

        // condense into int makes sense here, can keep a map of int value for pairs
        public string tagName;
        public string moduleName;

        
    }

    public enum FromTarget {

        Children,
        Descendents,
        LexicalDescendents, // descendents but also picks up slots
        Self
        
    }

    public struct ElementStyleId { }

    public struct ElementAttributeId {

        

    }

    public struct Selector {

        public int id;
        // public SelectorQuery rootQuery;
        public StyleState state;
        public FromTarget fromTarget;
        
        public void Run(UIElement origin) {
            LightList<UIElement> result = LightList<UIElement>.Get();

            // rootQuery.Gather(origin, result);

            if (result.size == 0) {
                result.Release();
                return;
            }

//            if (rootQuery.next == null) {
//                // match!    
//                for (int i = 0; i < result.size; i++) {
//                    //result.array[i].style.SetSelectorStyle(matchStyle);
//                }
//
//                result.Release();
//                return;
//            }

            for (int i = 0; i < result.size; i++) {
  //              if (rootQuery.next.Run(origin, result.array[i])) {
  //                  //result.array[i].style.SetSelectorStyle(matchStyle);
  //              }
            }

            result.Release();
        }

        public void FilterTargets(LightList<UIElement> targets, Func<UIElement, bool> filter) {
            for (int i = 0; i < targets.size; i++) {
                UIElement candidate = targets.array[i];
                // todo this could be a generated lambda for sure
                if (!filter(candidate)) {
                    targets.SwapRemoveAt(i--);
                }
            }                
        }
        
        public void GatherTargets(UIElement element, LightList<UIElement> targets) {
            switch (fromTarget) {
                case FromTarget.Children:
                    
                    targets.EnsureCapacity(element.children.size);
                    
                    for (int i = 0; i < element.children.size; i++) {
                        UIElement child = element.children.array[i];
                        if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                            targets.array[targets.size++] = child;
                        }
                    }
                    break;

                case FromTarget.LexicalDescendents:
                    element.GetTemplateDatabase().GetEnabledDescendents(element, targets);
                    // todo -- also add slot descendents 
                    break;
                
                case FromTarget.Descendents:
                    // probably want to pass a filter function too and do this gather in 1 pass
                    element.GetTemplateDatabase().GetEnabledDescendents(element, targets);
                    break;

                case FromTarget.Self:
                    targets.Add(element);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}