using System.Collections;
using System.Collections.Generic;

namespace Src {

    public class ExpressionContext {

        // todo -- make this work off of alias sources also
        // todo -- potentially lots of boxing going on here
        
        public object rootContext;
        protected readonly List<ExpressionAlias<int>> integerAliases;
        protected readonly List<ExpressionAlias<object>> objectAliases;

        public ExpressionContext(object rootContext) {
            this.rootContext = rootContext;
            this.integerAliases = new List<ExpressionAlias<int>>();
            this.objectAliases = new List<ExpressionAlias<object>>();
        }

        public void SetObjectAlias(string name, object target) {
            for (int i = 0; i < objectAliases.Count; i++) {
                if (objectAliases[i].name == name) {
                    objectAliases[i] = new ExpressionAlias<object>(name, target);
                    return;
                }
            }
            this.objectAliases.Add(new ExpressionAlias<object>(name, target));
        }

        public void SetIntAlias(string name, int value) {
            for (int i = 0; i < integerAliases.Count; i++) {
                if (integerAliases[i].name == name) {
                    integerAliases[i] = new ExpressionAlias<int>(name, value);
                    return;
                }
            }
            integerAliases.Add(new ExpressionAlias<int>(name, value));
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

        public object ResolveRuntimeAlias(string alias) {
            // todo -- this will be changed
            object retn = ResolveObjectAlias(alias);
            if (retn != null) return retn;
            int intValue = ResolveIntAlias(alias);
            if (intValue != int.MaxValue) {
                return intValue;
            }
            return null;
        }

    }

    public class UITemplateContext : ExpressionContext {

        public readonly UIView view;
        public IList activelist;

        public UITemplateContext(UIView view) : base(null) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) rootContext; }
            set { rootContext = value; }
        }

    }

}