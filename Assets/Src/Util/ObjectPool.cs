using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.Util {

    public class ObjectPool<T> where T : class, new() {

        private readonly Stack<T> m_Stack;
        private readonly Action<T> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;

        public ObjectPool(Action<T> actionOnGet = null, Action<T> actionOnRelease = null) {
            this.m_ActionOnGet = actionOnGet;
            this.m_ActionOnRelease = actionOnRelease;
            this.m_Stack = new Stack<T>();
        }
        
        public int TotalCount { get; private set; }

        public int ActiveCount => TotalCount - InactiveCount;

        public int InactiveCount => m_Stack.Count;

        public T Get() {
            T element;
            if (m_Stack.Count == 0) {
                element = new T();
                TotalCount++;
            }
            else {
                element = m_Stack.Pop();
            }

            m_ActionOnGet?.Invoke(element);
            return element;
        }

        public void Release(T element) {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element)) {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }

            m_ActionOnRelease?.Invoke(element);
            m_Stack.Push(element);
        }

    }

}