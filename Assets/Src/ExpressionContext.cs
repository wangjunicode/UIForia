using System.Collections.Generic;
using Rendering;

namespace Src {

    public class ExpressionContext {

        public object rootContext;
        protected List<Alias<int>> integerAliases;
        protected List<Alias<object>> objectAliases;

        public ExpressionContext(object rootContext) {
            this.rootContext = rootContext;
            this.integerAliases = new List<Alias<int>>();
            this.objectAliases = new List<Alias<object>>();
            this.objectAliases.Add(new Alias<object>("this", rootContext));
        }

        public void SetObjectAlias(string name, object target) {
            this.objectAliases.Add(new Alias<object>(name, target));
        }

        public void SetIntAlias(string name, int value) {
            integerAliases.Add(new Alias<int>(name, value));
        }

        public void RemoveObjectAlias(string alias) {
            for (int i = 0; i < objectAliases.Count; i++) {
                if (objectAliases[i].name == alias) {
                    objectAliases.RemoveAt(i);
                    return;
                }
            }
        }
        
        public void RemoveIntAlias(string alias) {
            for (int i = 0; i < integerAliases.Count; i++) {
                if (integerAliases[i].name == alias) {
                    integerAliases.RemoveAt(i);
                    return;
                }
            }
        }

        public int ResolveIntAlias(string alias) {
            for (int i = 0; i < integerAliases.Count; i++) {
                if (integerAliases[i].name == alias) {
                    return integerAliases[i].value;
                }
            }
            return int.MaxValue;
        }

        public object ResolveObjectAlias(string alias) {
            if (alias[0] != '$') {
                return rootContext;
            }

            for (int i = 0; i < objectAliases.Count; i++) {
                if (objectAliases[i].name == alias) {
                    return objectAliases[i].value;
                }
            }

            return null;
        }

        protected struct Alias<T> {

            public readonly string name;
            public readonly T value;

            public Alias(string name, T value) {
                this.name = name;
                this.value = value;
            }

        }

    }

    public class UITemplateContext : ExpressionContext {

        public readonly UIView view;

        public UITemplateContext(UIView view) : base(null) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) rootContext; }
            set { rootContext = value; }
        }

    }

}