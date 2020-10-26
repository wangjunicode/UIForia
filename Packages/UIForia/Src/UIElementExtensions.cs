using System;

namespace UIForia.Elements {

    public static class UIElementExtensions {

        public static void VisitDescendents(this UIElement self, Action<UIElement> descendentFn) {

            for (int i = 0; i < self.children.size; i++) {
                descendentFn(self.children[i]);
                VisitDescendents(self.children[i], descendentFn);
            }

        }

        public static void VisitDescendentsConditionally(this UIElement self, Func<UIElement, bool> descendentFn) {

            for (int i = 0; i < self.children.size; i++) {
                if (descendentFn(self.children[i])) {
                    VisitDescendentsConditionally(self.children[i], descendentFn);
                }
            }

        }

    }

}