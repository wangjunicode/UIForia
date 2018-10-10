using System;
using System.Collections;
using System.Collections.Generic;
using Src.Extensions;
using Src.Util;

namespace Src {

    public interface IExpressionContextProvider {

        int UniqueId { get; }
        IExpressionContextProvider ExpressionParent { get; }

    }

    public class ExpressionContext {

        private Dictionary<string, List<ValueTuple<Type, IList>>> aliasMap;

        public IExpressionContextProvider rootContext;
        public IExpressionContextProvider current;

        public ExpressionContext(IExpressionContextProvider rootContext) {
            this.rootContext = rootContext;
            this.current = rootContext;
        }

        public void SetContextValue<T>(string alias, T value) {
            SetContextValue(rootContext, alias, value);
        }

        private List<ValueTuple<int, T>> GetList<T>(string alias, bool create = false) {
            List<ValueTuple<Type, IList>> list;
            if (aliasMap.TryGetValue(alias, out list)) {
                for (int i = 0; i < list.Count; i++) {
                    if (list[i].Item1 == typeof(T)) {
                        return (List<ValueTuple<int, T>>) list[i].Item2;
                    }
                }
                if (!create) return null;
                List<ValueTuple<int, T>> retn = ListPool<ValueTuple<int, T>>.Get();
                list.Add(ValueTuple.Create(typeof(T), (IList) retn));
                return retn;
            }
            else {
                if (!create) return null;
                list = new List<ValueTuple<Type, IList>>();
                aliasMap[alias] = list;
                List<ValueTuple<int, T>> retn = ListPool<ValueTuple<int, T>>.Get();
                list.Add(ValueTuple.Create(typeof(T), (IList) retn));
                return retn;
            }
           
        }

        public void SetContextValue<T>(IExpressionContextProvider provider, string alias, T value) {
            if (aliasMap == null) {
                aliasMap = new Dictionary<string, List<ValueTuple<Type, IList>>>();
            }

            int id = provider.UniqueId;
            ValueTuple<int, T> tuple = ValueTuple.Create(id, value);

            List<ValueTuple<int, T>> valueList = GetList<T>(alias, true);
            
            for (int i = 0; i < valueList.Count; i++) {
                if (valueList[i].Item1 == id) {
                    valueList[i] = tuple;
                    return;
                }
            }
            valueList.Add(tuple);
        }

        public bool GetContextValue<T>(string alias, out T retn) {
            return GetContextValue(rootContext, alias, out retn);
        }

        public bool GetContextValue<T>(IExpressionContextProvider provider, string alias, out T retn) {
  
            if (aliasMap == null) {
                retn = default(T);
                return false;
            }

            // todo -- profile replacing dictionary w/ list
            List<ValueTuple<int, T>> valueList = GetList<T>(alias);
            if (valueList == null) {
                retn = default(T);
                return false;
            }

            IExpressionContextProvider ptr = provider;

            while (ptr != null) {
                int id = ptr.UniqueId;

                for (int i = 0; i < valueList.Count; i++) {
                    if (valueList[i].Item1 == id) {
                        retn = valueList[i].Item2;
                        return true;
                    }
                }

                ptr = ptr.ExpressionParent;
            }

            retn = default(T);
            return false;
        }

        public void RemoveContextValue<T>(IExpressionContextProvider provider, string alias, T unusedButRequired = default(T)) {
            if (aliasMap == null) {
                return;
            }

            List<ValueTuple<int, T>> valueList = GetList<T>(alias);
            if (valueList == null) {
                return;
            }

            int id = provider.UniqueId;

            for (int i = 0; i < valueList.Count; i++) {
                if (valueList[i].Item1 == id) {
                    valueList.UnstableRemove(i);
                    return;
                }
            }
        }

        public void RemoveContextValue<T>(string alias, T unusedButRequired = default(T)) {
            RemoveContextValue(rootContext, alias, unusedButRequired);
        }

    }

}