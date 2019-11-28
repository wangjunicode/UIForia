using System;
using UIForia.Elements;

namespace UIForia.Compilers {

    public abstract class EventProxy_Base {

        public UIElement root;
        public UIElement element;
            
    }

    public class EventProxy_Action : EventProxy_Base {

        public Action handler;

        public void HandleIt() {
            handler?.Invoke();
        }

    }
    
    public class EventProxy_Action<T> : EventProxy_Base {

        public Action<T> handler;
        
        public void HandleIt(T t) {
            handler?.Invoke(t);
        }
        
    }
    
    public class EventProxy_Action<T, U> : EventProxy_Base {

        public Action<T, U> handler;

    }
    
    public class EventProxy_Action<T, U, V> : EventProxy_Base {

        public Action<T, U, V> handler;

    }
    
    public class EventProxy_Action<T, U, V, W> : EventProxy_Base {

        public Action<T, U, V, W> handler;

    }
    
    public class EventProxy_Action<T, U, V, W, X> : EventProxy_Base {

        public Action<T, U, V, W, X> handler;

    }
    
    public class EventProxy_Action<T, U, V, W, X, Y> : EventProxy_Base {

        public Action<T, U, V, W, X, Y> handler;

    }
    
    public class EventProxy_Action<T, U, V, W, X, Y, Z> : EventProxy_Base {

        public Action<T, U, V, W, X, Y, Z> handler;

    }
    
    public class EventProxy_Func<T> : EventProxy_Base {

        public Func<T> handler;

    }
    
    public class EventProxy_Func<T, U> : EventProxy_Base {

        public Func<T, U> handler;

    }
    
    public class EventProxy_Func<T, U, V> : EventProxy_Base {

        public Func<T, U, V> handler;

    }
    
    public class EventProxy_Func<T, U, V, W> : EventProxy_Base {

        public Func<T, U, V, W> handler;

    }
    
    public class EventProxy_Func<T, U, V, W, X> : EventProxy_Base {

        public Func<T, U, V, W, X> handler;

    }
    
    public class EventProxy_Func<T, U, V, W, X, Y> : EventProxy_Base {

        public Func<T, U, V, W, X, Y> handler;

    }
    
    public class EventProxy_Func<T, U, V, W, X, Y, Z> : EventProxy_Base {

        public Func<T, U, V, W, X, Y, Z> handler;

    }

}