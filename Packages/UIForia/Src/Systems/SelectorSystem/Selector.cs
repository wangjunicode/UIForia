using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Selectors {

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

        public int id;             // this might be an index actually
        public int styleId;
        
        // public SelectorQuery rootQuery;
        public StyleState state; // maybe don't even need this, computed based on id + ranges
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
        
        // for the descendent cases there is probably some advantage to running top down and 
        // some how storing traversal results in a sorted way that lets a child who also
        // has a selector exploit the descendent list the parent already gathered.
        // maybe with a depth + traversal index check?
        public void Run(UIElement element, LightList<UIElement> targets) {
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

        public void Run(UIElement element, int computedSelectorId, StructList<GucciSystem.SelectorEffect> targets) {
            throw new NotImplementedException();
        }

    }

}